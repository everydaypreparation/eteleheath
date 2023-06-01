using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using DinkToPdf;
using DinkToPdf.Contracts;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AzureBlobStorage;
using EMRO.Common.AzureBlobStorage.Dto;
using EMRO.Common.CustomNotification;
using EMRO.Common.Templates;
using EMRO.Email;
using EMRO.OncologyConsultReports.Dto;
using EMRO.Patients.IntakeForm;
using EMRO.Sessions;
using EMRO.UserConsents;
using EMRO.UsersMetaInfo;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using System.Collections.Generic;

namespace EMRO.OncologyConsultReports
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConsultReportAppService : ApplicationService, IConsultReportAppService
    {
        private readonly IRepository<OncologyConsultReport, Guid> _consultReportRepository;
        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private readonly IConverter _converter;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IRepository<UserConsent, Guid> _userConsentRepository;
        private readonly IRepository<UserConsentPatientsDetails, Guid> _userConsentPatientsDetailsRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly EmroAppSession _session;
        private readonly IBlobContainer _blobContainer;
        private readonly IMailer _mailer;
        private readonly TemplateAppService _templateAppService;
        private readonly IAuditReportAppService _auditReportAppService;
        private string _apiEndpoint;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        public ConsultReportAppService(IRepository<OncologyConsultReport, Guid> consultReportRepository,
            IOptions<UplodedFilePath> uplodedFilePath,
            IAuditReportAppService auditReportAppService,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IConverter converter,
            IDoctorAppointmentRepository doctorAppointmentRepository,
            IRepository<User, long> userRepository,
            IRepository<UserConsent, Guid> userConsentRepository,
            IRepository<UserConsentPatientsDetails, Guid> userConsentPatientsDetailsRepository,
            IRepository<UserMetaDetails, Guid> userMetaDetailsRepository,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository
            , EmroAppSession session
            , IBlobContainer blobContainer
            , IMailer mailer
            , TemplateAppService templateAppService
            //, INotificationPublisher notificationPublisher
            , ICustomNotificationAppService customNotificationAppService)
        {
            _consultReportRepository = consultReportRepository;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            _configuration = configuration;
            _converter = converter;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _userConsentRepository = userConsentRepository;
            _userRepository = userRepository;
            _userConsentPatientsDetailsRepository = userConsentPatientsDetailsRepository;
            _userMetaDetailsRepository = userMetaDetailsRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _session = session;
            _blobContainer = blobContainer;
            _auditReportAppService = auditReportAppService;
            _mailer = mailer;
            _templateAppService = templateAppService;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
            _apiEndpoint = _configuration["App:ServerRootAddress"] + "api/V1/File/DownloadImage";
        }

        /// <summary>
        /// This function is used to create consultation report by doctor
        /// </summary>
        /// <param name="input"></param>
        /// <returns>return 200 response after successfully saved data</returns>
        public async Task<ConsultReportOutput> Create([FromForm] ConsultReportInput input)
        {
            ConsultReportOutput output = new ConsultReportOutput();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            string SignatureEncryptedFilePath = string.Empty;
            try
            {
                if (input.SignaturePath != null)
                {
                    profileOutput = await SaveSignature(input.SignaturePath, input.ConsultId, input.UserId);
                }
                if (!string.IsNullOrEmpty(input.ConsultId))
                {
                    Guid Id = new Guid(input.ConsultId);
                    var consult = await _consultReportRepository.GetAsync(Id);
                    if (consult != null)
                    {
                        consult.Purpose = input.Purpose;
                        consult.Allergies = input.Allergies;
                        consult.PastMedicalHistory = input.PastMedicalHistory;
                        consult.Plan = input.Plan;
                        consult.Impression = input.Impression;
                        consult.Investigation = input.Investigation;
                        consult.ReviewOfHistory = input.ReviewOfHistory;
                        consult.FamilyHistory = input.FamilyHistory;
                        consult.SocialHistory = input.SocialHistory;
                        consult.Medication = input.Medication;
                        consult.Notes = input.Notes;
                        consult.IsCompleted = false;
                        consult.IsActive = true;
                        if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                        {
                            consult.AppointmentId = input.AppointmentId;
                        }

                        consult.UserId = input.UserId;
                        consult.UpdatedOn = DateTime.UtcNow;
                        consult.CompletedDate = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(SignatureEncryptedFilePath))
                        {
                            consult.SignaturePath = SignatureEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim();
                        }
                        if (_session.UniqueUserId != null)
                        {
                            consult.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _consultReportRepository.UpdateAsync(consult);
                        output.Message = "Report updated successfully.";
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
                    var consultReport = new OncologyConsultReport
                    {
                        Purpose = input.Purpose,
                        Allergies = input.Allergies,
                        PastMedicalHistory = input.PastMedicalHistory,
                        Plan = input.Plan,
                        Impression = input.Impression,
                        Investigation = input.Investigation,
                        ReviewOfHistory = input.ReviewOfHistory,
                        FamilyHistory = input.FamilyHistory,
                        SocialHistory = input.SocialHistory,
                        Medication = input.Medication,
                        Notes = input.Notes,
                        IsCompleted = false,
                        IsActive = true,
                        // AppointmentId = input.AppointmentId,
                        UserId = input.UserId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    };
                    if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                    {
                        consultReport.AppointmentId = input.AppointmentId;
                    }
                    if (!string.IsNullOrEmpty(SignatureEncryptedFilePath))
                    {
                        consultReport.SignaturePath = SignatureEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim();
                    }
                    if (_session.UniqueUserId != null)
                    {
                        consultReport.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    consultReport.TenantId = AbpSession.TenantId;
                    Guid newId = await _consultReportRepository.InsertAndGetIdAsync(consultReport);
                    output.ConsultId = newId;
                    output.Message = "Report Created successfully.";
                    output.StatusCode = 200;

                    //Notification
                    if (input.AppointmentId != null && input.AppointmentId != Guid.Empty)
                    {
                        var appointment = _doctorAppointmentRepository.FirstOrDefault(x => x.Id == input.AppointmentId);
                        var user = _userRepository.FirstOrDefault(x => x.UniqueUserId == Guid.Parse(_session.UniqueUserId));
                        var doctorDetails = _userRepository.FirstOrDefault(x => x.UniqueUserId == appointment.DoctorId);
                        if (appointment.AppointmentSlotId != null && appointment.AppointmentSlotId != Guid.Empty)
                        {
                            string Title = "New case created";
                            string Message = user.FullName + " created a new case. See dashboard";
                            _customNotificationAppService.NotificationPublish(doctorDetails.Id, Title, Message);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Consult Report Create API" + ex.StackTrace);
            }

            return output;
        }

        public async Task<ConsultReportOutput> Completed([FromForm] CompelteConsultReport input)
        {
            var stopwatch = Stopwatch.StartNew();

            ConsultReportOutput output = new ConsultReportOutput();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                if (input.AppointmentId != Guid.Empty && input.AppointmentId != null)
                {
                    if (input.SignaturePath != null)
                    {
                        profileOutput = await SaveSignature(input.SignaturePath, input.ConsultId, input.UserId);
                        Pdfoutput output1 = await PDFGeneretor(input, profileOutput.ProfilePath);
                        if (output1.IsPdfGenerated == true && !string.IsNullOrEmpty(output1.PdfPath))
                        {

                            if (!string.IsNullOrEmpty(input.ConsultId))
                            {
                                Guid Id = new Guid(input.ConsultId);
                                var consult = await _consultReportRepository.GetAsync(Id);
                                if (consult != null)
                                {
                                    consult.Purpose = input.Purpose;
                                    consult.Allergies = input.Allergies;
                                    consult.PastMedicalHistory = input.PastMedicalHistory;
                                    consult.Plan = input.Plan;
                                    consult.Impression = input.Impression;
                                    consult.Investigation = input.Investigation;
                                    consult.ReviewOfHistory = input.ReviewOfHistory;
                                    consult.FamilyHistory = input.FamilyHistory;
                                    consult.SocialHistory = input.SocialHistory;
                                    consult.Medication = input.Medication;
                                    consult.Notes = input.Notes;
                                    consult.IsCompleted = true;
                                    consult.IsActive = true;
                                    consult.AppointmentId = input.AppointmentId;
                                    consult.UserId = (Guid)input.UserId;
                                    consult.UpdatedOn = DateTime.UtcNow;
                                    consult.CompletedDate = DateTime.UtcNow;
                                    consult.SignatureRequestId = profileOutput.CreateRequestId;
                                    consult.ReportRequestId = profileOutput.CreateRequestId;
                                    // consult.ReportPath = output1.PdfPath.Replace(_env.ContentRootPath, "").Trim();
                                }
                                if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                                {
                                    if (_uplodedFilePath.IsBlob)
                                    {
                                        consult.SignaturePath = profileOutput.ProfilePath.Trim();
                                    }
                                    else
                                    {
                                        consult.SignaturePath = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                                    }
                                    consult.IsBlobStorage = _uplodedFilePath.IsBlob;

                                }
                                if (!string.IsNullOrEmpty(output1.PdfPath))
                                {
                                    if (_uplodedFilePath.IsBlob)
                                    {
                                        consult.ReportPath = output1.PdfPath.Trim();
                                    }
                                    else
                                    {
                                        consult.ReportPath = output1.PdfPath.Replace(_env.ContentRootPath, "").Trim();
                                    }

                                }
                                if (_session.UniqueUserId != null)
                                {
                                    consult.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _consultReportRepository.UpdateAsync(consult);
                            }
                            else
                            {
                                var consultReport = new OncologyConsultReport
                                {
                                    Purpose = input.Purpose,
                                    Allergies = input.Allergies,
                                    PastMedicalHistory = input.PastMedicalHistory,
                                    Plan = input.Plan,
                                    Impression = input.Impression,
                                    Investigation = input.Investigation,
                                    ReviewOfHistory = input.ReviewOfHistory,
                                    FamilyHistory = input.FamilyHistory,
                                    SocialHistory = input.SocialHistory,
                                    Medication = input.Medication,
                                    Notes = input.Notes,
                                    IsCompleted = true,
                                    IsActive = true,
                                    AppointmentId = input.AppointmentId,
                                    UserId = (Guid)input.UserId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedOn = DateTime.UtcNow,
                                    CompletedDate = DateTime.UtcNow,
                                    ReportPath = output1.PdfPath.Replace(_env.ContentRootPath, "").Trim()
                                };
                                if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                                {
                                    if (_uplodedFilePath.IsBlob)
                                    {
                                        consultReport.SignaturePath = profileOutput.ProfilePath.Trim();
                                    }
                                    else
                                    {
                                        consultReport.SignaturePath = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                                    }

                                }
                                if (_session.UniqueUserId != null)
                                {
                                    consultReport.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                }


                                consultReport.TenantId = AbpSession.TenantId;
                                Guid newId = await _consultReportRepository.InsertAndGetIdAsync(consultReport);
                                output.ConsultId = newId;
                            }
                            var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId);
                            if (appointment != null)
                            {
                                var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);
                                string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                                if (patient != null)
                                {


                                    string Name = patient.FullName;
                                    string message = "A message is waiting for you and a report is being generated. Please login to the portal to see it.";
                                    string doctorbody = _templateAppService.GetTemplates(Name, message,"");

                                    //string doctorbody = "Hello " + "<b>" + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ",</b> <br /><br /> "
                                    //          + "A message is waiting for you and a report is being generated. Please login to the portal to see it." + " <br /><br /><br /> " +
                                    //           "Regards," + " <br />" + "EMRO Team";
                                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patient.EmailAddress, "Consultation Report", doctorbody, adminmail));

                                    //Encrypt Appointment Id for survey feedback form
                                    Guid feedbackAppointmentId = new Guid(input.AppointmentId.ToString());
                                    var encytedAppointmentId = System.Convert.ToBase64String(feedbackAppointmentId.ToByteArray());

                                    var feedbackUrl = HttpUtility.UrlEncode(encytedAppointmentId);

                                    //Decrypt Appointment Id for survey feedback form
                                    //var decryptedAppointmentId = Convert.FromBase64String(encytedAppointmentId);
                                    //var appIdTest = new Guid(decryptedAppointmentId);

                                    //get Doctor Details
                                    var doctors = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.UserId);

                                    //get appointment details
                                    Guid? appointentSlotId = _doctorAppointmentRepository.FirstOrDefault(x => x.Id == input.AppointmentId).AppointmentSlotId;
                                    var slotDetails = _doctorAppointmentSlotRepository.FirstOrDefault(x => x.Id == appointentSlotId);

                                    string feedbackMessage = "ETeleHealth team would like to know about how your appointment with Dr." + doctors.FullName + " has gone." + "<br/>" +
    "Please spare a minute and provide your valuable feedback for the appointment held with him " + 
    "<a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/survey-form/" + feedbackUrl + "'>Click to fill the form</a> <br />";

                                    if (slotDetails != null)
                                    {
                                        feedbackMessage = "ETeleHealth team would like to know about how your appointment with Dr." + doctors.FullName + " has gone." + "<br/>" +
    "Please spare a minute and provide your valuable feedback for the appointment held with him on " + slotDetails.AvailabilityDate + ", " +
     DateTime.Parse(slotDetails.AvailabilityStartTime).ToString("hh:mm tt") + " (" + slotDetails.TimeZone + ")" + "<br/><br/>" +
    "<a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/survey-form/" + feedbackUrl + "'>Click to fill the form</a> <br />";
                                    }

                                    string feedbackbody = _templateAppService.GetTemplates(Name, feedbackMessage,"");

                                    //Send email for survey feedback form
                                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patient.EmailAddress, "ETeleHealth - Feedback of your appointment", feedbackbody, ""));

                                    //Send Notification to user

                                    string Title = "Report generated";
                                    string Message = "Dr " + doctors.FullName + " completed your report and is now available in your dashboard.";
                                    _customNotificationAppService.NotificationPublish(patient.Id, Title, Message);

                                    stopwatch.Stop();

                                    //Log Audit Events
                                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                                    createEventsInput.Parameters = Utility.Serialize(input);
                                    createEventsInput.Operation = "Consult Report has been created for user " + patient.FullName + " by consultant " + doctors.Title + " " + doctors.FullName;
                                    createEventsInput.Component = "Consult Report";
                                    createEventsInput.Action = "Completed";

                                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                                }
                            }

                            output.Message = "Report Completed successfully.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Message = "Something went wrong, please try again.";
                            output.StatusCode = 500;
                        }
                    }
                    else
                    {
                        output.Message = "Signature is required.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Please complete your booking.";
                    output.StatusCode = 401;
                }


            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Consult Report Complete API" + ex.StackTrace);
            }

            return output;
        }

        protected async Task<UploadProfileOutput> SaveSignature(IFormFile formFile, string ConsultId, Guid? UserId)
        {
            string SignaturefileExt = string.Empty;
            string SignatureFileName = string.Empty;
            bool SignatureEncryptFileStatus = false;
            string SignatureEncryptedFilePath = string.Empty;
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string filePath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            if (formFile != null)
            {
                Guid Signatureobj = Guid.NewGuid();
                SignaturefileExt = Path.GetExtension(formFile.FileName);
                SignatureFileName = Signatureobj.ToString() + ".png";
                if (!string.IsNullOrEmpty(ConsultId))
                {
                    Guid Id = new Guid(ConsultId);
                    var consult = await _consultReportRepository.GetAsync(Id);
                    if (consult != null)
                    {
                        string path = _env.ContentRootPath + consult.SignaturePath;
                        if (File.Exists(path))
                        {
                            // If file found, delete it    
                            File.Delete(path);
                        }

                        // bool status = _blobContainer.Delete(consult.SignaturePath);
                    }
                }

                if (_uplodedFilePath.IsBlob)
                {
                    filePath = Path.Combine(UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobConsultSignature);
                    filePath = filePath + SignatureFileName;
                    uploadDocumentInput.azureDirectoryPath = filePath;
                    uploadDocumentInput.Document = formFile;
                    var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                    if (blobResponse != null)
                    {
                        profileOutput.ProfilePath = filePath;
                        profileOutput.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                    }

                }
                else
                {
                    filePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.ConsultSignature + UserId.ToString() + _uplodedFilePath.Slash);

                    if (!Directory.Exists(filePath))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(filePath);
                    }
                    SignatureEncryptedFilePath = filePath + "Enc_" + SignatureFileName;
                    filePath = filePath + SignatureFileName;

                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await formFile.CopyToAsync(stream);
                        stream.Flush();

                    }
                    SignatureEncryptFileStatus = Utility.EncryptFile(filePath, SignatureEncryptedFilePath, EncryptedKey);
                    if (SignatureEncryptFileStatus)
                    {

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    profileOutput.ProfilePath = SignatureEncryptedFilePath; ;
                }

            }
            return profileOutput;

        }

        protected async Task<string> CreatePDF(string html, string PdffileName, string UserId)
        {
            string path = _env.ContentRootPath + _uplodedFilePath.ConsultReport + UserId;
            string EncryptedPath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
                }
                path = path + _uplodedFilePath.Slash + PdffileName;
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DPI = 250,
                    DocumentTitle = "Consult Report",
                    Out = path
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8", MinimumFontSize = 45 },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Spacing = 2.00 },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Center = "", Spacing = 2.812 }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };


                var file = _converter.Convert(pdf);
                if (_uplodedFilePath.IsBlob)
                {
                    EncryptedPath = UserId + _uplodedFilePath.Slash + _uplodedFilePath.BlobConsultReport + PdffileName;
                    var blobResponse = await _blobContainer.UploadPdf(EncryptedPath, path);

                    if (System.IO.File.Exists(path))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(path);
                    }
                }
                else
                {
                    EncryptedPath = _env.ContentRootPath + _uplodedFilePath.ConsultReport + UserId + _uplodedFilePath.Slash + "Enc_" + PdffileName;
                }

                //PDF Encryption
                bool EncryptFileStatus = Utility.EncryptFile(path, EncryptedPath, _configuration[Utility.ENCRYPTION_KEY]);
                if (EncryptFileStatus)
                {
                    if (System.IO.File.Exists(path))
                    {
                        // If file found, delete it    
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Download PDF" + ex.StackTrace);
            }
            return EncryptedPath;
        }

        protected async Task<Pdfoutput> PDFGeneretor(CompelteConsultReport input, string SignatureEncryptedFilePath)
        {
            Pdfoutput output = new Pdfoutput();
            output.IsPdfGenerated = false;
            string ImageUrl = string.Empty;
            string logoImageUrl = string.Format("data:image/png;base64,{0}", URLToBase64String(_apiEndpoint + "?fileName=logo-emro.png")); 
            string SignatureImageUrl = string.Format("data:image/png;base64,{0}", URLToBase64String(_apiEndpoint + "?fileName=signature.png"));
            try
            {
                var appointment = _userConsentRepository.FirstOrDefault(x => x.AppointmentId == input.AppointmentId);
                if (appointment != null)
                {
                    if (_uplodedFilePath.IsBlob)
                    {
                        ImageUrl = string.Format("data:image/png;base64,{0}", URLToBase64String(_blobContainer.Download(SignatureEncryptedFilePath)));
                    }
                    else
                    {
                        ImageUrl = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(Utility.DecryptFile(SignatureEncryptedFilePath, _configuration[Utility.ENCRYPTION_KEY])));

                    }
                    var patient = _userConsentPatientsDetailsRepository.Get(appointment.UserConsentPatientsDetailsId);
                    var doctor = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.UserId);
                    var doctordeatils = _userMetaDetailsRepository.FirstOrDefault(x => x.UserId == input.UserId);
                    var familyDoctor = _userRepository.FirstOrDefault(x => x.UniqueUserId == appointment.FamilyDoctorId);

                    if (patient != null && doctor != null)
                    {
                        var pathToFile = _env.WebRootPath
                           + Path.DirectorySeparatorChar.ToString()
                           + "Templates"
                           + Path.DirectorySeparatorChar.ToString()
                           + "PDFTemplate"
                           + Path.DirectorySeparatorChar.ToString()
                           + "pdf.html";

                        var builder = new BodyBuilder();
                        using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                        {

                            builder.HtmlBody = SourceReader.ReadToEnd();

                        }
                        string pdfBody = string.Format(builder.HtmlBody,
                        patient.FirstName + ' ' + patient.LastName,
                        input.Purpose,
                        input.ReviewOfHistory,
                        input.Notes,
                        input.PastMedicalHistory,
                        input.Allergies,
                        input.Medication,
                        input.SocialHistory,
                        input.FamilyHistory,
                        input.Investigation,
                        input.Impression,
                        input.Plan,
                        doctor.Title + " " + doctor.Name + " " + doctor.Surname + "," + " " + doctordeatils.Credentials,
                        ImageUrl,
                         DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm tt"),
                        familyDoctor != null ? familyDoctor.Title + " " + familyDoctor.Name + " " + familyDoctor.Surname + " (Family Doctor)," : "",
                        doctor.Title + " " + doctor.Name + " " + doctor.Surname + " " + "(" + doctordeatils.OncologySpecialty + ")",
                        logoImageUrl,
                        SignatureImageUrl
                        );
                        Guid obj = Guid.NewGuid();
                        string filename = obj.ToString() + ".pdf";
                        output.PdfPath = await CreatePDF(pdfBody, filename, input.UserId.ToString());
                        if (!string.IsNullOrEmpty(output.PdfPath))
                        {
                            output.IsPdfGenerated = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("PDF Generetor" + ex.StackTrace);
            }

            return output;
        }

        public async Task<GetConsultOutput> Get(string ConsultId)
        {
            GetConsultOutput output = new GetConsultOutput();
            try
            {
                if (!string.IsNullOrEmpty(ConsultId))
                {
                    Guid Id = new Guid(ConsultId);
                    var consult = await _consultReportRepository.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true && x.IsCompleted == false);
                    if (consult != null)
                    {
                        output.Purpose = consult.Purpose;
                        output.Allergies = consult.Allergies;
                        output.PastMedicalHistory = consult.PastMedicalHistory;
                        output.Plan = consult.Plan;
                        output.Impression = consult.Impression;
                        output.Investigation = consult.Investigation;
                        output.ReviewOfHistory = consult.ReviewOfHistory;
                        output.FamilyHistory = consult.FamilyHistory;
                        output.SocialHistory = consult.SocialHistory;
                        output.Medication = consult.Medication;
                        output.Notes = consult.Notes;
                        output.AppointmentId = consult.AppointmentId;
                        output.UserId = consult.UserId;
                        output.ConsultId = consult.Id;

                        if (!string.IsNullOrEmpty(consult.SignaturePath))
                        {
                            if (consult.IsBlobStorage)
                            {
                                output.Signature = await _blobContainer.DownloadSignature(consult.SignaturePath);
                                //output.Signature = "";
                            }
                            else
                            {
                                string path = _env.ContentRootPath + consult.SignaturePath;
                                string DocumentfileExt = Path.GetExtension(path);
                                output.Mimetype = Utility.GetMimeType(path);
                                output.SignatureName = Path.GetFileName(path).Replace("Enc_", "");
                                byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                                output.Signature = Convert.ToBase64String(b);
                            }

                        }

                        output.Message = " Get consult report successfully.";
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
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Consult" + ex.StackTrace);
            }
            return output;
        }

        public async Task<CasesOutput> ActiveCase(ActiveCaseInput input)
        {
            CasesOutput output = new CasesOutput();
            var outputItems = new List<GetCasesOutput>();
            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty && !string.IsNullOrEmpty(input.RoleName))
                {
                    var users = await _userRepository.GetAllListAsync(x => x.UserType == "Patient");
                    if (input.RoleName == "FAMILYDOCTOR")
                    {
                        var userconsent = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId == input.UserId);
                        if (userconsent != null && userconsent.Count > 0)
                        {
                            output.Items = (from c in userconsent.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.AppointmentId equals uc.AppointmentId
                                            join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                            on c.AppointmentId equals da.Id
                                            //join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            //on da.AppointmentSlotId equals slot.Id
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on c.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals u.UniqueUserId
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                             on da.AppointmentSlotId equals slot.Id
                                             into ps
                                            from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == false && (da.IsBooked == 1) && us.UserType == "Patient"
                                            orderby uc.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                DoctorName = u.Name + " " + u.Surname,
                                                PatientName = pa.FirstName + " " + pa.LastName,
                                                //AppointmentDate = Convert.ToDateTime(slot.AvailabilityDate).ToString("dd MMM"),
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = da.Id,
                                                IsBookLater = da.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = da.DoctorId,
                                                IsPayment = da.IsBooked == 1 ? true : false,
                                                //PatientId = c.PatientId == null ? 0 : c.PatientId != Guid.Empty ? 0 : users.Where(x => x.UniqueUserId == c.PatientId).FirstOrDefault().Id
                                            }).OrderBy(x => x.AppointmentDate).ThenByDescending(x => x.IsBookLater).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get active cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList(); 
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }
                    else if (input.RoleName != "CONSULTANT")
                    {
                        var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == input.UserId && (x.IsBooked == 1));
                        if (appointment != null && appointment.Count > 0)
                        {

                            output.Items = (from c in appointment.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.Id equals uc.AppointmentId
                                            join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                            on c.Id equals ucp.AppointmentId
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on ucp.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on c.DoctorId equals u.UniqueUserId
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on uc.UserId equals us.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on c.AppointmentSlotId equals slot.Id
                                            into ps
                                            from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == false
                                            orderby uc.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                DoctorName = u.Title + " " + u.Name + " " + u.Surname,
                                                PatientName = pa.FirstName + " " + pa.LastName,
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = c.Id,
                                                IsBookLater = c.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = c.DoctorId,
                                                IsPayment = c.IsBooked == 1 ? true : false,
                                                UserId = ucp.PatientId,
                                                PatientId = pa.CaseId
                                                //PatientId = ucp.PatientId == null ? 0 : ucp.PatientId != Guid.Empty ? 0 : users.Where(x => x.UniqueUserId == ucp.PatientId).FirstOrDefault().Id
                                            }).GroupBy(x => x.AppointmentId).Select(x => x.FirstOrDefault()).ToList();
                            output.Items = (from list in output.Items.AsEnumerable()
                                            join u in users.AsEnumerable()
                                            on list.UserId equals u.UniqueUserId
                                            into ps
                                            from p in ps.DefaultIfEmpty()
                                            select new GetCasesOutput
                                            {
                                                DoctorName = list.DoctorName,
                                                PatientName = list.PatientName,
                                                AppointmentDate = list.AppointmentDate,
                                                ConsultId = list.ConsultId,
                                                BookedBy = list.BookedBy,
                                                AppointmentId = list.AppointmentId,
                                                IsBookLater = list.IsBookLater,
                                                DoctorId = list.DoctorId,
                                                IsPayment = list.IsPayment,
                                                PatientId = list.PatientId != null ? list.PatientId : p == null ? "0" : p.UniqueUserId == Guid.Empty ? "0" : Convert.ToString(p.Id)
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType,
                                            }).GroupBy(x => x.AppointmentId).Select(x => x.FirstOrDefault()).OrderBy(x => x.AppointmentDate).ThenByDescending(x => x.IsBookLater).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get active cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList();
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }
                    else
                    {
                        var appointments = _doctorAppointmentRepository.GetAll().Where(x => x.DoctorId == input.UserId && (x.IsBooked == 1)).ToList();
                        // var cases = _consultReportRepository.GetAll().Where(x => x.IsActive == true && x.IsCompleted == false && x.UserId == input.UserId).ToList();
                        if (appointments != null && appointments.Count > 0)
                        {
                            output.Items = (from c in appointments.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.Id equals uc.AppointmentId
                                            join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                            on c.Id equals ucp.AppointmentId
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on ucp.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on c.UserId equals u.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                             on c.AppointmentSlotId equals slot.Id
                                             into ps
                                            from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == false
                                            orderby c.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                PatientName = u.Name + " " + u.Surname,
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = c.Id,
                                                IsBookLater = c.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = c.DoctorId,
                                                IsPayment = c.IsBooked == 1 ? true : false,
                                                UserId = ucp.PatientId,
                                                PatientId = u.UserType == "Insurance" ? pa.CaseId : u.UserType == "MedicalLegal" ? pa.CaseId : null
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType,
                                            }).GroupBy(x => x.AppointmentId).Select(x => x.FirstOrDefault()).ToList();
                            output.Items = (from list in output.Items.AsEnumerable()
                                            join u in users.AsEnumerable()
                                            on list.UserId equals u.UniqueUserId
                                            into ps
                                            from p in ps.DefaultIfEmpty()
                                            select new GetCasesOutput
                                            {
                                                PatientName = list.PatientName,
                                                AppointmentDate = list.AppointmentDate,
                                                ConsultId = list.ConsultId,
                                                BookedBy = list.BookedBy,
                                                AppointmentId = list.AppointmentId,
                                                IsBookLater = list.IsBookLater,
                                                DoctorId = list.DoctorId,
                                                IsPayment = list.IsPayment,
                                                PatientId = list.PatientId != null ? list.PatientId : p == null ? "0" : p.UniqueUserId == Guid.Empty ? "0" : Convert.ToString(p.Id)
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType,
                                            }).GroupBy(x => x.AppointmentId).Select(x => x.FirstOrDefault()).OrderBy(x => x.AppointmentDate).ThenByDescending(x => x.IsBookLater).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get active cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList();
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Consult" + ex.StackTrace);
            }
            return output;
        }

        public async Task<CasesOutput> ArchiveCase(ActiveCaseInput input)
        {
            CasesOutput output = new CasesOutput();
            var outputItems = new List<GetCasesOutput>();
            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty && !string.IsNullOrEmpty(input.RoleName))
                {
                    if (input.RoleName == "FAMILYDOCTOR")
                    {
                        var userconsent = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId == input.UserId);
                        if (userconsent != null && userconsent.Count > 0)
                        {
                            output.Items = (from c in userconsent.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.AppointmentId equals uc.AppointmentId
                                            join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                            on c.AppointmentId equals da.Id
                                            //join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            //on da.AppointmentSlotId equals slot.Id
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on c.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals u.UniqueUserId
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals slot.Id
                                            into ps
                                            from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == true && da.IsBooked == 1
                                            orderby uc.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                DoctorName = u.Name + " " + u.Surname,
                                                PatientName = pa.FirstName + " " + pa.LastName,
                                                //AppointmentDate = Convert.ToDateTime(slot.SlotZoneTime).ToString(),
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = da.Id,
                                                IsBookLater = da.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = da.DoctorId,
                                                IsPayment = da.IsBooked == 1 ? true : false
                                                //BookedBy = us.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(us.UserType) ? "Admin" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : us.UserType

                                            }).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get active cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList();
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }
                    else if (input.RoleName != "CONSULTANT")
                    {
                        var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == input.UserId && x.IsBooked == 1);
                        if (appointment != null && appointment.Count > 0)
                        {

                            output.Items = (from c in appointment.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.Id equals uc.AppointmentId
                                            //join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            //on c.AppointmentSlotId equals slot.Id
                                            join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                            on c.Id equals ucp.AppointmentId
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on ucp.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on c.DoctorId equals u.UniqueUserId
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on uc.UserId equals us.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on c.AppointmentSlotId equals slot.Id
                                            into ps
                                            from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == true
                                            orderby uc.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                DoctorName = u.Name + " " + u.Surname,
                                                PatientName = pa.FirstName + " " + pa.LastName,
                                                //AppointmentDate = Convert.ToDateTime(slot.SlotZoneTime).ToString(),
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = c.Id,
                                                IsBookLater = c.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = c.DoctorId,
                                                IsPayment = c.IsBooked == 1 ? true : false
                                                // BookedBy = us.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(us.UserType) ? "Admin" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : us.UserType

                                            }).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get archive cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList();
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }
                    else
                    {

                        var appointments = _doctorAppointmentRepository.GetAll().Where(x => x.DoctorId == input.UserId && (x.IsBooked == 1 || x.IsBooked == 3)).ToList();
                        var users = await _userRepository.GetAllListAsync(x => x.UserType == "Patient");
                        // var cases = _consultReportRepository.GetAll().Where(x => x.IsActive == true && x.IsCompleted == false && x.UserId == input.UserId).ToList();
                        if (appointments != null && appointments.Count > 0)
                        {
                            output.Items = (from c in appointments.AsEnumerable()
                                            join uc in _consultReportRepository.GetAll().AsEnumerable()
                                            on c.Id equals uc.AppointmentId
                                            join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                            on c.Id equals ucp.AppointmentId
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on c.UserId equals u.UniqueUserId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                             on c.AppointmentSlotId equals slot.Id
                                             into ps
                                             from p in ps.DefaultIfEmpty()
                                            where uc.IsActive == true && uc.IsCompleted == true
                                            orderby c.CreatedOn descending
                                            select new GetCasesOutput
                                            {
                                                PatientName = u.Name + " " + u.Surname,
                                                //AppointmentDate = Convert.ToDateTime(slot.SlotZoneTime).ToString(),
                                                AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                                ConsultId = uc.Id,
                                                BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : "",
                                                AppointmentId = c.Id,
                                                IsBookLater = c.AppointmentSlotId == Guid.Empty ? true : false,
                                                DoctorId = c.DoctorId,
                                                IsPayment = c.IsBooked == 1 ? true : false,
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType,
                                            }).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get archive cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Items = outputItems.ToList();
                            output.Message = "No record found.";
                            output.StatusCode = 200;
                        }
                    }


                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Consult" + ex.StackTrace);
            }
            return output;
        }

        public async Task<ConsultReportOutput> Delete(string ConsultId)
        {
            ConsultReportOutput output = new ConsultReportOutput();
            try
            {
                if (!string.IsNullOrEmpty(ConsultId))
                {
                    Guid Id = new Guid(ConsultId);
                    var consult = await _consultReportRepository.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true && x.IsCompleted == false);
                    if (consult != null)
                    {
                        consult.IsActive = false;
                        await _consultReportRepository.UpdateAsync(consult);
                        output.Message = "consult report deleted successfully.";
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
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Consult" + ex.StackTrace);
            }
            return output;
        }

        public UserCasesOutput UserConsultantReport(UserActiveCaseInput input)
        {
            UserCasesOutput output = new UserCasesOutput();
            try
            {
                if (input.AppointmentId != null && input.AppointmentId != Guid.Empty && !string.IsNullOrEmpty(input.RoleName))
                {
                    if (input.RoleName == "PATIENT")
                    {
                        var appointmentuser = _doctorAppointmentRepository.FirstOrDefault(x => x.Id == input.AppointmentId);
                        if (appointmentuser != null)
                        {
                            var appointment = _doctorAppointmentRepository.GetAll().Where(x => x.UserId == appointmentuser.UserId && x.IsBooked == 1).ToList();

                            output.Items = (from uc in appointment.AsEnumerable()
                                            join c in _consultReportRepository.GetAll().AsEnumerable()
                                            on uc.Id equals c.AppointmentId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on uc.AppointmentSlotId equals slot.Id
                                            join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                            on c.AppointmentId equals ucp.AppointmentId
                                            join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                            on ucp.UserConsentPatientsDetailsId equals pa.Id
                                            join u in _userRepository.GetAll().AsEnumerable()
                                            on uc.DoctorId equals u.UniqueUserId
                                            where c.IsActive == true && c.IsCompleted == true
                                            orderby c.CreatedOn descending
                                            select new GetUserCasesOutput
                                            {
                                                DoctorName = u.Name + " " + u.Surname,
                                                PatientName = pa.FirstName + " " + pa.LastName,
                                                AppointmentDate = slot.SlotZoneTime,
                                                ConsultId = c.Id,
                                                BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : ""
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType
                                            }).ToList();
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get archive cases list.";
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

                        var cases = _consultReportRepository.GetAll().Where(x => x.IsActive == true && x.IsCompleted == true && x.AppointmentId == input.AppointmentId).ToList();
                        if (cases != null && cases.Count > 0)
                        {

                            if (input.RoleName != "CONSULTANT")
                            {
                                output.Items = (from c in cases.AsEnumerable()
                                                join uc in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                on c.AppointmentId equals uc.Id
                                                join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                on uc.AppointmentSlotId equals slot.Id
                                                join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                                on c.AppointmentId equals ucp.AppointmentId
                                                join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                                on ucp.UserConsentPatientsDetailsId equals pa.Id
                                                join u in _userRepository.GetAll().AsEnumerable()
                                                on uc.DoctorId equals u.UniqueUserId
                                                orderby c.CreatedOn descending
                                                select new GetUserCasesOutput
                                                {
                                                    DoctorName = u.Name + " " + u.Surname,
                                                    PatientName = pa.FirstName + " " + pa.LastName,
                                                    AppointmentDate = slot.SlotZoneTime,
                                                    ConsultId = c.Id,
                                                    BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : ""
                                                    // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType
                                                }).ToList();
                            }
                            else
                            {
                                output.Items = (from c in cases.AsEnumerable()
                                                join uc in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                on c.AppointmentId equals uc.Id
                                                join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                on uc.AppointmentSlotId equals slot.Id
                                                join ucp in _userConsentRepository.GetAll().AsEnumerable()
                                                on c.AppointmentId equals ucp.AppointmentId
                                                join pa in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                                on ucp.UserConsentPatientsDetailsId equals pa.Id
                                                join u in _userRepository.GetAll().AsEnumerable()
                                                on uc.DoctorId equals u.UniqueUserId
                                                orderby c.CreatedOn descending
                                                select new GetUserCasesOutput
                                                {
                                                    DoctorName = u.Name + " " + u.Surname,
                                                    PatientName = pa.FirstName + " " + pa.LastName,
                                                    AppointmentDate = slot.SlotZoneTime,
                                                    ConsultId = c.Id,
                                                    BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : ""
                                                    //BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType,
                                                }).ToList();
                            }
                            output.Count = output.Items.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }
                            output.Message = "Get archive cases list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Message = "No record found.";
                            output.StatusCode = 401;
                        }
                    }

                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get Consult" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetReportOutput> GetReport(string ConsultId)
        {
            GetReportOutput output = new GetReportOutput();
            try
            {
                if (!string.IsNullOrEmpty(ConsultId))
                {
                    Guid Id = new Guid(ConsultId);
                    var consult = await _consultReportRepository.FirstOrDefaultAsync(x => x.Id == Id && x.IsActive == true && x.IsCompleted == true);
                    if (consult != null)
                    {
                        var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == consult.AppointmentId);
                        if (appointment != null)
                        {
                            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);
                            var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);

                            var userconsent = await _userConsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == consult.AppointmentId);
                            var patient = await _userConsentPatientsDetailsRepository.FirstOrDefaultAsync(x => x.Id == userconsent.UserConsentPatientsDetailsId);

                            if (user != null && doctor != null && user.UserType.ToUpper() == "PATIENT")
                            {
                                output.ReoportName = doctor.Name + "_" + user.Name + "_" + appointment.Id + "_" + "report.pdf";
                            }
                            else if (user != null && doctor != null && (user.UserType.ToUpper() == "INSURANCE" || user.UserType.ToUpper() == "MEDICALLEGAL"))
                            {
                                output.ReoportName = doctor.Name + "_" + user.Name + "_" + patient.CaseId + "_" + "report.pdf";
                            }
                            else
                            {
                                output.ReoportName = doctor.Name + "_" + user.Name + "_" + appointment.Id + "_" + "report.pdf";
                            }
                        }

                        if (!string.IsNullOrEmpty(consult.ReportPath))
                        {
                            if (consult.IsBlobStorage)
                            {
                                output.Report = _blobContainer.Download(consult.ReportPath);
                            }
                            else
                            {
                                string path = _env.ContentRootPath + consult.ReportPath;
                                string DocumentfileExt = Path.GetExtension(path);
                                output.Mimetype = Utility.GetMimeType(path);
                                if (string.IsNullOrEmpty(output.ReoportName))
                                {
                                    output.ReoportName = Path.GetFileName(path).Replace("Enc_", "");
                                }
                                byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                                output.Report = Convert.ToBase64String(b);
                            }

                        }

                        output.Message = " Get consult report successfully.";
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
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Get Report" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Get list of consultant report for particular user.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>list of consultant report for particular user</returns>
        public async Task<UserCasesOutput> PatientConsultantReport(PatientActiveCaseInput input)
        {
            UserCasesOutput output = new UserCasesOutput();

            try
            {
                if (input.UserId != null && input.UserId != Guid.Empty)
                {

                    var patientDetail = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.UserId);
                    var appointmentList = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == input.UserId && x.DoctorId != input.DoctorId);

                    if (patientDetail != null)
                    {
                        if (appointmentList != null)
                        {
                            output.Items = (from a in appointmentList
                                            join ucp in _consultReportRepository.GetAll().AsEnumerable()
                                            on a.Id equals ucp.AppointmentId
                                            join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on a.AppointmentSlotId equals slot.Id
                                            join doc in _userRepository.GetAll().AsEnumerable()
                                            on a.DoctorId equals doc.UniqueUserId
                                            where ucp.IsActive == true && ucp.IsCompleted == true
                                            select new GetUserCasesOutput
                                            {
                                                DoctorName = doc.Name + " " + doc.Surname,
                                                PatientName = patientDetail.Name + " " + patientDetail.Surname,
                                                AppointmentDate = slot.SlotZoneTime,
                                                ConsultId = ucp.Id,
                                                BookedBy = patientDetail.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(patientDetail.UserType) ? "" : patientDetail.UserType == "Insurance" ? "Legal" : patientDetail.UserType == "MedicalLegal" ? "Legal" : ""
                                                // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType
                                            }).ToList();

                            output.Message = "Get conultant report list.";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            output.Message = "No consultant report found for this user.";
                            output.StatusCode = 200;
                        }
                    }
                    else
                    {
                        output.Message = "User not found.";
                        output.StatusCode = 404;
                    }
                }
                else
                {
                    output.Message = "Bad request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get Consult for patient message, " + ex.Message);
                Logger.Error("Get Consult for patient stacktrace, " + ex.StackTrace);
            }

            return output;
        }

        /// <summary>
        /// Get consultant report for all role.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<UserCasesOutput> GetConsultantReportForAllRole(GetConsultantReportInput input)
        {
            UserCasesOutput output = new UserCasesOutput();
            try
            {

                var userDetail = new User();
                var appointmentList = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);

                if (input.RoleName.ToUpper() == "PATIENT")
                {
                    if (input.UserId != null && input.UserId != Guid.Empty)
                    {
                        userDetail = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.UserId);
                        appointmentList = appointmentList.Where(x => x.UserId == input.UserId).ToList();
                    }
                    else
                    {
                        output.Message = "Bad request.";
                        output.StatusCode = 401;
                    }
                }
                else if (input.RoleName.ToUpper() == "INSURANCE" || 
                    input.RoleName.ToUpper() == "MEDICALLEGAL" || 
                    input.RoleName.ToUpper() == "CONSULTANT" || 
                    input.RoleName.ToUpper() == "FAMILYDOCTOR" || 
                    input.RoleName.ToUpper() == "DIAGNOSTIC")
                {
                    if (input.DoctorId != null && input.DoctorId != Guid.Empty && input.PatientId != null && input.PatientId != Guid.Empty)
                    {
                        userDetail = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.PatientId);
                        appointmentList = appointmentList.Where(x => x.UserId == input.PatientId && x.DoctorId == input.DoctorId).ToList();
                    }
                    else
                    {
                        output.Message = "Bad request.";
                        output.StatusCode = 401;
                    }
                }

                if (userDetail != null)
                {
                    if (appointmentList != null)
                    {
                        output.Items = (from a in appointmentList
                                        join ucp in _consultReportRepository.GetAll().AsEnumerable()
                                        on a.Id equals ucp.AppointmentId
                                        //join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                        //on a.AppointmentSlotId equals slot.Id
                                        join doc in _userRepository.GetAll().AsEnumerable()
                                        on a.DoctorId equals doc.UniqueUserId
                                        join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                        on a.AppointmentSlotId equals slot.Id
                                        into ps
                                        from p in ps.DefaultIfEmpty()
                                        where ucp.IsActive == true && ucp.IsCompleted == true
                                        orderby ucp.UpdatedOn == null, ucp.UpdatedOn descending
                                        select new GetUserCasesOutput
                                        {
                                            DoctorName = doc.Name + " " + doc.Surname,
                                            PatientName = userDetail.Name + " " + userDetail.Surname,
                                            //AppointmentDate = slot.SlotZoneTime,
                                            AppointmentDate = p == null ? null : p.SlotZoneTime == null ? (DateTime?)null : Convert.ToDateTime(p.SlotZoneTime),
                                            ConsultId = ucp.Id,
                                            BookedBy = userDetail.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(userDetail.UserType) ? "" : userDetail.UserType == "Insurance" ? "Legal" : userDetail.UserType == "MedicalLegal" ? "Legal" : ""
                                            // BookedBy = u.UserType == "EmroAdmin" ? "Admin" : string.IsNullOrEmpty(u.UserType) ? "Admin" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : u.UserType
                                        }).ToList();

                        output.Message = "Get conultant report list.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No consultant report found for this user.";
                        output.StatusCode = 200;
                    }
                }
                else
                {
                    output.Message = "User not found.";
                    output.StatusCode = 404;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get Consultant report for all role -- error message, " + ex.Message);
                Logger.Error("Get Consultant report for all role -- error stacktrace, " + ex.StackTrace);
            }

            return output;
        }

        private string URLToBase64String(string url)
        {
            string base64String = "";

            try
            {
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    base64String = Convert.ToBase64String(response.RawBytes);
                }
                else
                {
                    Logger.Error("URLToBase64String, response error - " + response.ErrorMessage);
                }
            }
            catch(Exception ex)
            {
                Logger.Error("URLToBase64String, error message - " + ex.Message);
                Logger.Error("URLToBase64String, error stacktrace - " + ex.StackTrace);
            }

            return base64String;
        }
    }
}
