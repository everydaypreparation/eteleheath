using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Notifications;
using Abp.Runtime.Security;
using Abp.UI;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common.AzureBlobStorage;
using EMRO.Common.AzureBlobStorage.Dto;
using EMRO.Common.CustomNotification;
using EMRO.Documents.Dtos;
using EMRO.Sessions;
using EMRO.UserConsents;
using EMRO.UserDocuments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EMRO.Documents
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class DocumentAppService : ApplicationService, IDocumentAppService
    {

        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private readonly IRepository<UserDocument, Guid> _userDocumentRepository;
        private readonly IRepository<UserConsent, Guid> _userconsentRepository;
        private readonly EmroAppSession _session;
        private readonly IBlobContainer _blobContainer;
        private readonly IRepository<User, long> _userRepository;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        private readonly IAuditReportAppService _auditReportAppService;
        public DocumentAppService(IConfiguration configuration,
            IRepository<UserDocument, Guid> userDocumentRepository,
            IWebHostEnvironment env,
            IOptions<UplodedFilePath> uplodedFilePath,
             IRepository<UserConsent, Guid> userconsentRepository
            , EmroAppSession session
            , IBlobContainer blobContainer
            , IRepository<User, long> userRepository
            , IDoctorAppointmentRepository doctorAppointmentRepository
            //, INotificationPublisher notificationPublisher
            , ICustomNotificationAppService customNotificationAppService
            , IAuditReportAppService auditReportAppService
            )
        {
            _configuration = configuration;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            _userDocumentRepository = userDocumentRepository;
            _userconsentRepository = userconsentRepository;
            _session = session;
            _blobContainer = blobContainer;
            _userRepository = userRepository;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
            _auditReportAppService = auditReportAppService;
        }

        /// <summary>
        /// User can upload our own document
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DocumentOutput> Upload([FromForm] CreateDocumentInput input)
        {
            var userDocument = new UserDocument();
            DocumentOutput output = new DocumentOutput();
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string DocumentfileExt = string.Empty;
            bool DocumentEncryptFileStatus = false;
            string DocumentEncryptedFilePath = string.Empty;
            string upfilePath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            //long result = 0;
            try
            {
                Guid appid = Guid.Empty;
                var stopwatch = Stopwatch.StartNew();
                if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                {
                    var consent = await _userconsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppointmentId);
                    if (consent != null)
                    {
                        appid = consent.Id;
                    }
                }
                if (input.UserId != null && input.UserId != Guid.Empty)
                {
                    if (input.ReportDocumentPaths != null && input.ReportDocumentPaths.Count > 0)
                    {
                        foreach (IFormFile item in input.ReportDocumentPaths)
                        {
                            Guid updobj = Guid.NewGuid();
                            //var userDocument = new UserDocument();
                            DocumentfileExt = Path.GetExtension(item.FileName);
                            string titlename = Path.GetFileName(item.FileName);
                            string mimetype = item.ContentType;
                            string upfilename = updobj.ToString() + DocumentfileExt;
                            if (_uplodedFilePath.IsBlob)
                            {
                                upfilePath = Path.Combine(input.UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobDocumentPath + input.Category + _uplodedFilePath.Slash);
                                upfilePath = upfilePath + upfilename;
                                uploadDocumentInput.azureDirectoryPath = upfilePath;
                                uploadDocumentInput.Document = item;
                                var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                                if (blobResponse != null)
                                {
                                    userDocument.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                                    userDocument.VersionId = blobResponse.Value.VersionId;
                                    userDocument.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                                    userDocument.EncryptionScope = blobResponse.Value.EncryptionScope;
                                    userDocument.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;

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
                                upfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.DocumentPath + input.UserId.ToString() + _uplodedFilePath.Slash + input.Category + _uplodedFilePath.Slash);

                                if (!Directory.Exists(upfilePath))
                                {
                                    DirectoryInfo directoryInfo = Directory.CreateDirectory(upfilePath);
                                }
                                DocumentEncryptedFilePath = upfilePath + "Enc_" + upfilename;
                                upfilePath = upfilePath + upfilename;

                                using (var stream = new FileStream(upfilePath, FileMode.Create, FileAccess.Write))
                                {
                                    await item.CopyToAsync(stream);
                                    stream.Flush();

                                }
                                DocumentEncryptFileStatus = Utility.EncryptFile(upfilePath, DocumentEncryptedFilePath, EncryptedKey);
                                if (DocumentEncryptFileStatus)
                                {

                                    if (System.IO.File.Exists(upfilePath))
                                    {
                                        System.IO.File.Delete(upfilePath);
                                    }
                                }
                            }
                            userDocument.Category = input.Category;
                            userDocument.MimeType = mimetype;
                            userDocument.Title = titlename;
                            userDocument.IsActive = true;
                            userDocument.Status = "Uploaded";
                            userDocument.CreatedOn = DateTime.UtcNow;
                            userDocument.UserId = input.UserId;
                            userDocument.IsBlobStorage = _uplodedFilePath.IsBlob;


                            if (_uplodedFilePath.IsBlob == false)
                            {
                                userDocument.Path = DocumentEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim();
                            }
                            else
                            {
                                userDocument.Path = upfilePath.Trim();
                            }

                            if (appid != null)
                            {
                                userDocument.UserConsentsId = appid;

                            }
                            if (_session.UniqueUserId != null)
                            {
                                userDocument.CreatedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            userDocument.TenantId = AbpSession.TenantId;
                            Guid result = await _userDocumentRepository.InsertAndGetIdAsync(userDocument);

                        }
                        // output.DocumentId = result;
                        output.Message = "File uploaded successfully.";
                        output.StatusCode = 200;

                        stopwatch.Stop();

                        //Notification
                        if (_session.UniqueUserId != null)
                        {
                            var uploadedUser = _userRepository.FirstOrDefault(x => x.UniqueUserId == Guid.Parse(_session.UniqueUserId));

                            //Log Audit Events
                            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                            createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                            createEventsInput.Parameters = Utility.Serialize(userDocument);
                            createEventsInput.Operation = uploadedUser.FullName + " has uploaded File successfully.";
                            createEventsInput.Component = "Document";
                            createEventsInput.Action = "Upload";

                            await _auditReportAppService.CreateAuditEvents(createEventsInput);

                            if (uploadedUser.UserType == "Consultant")
                            {
                                if(input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                                {
                                    var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId);
                                    var  user = _userRepository.FirstOrDefault(x => x.UniqueUserId == appointment.UserId);
                                    if(user.UserType == "MedicalLegal" || user.UserType == "Insurance")
                                    {
                                        string Title = "Upload Review for Legal case";
                                        string Message = uploadedUser.FullName + " uploaded review for legal case.";
                                        _customNotificationAppService.NotificationPublish(user.Id, Title, Message);
                                    }
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        output.Message = "Bad Request.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
                Logger.Info("Create  Documents for input: " + input);

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Documents Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Delete Document
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        public async Task<DocumentOutput> Delete(Guid DocumentId)
        {
            DocumentOutput output = new DocumentOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (DocumentId != null && DocumentId != Guid.Empty)
                {
                    var data = await _userDocumentRepository.GetAsync(DocumentId);
                    if (data != null)
                    {
                        data.IsActive = false;
                        data.UpdatedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            data.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        var result = await _userDocumentRepository.UpdateAsync(data);
                        output.Message = "File deleted successfully.";
                        output.StatusCode = 200;

                        stopwatch.Stop();
                        var uploadedUser = _userRepository.FirstOrDefault(x => x.UniqueUserId == Guid.Parse(_session.UniqueUserId));

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(data);
                        createEventsInput.Operation = uploadedUser.FullName + " has deleted File successfully.";
                        createEventsInput.Component = "Document";
                        createEventsInput.Action = "Delete";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
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
                Logger.Error("Delete Documents Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Get document by document Id
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        public async Task<GetDocumentsOutput> Get(Guid DocumentId)
        {
            GetDocumentsOutput output = new GetDocumentsOutput();
            try
            {
                if (DocumentId != null && DocumentId != Guid.Empty)
                {
                    var data = await _userDocumentRepository.GetAsync(DocumentId);
                    if (data != null)
                    {
                        string path = _env.ContentRootPath + data.Path;
                        string DocumentfileExt = Path.GetExtension(path);
                        if (data.IsBlobStorage)
                        {
                            output.Filedata = _blobContainer.Download(data.Path);
                        }
                        else
                        {
                            byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                            output.Filedata = Convert.ToBase64String(b);
                        }

                        output.MimeType = data.MimeType;
                        output.DocumentId = data.Id;
                        output.Category = data.Category;
                        output.DocumentName = data.Title;
                        output.Message = "Get file successfully.";
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

        /// <summary>
        /// Re upload document (edit document funcation)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DocumentOutput> ReUpload([FromForm] CreateDocumentInput input)
        {
            DocumentOutput output = new DocumentOutput();
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string DocumentfileExt = string.Empty;
            bool DocumentEncryptFileStatus = false;
            string DocumentEncryptedFilePath = string.Empty;
            string upfilePath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            try
            {
                if (input.UserId != null && input.DocumentId != Guid.Empty && input.DocumentId != null && input.UserId != Guid.Empty)
                {

                    if (input.ReportDocumentPaths != null && input.ReportDocumentPaths.Count > 0)
                    {
                        var data = await _userDocumentRepository.GetAsync(input.DocumentId);
                        if (data != null)
                        {
                            string path = _env.ContentRootPath + data.Path;
                            if (File.Exists(path))
                            {
                                // If file found, delete it    
                                File.Delete(path);
                            }

                          //  bool isdeleted = _blobContainer.Delete(data.Path);
                            foreach (IFormFile item in input.ReportDocumentPaths)
                            {
                                Guid updobj = Guid.NewGuid();

                                DocumentfileExt = Path.GetExtension(item.FileName);
                                string titlename = Path.GetFileName(item.FileName);
                                string mimetype = item.ContentType;
                                string upfilename = updobj.ToString() + DocumentfileExt;
                                if (_uplodedFilePath.IsBlob)
                                {
                                    upfilePath = Path.Combine(input.UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobDocumentPath + input.Category + _uplodedFilePath.Slash);
                                    upfilePath = upfilePath + upfilename;
                                    uploadDocumentInput.azureDirectoryPath = upfilePath;
                                    uploadDocumentInput.Document = item;
                                    var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                                    if (blobResponse != null)
                                    {

                                        data.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                                        data.VersionId = blobResponse.Value.VersionId;
                                        data.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                                        data.EncryptionScope = blobResponse.Value.EncryptionScope;
                                        data.Path = upfilePath.Trim();
                                        data.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                                    }
                                }
                                else
                                {
                                    upfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.DocumentPath + input.UserId.ToString() + _uplodedFilePath.Slash + data.Category + _uplodedFilePath.Slash);
                                    if (!Directory.Exists(upfilePath))
                                    {
                                        DirectoryInfo directoryInfo = Directory.CreateDirectory(upfilePath);
                                    }
                                    DocumentEncryptedFilePath = upfilePath + "Enc_" + upfilename;
                                    upfilePath = upfilePath + upfilename;

                                    using (var stream = new FileStream(upfilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        await item.CopyToAsync(stream);
                                        stream.Flush();

                                    }
                                    DocumentEncryptFileStatus = Utility.EncryptFile(upfilePath, DocumentEncryptedFilePath, EncryptedKey);
                                    if (DocumentEncryptFileStatus)
                                    {

                                        if (System.IO.File.Exists(upfilePath))
                                        {
                                            System.IO.File.Delete(upfilePath);
                                        }
                                    }

                                    data.Path = DocumentEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim();
                                }


                                data.IsBlobStorage = _uplodedFilePath.IsBlob;
                                data.MimeType = mimetype;
                                data.Title = titlename;
                                data.UpdatedOn = DateTime.UtcNow;
                                data.UserId = input.UserId;
                                if (_session.UniqueUserId != null)
                                {
                                    data.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                if (input.AppointmentId != null)
                                {
                                    var consent = await _userconsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppointmentId);
                                    if (consent != null)
                                    {
                                        data.UserConsentsId = consent.Id;
                                    }
                                }
                                await _userDocumentRepository.UpdateAsync(data);

                            }
                            // output.DocumentId = result;
                            output.Message = "File reuploaded successfully.";
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
                        output.Message = "Please attached the documents.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
                Logger.Info("Create  Documents for input: " + input);

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Delete Documents Error:" + ex.StackTrace);
            }
            return output;
        }

    }
}
