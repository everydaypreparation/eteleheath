using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Notifications;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AzureBlobStorage;
using EMRO.Common.AzureBlobStorage.Dto;
using EMRO.Common.CustomNotification;
using EMRO.Common.Templates;
using EMRO.Email;
using EMRO.InternalMessages.Dto;
using EMRO.Sessions;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.InternalMessages
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class UserMessageAppService : ApplicationService, IUserMessageAppService
    {
        private readonly IRepository<UserMessages, Guid> _userMessageRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserMessagesAttachment, Guid> _userMessagesAttachmentRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        // private readonly UserManager _userManager;
        private IConfiguration _configuration;
        private readonly IMessageRepository _messageRepository;
        private readonly EmroAppSession _session;
        private readonly IMailer _mailer;
        private readonly IBlobContainer _blobContainer;
        private readonly TemplateAppService _templateAppService;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        public UserMessageAppService(IUnitOfWorkManager unitOfWorkManager,
            IRepository<UserMessages, Guid> userMessageRepository
            , IRepository<UserMessagesAttachment, Guid> userMessagesAttachmentRepository
            , IOptions<UplodedFilePath> uplodedFilePath, IWebHostEnvironment env
            , IConfiguration configuration,
            IMessageRepository messageRepository
            , EmroAppSession session
            , IRepository<User, long> userRepository
            , IMailer mailer
            , IBlobContainer blobContainer
            , TemplateAppService templateAppService
            //, INotificationPublisher notificationPublisher
            , ICustomNotificationAppService customNotificationAppService)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _userMessageRepository = userMessageRepository;
            _userMessagesAttachmentRepository = userMessagesAttachmentRepository;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            // _userManager = userManager;
            _configuration = configuration;
            _messageRepository = messageRepository;
            _session = session;
            _userRepository = userRepository;
            _mailer = mailer;
            _blobContainer = blobContainer;
            _templateAppService = templateAppService;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
        }

        /// <summary>
        /// Send message to multiple users
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OutputUserMessageDto> Send(CreateUserMessagesDto input)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();

            try
            {
                var msg = await _userMessageRepository.GetAsync(input.MessageId);
                bool isReply = input.ReceiverUserIds.Split(',').Contains(input.SenderUserIds);
                if (isReply == false)
                {
                    if (msg != null)
                    {
                        msg.Subject = input.Subject.Trim();
                        msg.MessagesText = input.MessagesText.Trim();
                        msg.SenderUserIds = input.SenderUserIds;
                        msg.ReceiverUserIds = input.ReceiverUserIds;
                        msg.IsDraft = false;
                        msg.ParentId = input.ParentId;
                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            msg.AppointmentId = input.AppointmentId;
                        }
                        await _userMessageRepository.UpdateAsync(msg);

                        //Notification to receiver
                        var splitReciverIds = input.ReceiverUserIds.Split(',');
                        var users = _userRepository.GetAll().Where(x => splitReciverIds.Contains(x.UniqueUserId.ToString())).ToList();
                        foreach (var item in users)
                        {
                            string Title = "ETeleHealth Cloud Message";
                            string Message = "There is a new message for you, Please check your message box on dashboard";
                            _customNotificationAppService.NotificationPublish(item.Id, Title, Message);
                        }

                    }
                    else
                    {
                        var userMessage = new UserMessages
                        {
                            Subject = input.Subject,
                            MessagesText = input.MessagesText,
                            SenderUserIds = input.SenderUserIds,
                            ReceiverUserIds = input.ReceiverUserIds,
                            IsDraft = false,
                            ParentId = input.ParentId,
                            CreatedOn = DateTime.UtcNow
                        };

                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            userMessage.AppointmentId = input.AppointmentId;
                        }

                        if (_session.UniqueUserId != null)
                        {
                            userMessage.CreatedBy = Guid.Parse(_session.UniqueUserId);
                            userMessage.UserId = Guid.Parse(_session.UniqueUserId);
                        }
                        userMessage.TenantId = AbpSession.TenantId;
                        Guid newMessage = await _userMessageRepository.InsertAndGetIdAsync(userMessage);
                        string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                        //var users = _userRepository.GetAll().Where(x => x.UniqueUserId.ToString().Contains(input.ReceiverUserIds)).ToList();
                        var splitReciverIds = input.ReceiverUserIds.Split(',');
                        var users = _userRepository.GetAll().Where(x => splitReciverIds.Contains(x.UniqueUserId.ToString())).ToList();
                        foreach (var item in users)
                        {
                            string Name = item.Name.ToPascalCase() + " " + item.Surname.ToPascalCase();
                            string message = "There is a new message for you, Please open your inbox in the portal for more details....";
                            string body = _templateAppService.GetTemplates(Name, message, "");

                            //string body = "Hello " + "<b>" + item.Name.ToPascalCase() + " " + item.Surname.ToPascalCase() + ",</b> <br /><br /> "
                            //         + "There is a new message for you, Please open your inbox in the portal for more details...."
                            //         + " <br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(item.EmailAddress, "ETeleHealth Cloud Message", body, adminmail));

                            //Send Notification to users
                            string Title = "ETeleHealth Cloud Message";
                            string Message = "There is a new message for you, Please check your message box on dashboard.";
                            _customNotificationAppService.NotificationPublish(item.Id, Title, Message);
                        }
                    }

                    outputUserMessageDto.Message = "Message sent successfully.";
                    outputUserMessageDto.StatusCode = 200;
                }
                else
                {
                    outputUserMessageDto.Message = "You can't send messages to yourself.";
                    outputUserMessageDto.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = ex.Message;
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("User Message create API" + ex.StackTrace);

            }
            return outputUserMessageDto;
        }

        /// <summary>
        /// Send samvaad email to multiple users
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OutputUserMessageDto> SendSamvaadEmail(List<SamvaadMeetingUserDetails> samvaadMeetingUserDetails, string samvaadLink)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();

            try
            {
                await Task.Run(() =>
                {
                    foreach (var samvaadMeetingUserDetail in samvaadMeetingUserDetails)
                    {
                        string name = samvaadMeetingUserDetail.fullName;
                        string message = "Please join the video call with ETeleHealth Admin by clicking the <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#" + samvaadLink + "'>link</a>.";
                        string mailBody = _templateAppService.GetTemplates(name, message, "");

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(samvaadMeetingUserDetail.emailId, "ETeleHealth Admin is waiting for you on  a video call", mailBody, ""));
                    }
                });

                outputUserMessageDto.Message = "Message sent successfully.";
                outputUserMessageDto.StatusCode = 200;
            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = ex.Message;
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("User Message create API" + ex.StackTrace);

            }
            return outputUserMessageDto;
        }

        /// <summary>
        /// Delete messages by sender,receiver,trash,draft
        /// when any user delete message from sent the  we get IsDeleteSender = true  
        /// when any user delete message from inbox the we get IsDeleteReceiver = true 
        /// when any user delete message from trash the we get IsDeleteTrash = true
        /// when any user delete message from draft the we get IsDeleteDraft = true
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OutputUserMessageDto> Delete(DeleteMessageInput input)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();
            try
            {
                if (!string.IsNullOrEmpty(input.Id))
                {
                    if (input.IsDeleteReceiver == true || input.IsDeleteSender == true || input.IsDeleteTrash == true || input.IsDeleteDraft == true)
                    {
                        string[] maillarray = input.Id.Split(",");
                        foreach (var item in maillarray)
                        {
                            Guid mailId = new Guid(item);
                            var msg = await _userMessageRepository.GetAsync(mailId);
                            if (msg != null)
                            {
                                if (input.IsDeleteDraft == true)
                                {
                                    await _userMessageRepository.DeleteAsync(msg);
                                }
                                else
                                {

                                    if (input.IsDeleteReceiver == true)
                                    {
                                        if (!string.IsNullOrEmpty(msg.DeletedbyReceiver))
                                        {
                                            msg.DeletedbyReceiver = msg.DeletedbyReceiver + "," + input.UserId.ToString();
                                        }
                                        else
                                        {
                                            msg.DeletedbyReceiver = input.UserId.ToString();
                                        }

                                        msg.DeletedOn = DateTime.UtcNow;
                                    }
                                    else if (input.IsDeleteSender == true)
                                    {
                                        msg.DeletedbySender = input.UserId.ToString();
                                        msg.DeletedOn = DateTime.UtcNow;
                                    }
                                    else if (input.IsDeleteTrash == true)
                                    {
                                        if (!string.IsNullOrEmpty(msg.DeletedFromTrash))
                                        {
                                            msg.DeletedFromTrash = msg.DeletedFromTrash + "," + input.UserId.ToString();
                                        }
                                        else
                                        {
                                            msg.DeletedFromTrash = input.UserId.ToString();
                                        }
                                        msg.DeletedOn = DateTime.UtcNow;
                                    }
                                    if (_session.UniqueUserId != null)
                                    {

                                        msg.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                    }

                                    await _userMessageRepository.UpdateAsync(msg);
                                }
                                outputUserMessageDto.Message = "Message deleted successfully.";
                                outputUserMessageDto.StatusCode = 200;
                            }
                            else
                            {
                                outputUserMessageDto.Message = "No record found.";
                                outputUserMessageDto.StatusCode = 401;
                            }
                        }
                    }
                    else
                    {
                        outputUserMessageDto.Message = "No action performed.";
                        outputUserMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    outputUserMessageDto.Message = "Please select messages.";
                    outputUserMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = ex.Message;
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("Delete API" + ex.StackTrace);
            }
            return outputUserMessageDto;
        }

        /// <summary>
        /// draft message we get isdraft = true
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OutputUserMessageDto> Draft(DraftMessageInput input)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();
            try
            {

                var userMessage = new UserMessages
                {
                    Subject = input.Subject.Trim(),
                    MessagesText = input.MessagesText.Trim(),
                    SenderUserIds = input.SenderUserIds,
                    ReceiverUserIds = input.ReceiverUserIds,
                    IsDraft = true,
                    ParentId = input.ParentId,
                    CreatedOn = DateTime.UtcNow
                };

                if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                {
                    userMessage.AppointmentId = input.AppointmentId;
                }

                if (_session.UniqueUserId != null)
                {

                    userMessage.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    userMessage.UserId = Guid.Parse(_session.UniqueUserId);
                }
                userMessage.TenantId = AbpSession.TenantId;
                Guid newMessage = await _userMessageRepository.InsertAndGetIdAsync(userMessage);
                outputUserMessageDto.MessageId = newMessage;
                outputUserMessageDto.Message = "Message draft successfully.";
                outputUserMessageDto.StatusCode = 200;
            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = ex.Message;
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("Draft API" + ex.StackTrace);
            }
            return outputUserMessageDto;
        }

        public async Task<UploadAttachmentDto> UploadAttachment([FromForm] UploadAttachmentInputDto input)
        {
            UploadAttachmentDto output = new UploadAttachmentDto();
            string AttachmentfileExt = string.Empty;
            string AttachmentFileName = string.Empty;
            bool AttachmentEncryptFileStatus = false;
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string upfilePath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            try
            {
                if (input.MessageId != null)
                {
                    if (input.AttachmentFile != null)
                    {
                        Guid updobj = Guid.NewGuid();
                        AttachmentfileExt = Path.GetExtension(input.AttachmentFile.FileName);
                        string upfilename = updobj.ToString() + AttachmentfileExt;
                        AttachmentFileName = Path.GetFileName(input.AttachmentFile.FileName);
                        string mimetype = input.AttachmentFile.ContentType;
                        var userAttachment = new UserMessagesAttachment();
                        // long Size = item.Length / 1048;
                        if (_uplodedFilePath.IsBlob)
                        {
                            upfilePath = Path.Combine(input.MessageId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobMailAttachment);
                            upfilePath = upfilePath + upfilename;
                            uploadDocumentInput.azureDirectoryPath = upfilePath;
                            uploadDocumentInput.Document = input.AttachmentFile;
                            var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                            if (blobResponse != null)
                            {
                                userAttachment.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                                userAttachment.VersionId = blobResponse.Value.VersionId;
                                userAttachment.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                                userAttachment.EncryptionScope = blobResponse.Value.EncryptionScope;
                                userAttachment.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                                userAttachment.FilePath = upfilePath;
                            }
                            else
                            {
                                output.Message = "Something went wrong, please try again.";
                                output.StatusCode = 401;
                                return output;
                            }
                        }
                        else
                        {
                            upfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.MailAttachment);

                            if (!Directory.Exists(upfilePath))
                            {
                                DirectoryInfo directoryInfo = Directory.CreateDirectory(upfilePath);
                            }
                            string AttachmentEncryptedFilePath = upfilePath + "Enc_" + upfilename;
                            upfilePath = upfilePath + upfilename;

                            using (var stream = new FileStream(upfilePath, FileMode.Create, FileAccess.Write))
                            {
                                await input.AttachmentFile.CopyToAsync(stream);
                                stream.Flush();

                            }
                            AttachmentEncryptFileStatus = Utility.EncryptFile(upfilePath, AttachmentEncryptedFilePath, EncryptedKey);
                            if (AttachmentEncryptFileStatus)
                            {

                                if (System.IO.File.Exists(upfilePath))
                                {
                                    System.IO.File.Delete(upfilePath);
                                }
                            }
                            userAttachment.FilePath = AttachmentEncryptedFilePath.Replace(_env.ContentRootPath, "");
                        }

                        userAttachment.AttachmentName = AttachmentFileName;
                        userAttachment.FileSize = input.FileSize;

                        userAttachment.CreatedOn = DateTime.UtcNow;
                        userAttachment.UserMessagesId = input.MessageId;
                        userAttachment.Mimetype = mimetype;

                        if (_session.UniqueUserId != null)
                        {

                            userAttachment.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        userAttachment.TenantId = AbpSession.TenantId;
                        var newId = await _userMessagesAttachmentRepository.InsertAndGetIdAsync(userAttachment);
                        output.AttachmentId = newId;
                        output.Message = "Attachment added successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No attachment found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "No message found with id 0.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.StatusCode = 500;
                Logger.Error("UploadAttachment API" + ex.StackTrace);
            }
            return output;
        }

        public async Task<UploadAttachmentDto> DeleteAttachment(Guid AttachmentId)
        {
            UploadAttachmentDto output = new UploadAttachmentDto();
            try
            {
                if (AttachmentId != null)
                {
                    var attachment = await _userMessagesAttachmentRepository.GetAsync(AttachmentId);
                    if (attachment != null)
                    {
                        var deleterecord = _userMessagesAttachmentRepository.DeleteAsync(attachment);
                        string filepath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + attachment.FilePath);
                        if (File.Exists(filepath))
                        {
                            // If file found, delete it    
                            File.Delete(filepath);
                        }
                        output.Message = "Attachment deleted successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "No attacment found with id 0.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.StatusCode = 500;
                Logger.Error("UploadAttachment API" + ex.StackTrace);
            }
            return output;
        }

        public async Task<OutputUserMessageDto> ReadBy(ReadByInputDto input)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();
            try
            {
                var msg = await _userMessageRepository.GetAsync(input.MessageId);
                if (msg != null)
                {
                    if (!string.IsNullOrEmpty(msg.ReadBy))
                    {
                        msg.ReadBy = msg.ReadBy + "," + input.UserId.ToString();
                    }
                    else
                    {
                        msg.ReadBy = input.UserId.ToString();
                    }
                    msg.UpdatedOn = DateTime.UtcNow;
                    if (_session.UniqueUserId != null)
                    {

                        msg.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }

                    await _userMessageRepository.UpdateAsync(msg);
                    outputUserMessageDto.Message = "Message readed successfully.";
                    outputUserMessageDto.StatusCode = 200;
                }
                else
                {
                    outputUserMessageDto.Message = "No record found.";
                    outputUserMessageDto.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = ex.Message;
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("Read API" + ex.StackTrace);
            }
            return outputUserMessageDto;
        }

        public async Task<OutputUserMessageDto> Restore(RestoreInputDto input)
        {
            OutputUserMessageDto outputUserMessageDto = new OutputUserMessageDto();
            try
            {
                if (!string.IsNullOrEmpty(input.MessageId.Trim()))
                {
                    if (input.UserId != Guid.Empty)
                    {
                        string[] mailIdsArray = input.MessageId.Split(",");
                        //long[] mailIdsArray = (long[])input.MessageId.Split(",").Select(x => long.Parse(x));
                        if (input.MessageId.Contains(","))
                        {
                            outputUserMessageDto.Message = "Your messages have been restored.";
                        }
                        else
                        {
                            outputUserMessageDto.Message = "Your message has been restored";
                        }
                        foreach (var item in mailIdsArray)
                        {
                            Guid mailId = new Guid(item);
                            var msg = await _userMessageRepository.GetAsync(mailId);
                            if (msg != null)
                            {
                                if (!string.IsNullOrEmpty(msg.DeletedbySender))
                                {
                                    if (msg.DeletedbySender == input.UserId.ToString())
                                    {
                                        msg.DeletedbySender = null;
                                    }

                                }

                                if (!string.IsNullOrEmpty(msg.DeletedbyReceiver))
                                {
                                    if (msg.DeletedbyReceiver.Contains(","))
                                    {
                                        string[] arr = msg.DeletedbyReceiver.Split(",");
                                        Guid[] receiverarray = Array.ConvertAll(arr, s => Guid.Parse(s));
                                        if (receiverarray.Contains(input.UserId))
                                        {
                                            receiverarray = receiverarray.Where(val => val != input.UserId).ToArray();
                                            msg.DeletedbyReceiver = String.Join(",", receiverarray);
                                        }
                                    }
                                    else
                                    {
                                        if (msg.DeletedbyReceiver == input.UserId.ToString())
                                        {
                                            msg.DeletedbyReceiver = null;
                                        }
                                    }
                                }

                                msg.UpdatedOn = DateTime.UtcNow;
                                if (_session.UniqueUserId != null)
                                {

                                    msg.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }

                                await _userMessageRepository.UpdateAsync(msg);
                            }
                        }
                        outputUserMessageDto.StatusCode = 200;
                    }
                    else
                    {
                        outputUserMessageDto.Message = "Bad request.";
                        outputUserMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    outputUserMessageDto.Message = "Please select messages.";
                    outputUserMessageDto.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                outputUserMessageDto.Message = "Your messages could not be restored";
                outputUserMessageDto.StatusCode = 500;
                Logger.Error("Restore API" + ex.StackTrace);
            }
            return outputUserMessageDto;
        }

        public async Task<GetMessageDto> GetById(ReadByInputDto input)
        {
            GetMessageDto getMessageDto = new GetMessageDto();
            try
            {
                string[] arr;
                Guid[] receiverarray;
                if (input.MessageId != null && input.MessageId != Guid.Empty && input.UserId != Guid.Empty && input.UserId != null)
                {
                    var msg = await _userMessageRepository.GetAsync(input.MessageId);
                    if (msg != null)
                    {
                        if (!string.IsNullOrEmpty(msg.ReceiverUserIds) && !string.IsNullOrEmpty(msg.SenderUserIds))
                        {
                            arr = msg.ReceiverUserIds.Split(",");
                            receiverarray = Array.ConvertAll(arr, s => Guid.Parse(s));

                            //if (!string.IsNullOrEmpty(msg.DeletedbySender))
                            //{
                            //    if (Convert.ToInt64(msg.DeletedbySender) == input.UserId)
                            //    {
                            //        getMessageDto.Message = "No record found.";
                            //        getMessageDto.StatusCode = 401;
                            //        return getMessageDto;
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(msg.DeletedbyReceiver))
                            //{
                            //    string[] deletearr = msg.DeletedbyReceiver.Split(",");
                            //    long[] deletereceiverarr = Array.ConvertAll(deletearr, s => long.Parse(s));
                            //    if (deletereceiverarr.Contains(input.UserId))
                            //    {
                            //        getMessageDto.Message = "No record found.";
                            //        getMessageDto.StatusCode = 401;
                            //        return getMessageDto;
                            //    }
                            //}

                            if (msg.SenderUserIds == input.UserId.ToString() || receiverarray.Contains(input.UserId))
                            {
                                var sendermail = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == Guid.Parse(msg.SenderUserIds));
                                string reciverEmailId = string.Empty;
                                foreach (var item in receiverarray)
                                {
                                    var recevier = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item);
                                    if (recevier != null)
                                    {
                                        if (string.IsNullOrEmpty(reciverEmailId))
                                        {
                                            reciverEmailId = recevier.FullName;
                                        }
                                        else
                                        {
                                            reciverEmailId = reciverEmailId + "," + recevier.FullName;
                                        }
                                    }

                                }
                                getMessageDto.SenderUserIds = sendermail.FullName;
                                getMessageDto.ReceiverUserIds = reciverEmailId;
                                getMessageDto.Subject = msg.Subject;
                                getMessageDto.MessagesText = msg.MessagesText;
                                getMessageDto.MessageId = msg.Id;
                                getMessageDto.AppointmentId = msg.AppointmentId;
                                getMessageDto.MailDateTime = Convert.ToDateTime(msg.CreatedOn).ToString("MMM dd, yyyy hh: mm tt");
                                getMessageDto.Attachments = _userMessagesAttachmentRepository.GetAll()
                                    .Where(x => x.UserMessagesId == msg.Id)
                                    .Select(x => new AttachmentDto
                                    {
                                        AttachmentId = x.Id,
                                        AttachmentName = x.AttachmentName
                                    }).ToList();
                                getMessageDto.Message = "Get message successfully.";
                                getMessageDto.StatusCode = 200;
                            }
                            else
                            {
                                getMessageDto.Message = "No record found.";
                                getMessageDto.StatusCode = 401;
                            }

                        }
                        else
                        {
                            getMessageDto.Message = "No record found.";
                            getMessageDto.StatusCode = 401;
                        }
                    }
                    else
                    {
                        getMessageDto.Message = "No record found.";
                        getMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message GetById API" + ex.StackTrace);
            }
            return getMessageDto;
        }

        public async Task<GetMessageListOutput> IndoxAsync(GetMessageInput input)
        {
            GetMessageListOutput getMessageDto = new GetMessageListOutput();
            try
            {
                if (input.UserId != Guid.Empty && input.UserId != null)
                {
                    var query = await _messageRepository.GetInboxAsync(input.UserId.ToString());
                    getMessageDto.Count = query.Count;
                    if (query.Count > 0)
                    {
                        getMessageDto.Items = (from q in query.AsEnumerable()
                                               select new GetMessageListDto
                                               {
                                                   MessageId = q.MessageId,
                                                   Subject = string.IsNullOrEmpty(q.Subject) ? "(no subject)" : q.Subject,
                                                   MessagesText = q.MessagesText,
                                                   FirstName = q.FirstName,
                                                   LastName = q.LastName,
                                                   SenderUserIds = q.SenderUserIds,
                                                   ReceiverUserIds = q.ReceiverUserIds,
                                                   MailDateTime = q.MailDateTime,
                                                   EmailId = q.EmailId,
                                                   ReadBy = q.ReadBy,
                                                   Attachments = _userMessagesAttachmentRepository.GetAll().Where(x => x.UserMessagesId == q.MessageId).Select(x => new AttachmentDto
                                                   {
                                                       AttachmentId = x.Id,
                                                       AttachmentName = x.AttachmentName
                                                   }).ToList()
                                               }).ToList();
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0)
                        {
                            getMessageDto.Items = getMessageDto.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }
                        getMessageDto.Message = "Get inbox successfully.";
                        getMessageDto.StatusCode = 200;
                    }
                    else
                    {
                        getMessageDto.Message = "No record found.";
                        getMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message Indox API" + ex.StackTrace);
            }
            return getMessageDto;
        }

        public async Task<GetMessageListOutput> Sent(GetMessageInput input)
        {
            GetMessageListOutput getMessageDto = new GetMessageListOutput();
            try
            {
                if (input.UserId != Guid.Empty && input.UserId != null)
                {
                    var query = await _messageRepository.GetSentAsync(input.UserId.ToString());
                    getMessageDto.Count = query.Count;
                    if (query.Count > 0)
                    {
                        getMessageDto.Items = (from q in query.AsEnumerable()
                                               select new GetMessageListDto
                                               {
                                                   MessageId = q.MessageId,
                                                   Subject = string.IsNullOrEmpty(q.Subject) ? "(no subject)" : q.Subject,
                                                   MessagesText = q.MessagesText,
                                                   FirstName = q.FirstName,
                                                   LastName = q.LastName,
                                                   SenderUserIds = q.SenderUserIds,
                                                   ReceiverUserIds = q.ReceiverUserIds,
                                                   MailDateTime = q.MailDateTime,
                                                   EmailId = q.EmailId,
                                                   Attachments = _userMessagesAttachmentRepository.GetAll().Where(x => x.UserMessagesId == q.MessageId).Select(x => new AttachmentDto
                                                   {
                                                       AttachmentId = x.Id,
                                                       AttachmentName = x.AttachmentName
                                                   }).ToList()
                                               }).ToList();
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0)
                        {
                            getMessageDto.Items = getMessageDto.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }
                        getMessageDto.Message = "Get sent successfully.";
                        getMessageDto.StatusCode = 200;
                    }
                    else
                    {
                        getMessageDto.Message = "No record found.";
                        getMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message Indox API" + ex.StackTrace);
            }
            return getMessageDto;
        }

        public async Task<GetMessageListOutput> Thrash(GetMessageInput input)
        {
            GetMessageListOutput getMessageDto = new GetMessageListOutput();
            try
            {
                if (input.UserId != Guid.Empty && input.UserId != null)
                {
                    var query = await _messageRepository.GetTrashAsync(input.UserId.ToString());
                    getMessageDto.Count = query.Count;
                    if (query.Count > 0)
                    {
                        getMessageDto.Items = (from q in query.AsEnumerable()
                                               select new GetMessageListDto
                                               {
                                                   MessageId = q.MessageId,
                                                   Subject = string.IsNullOrEmpty(q.Subject) ? "(no subject)" : q.Subject,
                                                   MessagesText = q.MessagesText,
                                                   FirstName = q.FirstName,
                                                   LastName = q.LastName,
                                                   SenderUserIds = q.SenderUserIds,
                                                   ReceiverUserIds = q.ReceiverUserIds,
                                                   MailDateTime = q.MailDateTime,
                                                   EmailId = q.EmailId,
                                                   Attachments = _userMessagesAttachmentRepository.GetAll().Where(x => x.UserMessagesId == q.MessageId).Select(x => new AttachmentDto
                                                   {
                                                       AttachmentId = x.Id,
                                                       AttachmentName = x.AttachmentName
                                                   }).ToList()
                                               }).ToList();
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0)
                        {
                            getMessageDto.Items = getMessageDto.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }
                        getMessageDto.Message = "Get trash successfully.";
                        getMessageDto.StatusCode = 200;
                    }
                    else
                    {
                        getMessageDto.Message = "No record found.";
                        getMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message Indox API" + ex.StackTrace);
            }
            return getMessageDto;
        }

        public GetMessageListOutput Drafts(GetMessageInput input)
        {
            GetMessageListOutput getMessageDto = new GetMessageListOutput();
            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty)
                {
                    var query = _userMessageRepository.GetAllIncluding(x => x.Users).Where(x => x.IsDraft == true && x.SenderUserIds == input.UserId.ToString()).ToList();
                    getMessageDto.Count = query.Count;
                    getMessageDto.Items = (from u in query.AsEnumerable()
                                           select new GetMessageListDto
                                           {
                                               MessageId = u.Id,
                                               Subject = string.IsNullOrEmpty(u.Subject) ? "(no subject)" : u.Subject,
                                               MessagesText = u.MessagesText,
                                               FirstName = u.Users.Name,
                                               LastName = u.Users.Surname,
                                               SenderUserIds = u.SenderUserIds,
                                               ReceiverUserIds = u.ReceiverUserIds,
                                               MailDateTime = Convert.ToDateTime(u.CreatedOn).ToString(),
                                               EmailId = u.Users.EmailAddress,
                                               Attachments = _userMessagesAttachmentRepository.GetAll().Where(x => x.UserMessagesId == u.Id).Select(x => new AttachmentDto
                                               {
                                                   AttachmentId = x.Id,
                                                   AttachmentName = x.AttachmentName
                                               }).ToList()
                                           }).ToList();
                    if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0)
                    {
                        getMessageDto.Items = getMessageDto.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message Indox API" + ex.StackTrace);
            }
            return getMessageDto;
        }

        public async Task<GetAttachments> GetDownloadAttachment(Guid AttachmentId)
        {
            GetAttachments output = new GetAttachments();
            try
            {
                if (AttachmentId != null && AttachmentId != Guid.Empty)
                {
                    var data = await _userMessagesAttachmentRepository.GetAsync(AttachmentId);
                    if (data != null)
                    {
                        if (data.IsBlobStorage)
                        {
                            output.Filedate = _blobContainer.Download(data.FilePath);
                        }
                        else
                        {
                            string path = _env.ContentRootPath + data.FilePath;
                            byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                            output.Filedate = Convert.ToBase64String(b);
                        }

                        output.MimeType = data.Mimetype;
                        output.AttachmentId = data.Id;
                        output.Message = "get file successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get Documents Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetMessageListOutput> GetUserMessages(GetUsersMessageInput input)
        {
            GetMessageListOutput getMessageDto = new GetMessageListOutput();
            try
            {
                if (!string.IsNullOrEmpty(input.FromUserId) && !string.IsNullOrEmpty(input.FromUserId))
                {
                    var query = await _messageRepository.GetMessagesAsync(input.FromUserId, input.ToUserId);

                    if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                    {
                        query = query.Where(x => x.AppointmentId == input.AppointmentId).ToList();
                    }
                    getMessageDto.Count = query.Count;
                    if (query.Count > 0)
                    {
                        getMessageDto.Items = (from q in query.AsEnumerable()
                                               select new GetMessageListDto
                                               {
                                                   MessageId = q.MessageId,
                                                   Subject = string.IsNullOrEmpty(q.Subject) ? "(no subject)" : q.Subject,
                                                   MessagesText = q.MessagesText,
                                                   FirstName = q.FirstName,
                                                   LastName = q.LastName,
                                                   SenderUserIds = q.SenderUserIds,
                                                   ReceiverUserIds = q.ReceiverUserIds,
                                                   MailDateTime = q.MailDateTime,
                                                   EmailId = q.EmailId,
                                                   ReadBy = q.ReadBy,
                                                   AppointmentId = q.AppointmentId,
                                                   Attachments = _userMessagesAttachmentRepository.GetAll().Where(x => x.UserMessagesId == q.MessageId).Select(x => new AttachmentDto
                                                   {
                                                       AttachmentId = x.Id,
                                                       AttachmentName = x.AttachmentName
                                                   }).ToList()
                                               }).ToList();
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0)
                        {
                            getMessageDto.Items = getMessageDto.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }
                        getMessageDto.Message = "Get messages successfully.";
                        getMessageDto.StatusCode = 200;
                    }
                    else
                    {
                        getMessageDto.Message = "No record found.";
                        getMessageDto.StatusCode = 401;
                    }
                }
                else
                {
                    getMessageDto.Message = "Bad Request.";
                    getMessageDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getMessageDto.Message = "Something went wrong, please try again.";
                getMessageDto.StatusCode = 500;
                Logger.Error("Message Indox API" + ex.StackTrace);
            }
            return getMessageDto;
        }
    }
}
