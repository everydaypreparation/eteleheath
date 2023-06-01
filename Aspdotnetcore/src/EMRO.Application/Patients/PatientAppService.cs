using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Notifications;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AzureBlobStorage;
using EMRO.Common.AzureBlobStorage.Dto;
using EMRO.Common.CustomNotification;
using EMRO.Common.Templates;
using EMRO.Email;
using EMRO.InviteUsers;
using EMRO.Master;
using EMRO.OncologyConsultReports;
using EMRO.Patients.Dto;
using EMRO.Patients.IntakeForm;
using EMRO.Sessions;
using EMRO.UserConsents;
using EMRO.UserDocuments;
using EMRO.UsersMetaInfo;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;

namespace EMRO.Patients
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class PatientAppService : ApplicationService, IPatientAppService
    {
        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<UserDocument, Guid> _userDocumentRepository;
        private readonly IRepository<UserConsentPatientsDetails, Guid> _userConsentPatientsDetailsRepository;
        private readonly IRepository<UserConsent, Guid> _userconsentRepository;
        private readonly IRepository<UserConsentForm, Guid> _userConsentFormRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IRepository<DoctorAppointment, Guid> _doctorAppointmentRepository;
        private readonly IMailer _mailer;
        private IConfiguration _configuration;
        private readonly IAuditReportAppService _auditReportAppService;
        IRepository<TimeZones, Guid> _timeZonesRepository;
        private readonly EmroAppSession _session;
        private readonly IRepository<InviteUser, Guid> _inviteUserRepository;
        IRepository<OncologyConsultReport, Guid> _consultReportRepository;
        private readonly IBlobContainer _blobContainer;
        private readonly TemplateAppService _templateAppService;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;

        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public PatientAppService(IRepository<User, long> userRepository,
        IAuditReportAppService auditReportAppService,
            IRepository<UserMetaDetails, Guid> userMetaDetailsRepository,
            UserManager userManager, RoleManager roleManager,
            IRepository<UserConsent, Guid> userconsentRepository,
            IRepository<UserDocument, Guid> userDocumentRepository,
            IRepository<UserConsentPatientsDetails, Guid> userConsentPatientsDetailsRepository,
            IOptions<UplodedFilePath> uplodedFilePath, IWebHostEnvironment env,
            IRepository<UserConsentForm, Guid> userConsentFormRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IRepository<DoctorAppointment, Guid> doctorAppointmentRepository,
            IMailer mailer, IConfiguration configuration,
            IRepository<TimeZones, Guid> timeZonesRepository
            , EmroAppSession session
            , IRepository<InviteUser, Guid> inviteUserRepository
            , IRepository<OncologyConsultReport, Guid> consultReportRepository
            , IBlobContainer blobContainer
            , TemplateAppService templateAppService
            //, INotificationPublisher notificationPublisher
            , ICustomNotificationAppService customNotificationAppService
            )
        {
            _userRepository = userRepository;
            _userMetaDetailsRepository = userMetaDetailsRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _userconsentRepository = userconsentRepository;
            _userDocumentRepository = userDocumentRepository;
            _userConsentPatientsDetailsRepository = userConsentPatientsDetailsRepository;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            _auditReportAppService = auditReportAppService;
            _userConsentFormRepository = userConsentFormRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _mailer = mailer;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _configuration = configuration;
            _timeZonesRepository = timeZonesRepository;
            _session = session;
            _inviteUserRepository = inviteUserRepository;
            _consultReportRepository = consultReportRepository;
            _blobContainer = blobContainer;
            _templateAppService = templateAppService;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
        }

        public async Task<DoctorByIdOutput> GetById(Guid Id)
        {
            DoctorByIdOutput doctorByIdOutput = new DoctorByIdOutput();
            try
            {
                if (Id != Guid.Empty && Id != null)
                {
                    var user = await _userRepository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.UniqueUserId == Id);
                    var roleIds = user.Roles.Select(x => x.RoleId).ToArray();
                    if (_roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName).Contains("CONSULTANT"))
                    {
                        var usermetadeta = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == Id && x.IsActive == true).FirstOrDefault();

                        if (user != null)
                        {
                            doctorByIdOutput.Gender = user.Gender == "0" ? "" : user.Gender;
                            doctorByIdOutput.ConsultantName = user.FullName;
                            doctorByIdOutput.Title = user.Title;
                            if (!string.IsNullOrEmpty(user.UploadProfilePicture))
                            {
                                if (user.IsBlobStorage)
                                {
                                    doctorByIdOutput.profileUrl = _blobContainer.Download(user.UploadProfilePicture);
                                }
                                else
                                {
                                    doctorByIdOutput.profileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + user.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                }
                            }
                            doctorByIdOutput.ConsultantId = user.UniqueUserId;
                            if (usermetadeta != null)
                            {
                                doctorByIdOutput.HospitalName = usermetadeta.HospitalAffiliation;
                                doctorByIdOutput.AffiliationSecondary = usermetadeta.CurrentAffiliation;
                                doctorByIdOutput.ProfessionalBio = usermetadeta.ProfessionalBio;
                                doctorByIdOutput.UndergraduateMedicalTraining = usermetadeta.UndergraduateMedicalTraining;
                                doctorByIdOutput.OncologySpecialty = usermetadeta.OncologySpecialty;
                                doctorByIdOutput.OncologySubSpecialty = usermetadeta.OncologySubSpecialty;
                                doctorByIdOutput.MedicalAssociationMembership = usermetadeta.MedicalAssociationMembership;
                                doctorByIdOutput.LicensingNumber = usermetadeta.LicensingNumber;

                            }
                            doctorByIdOutput.StatusCode = 200;
                            doctorByIdOutput.Message = "Get details successfully.";
                        }
                        else
                        {
                            doctorByIdOutput.StatusCode = 401;
                            doctorByIdOutput.Message = "No record found.";
                        }

                    }
                    else
                    {
                        doctorByIdOutput.StatusCode = 401;
                        doctorByIdOutput.Message = "No record found.";
                    }
                }
                else
                {
                    doctorByIdOutput.StatusCode = 401;
                    doctorByIdOutput.Message = "Bad Request.";
                }

            }
            catch (Exception ex)
            {
                doctorByIdOutput.Message = ex.Message;
                doctorByIdOutput.StatusCode = 500;
                Logger.Error("patient page GetById" + ex.StackTrace);
            }
            return doctorByIdOutput;
        }

        /// <summary>
        /// This function is use to search family and patient user
        /// </summary>
        /// <param name="input"> user first name and Last name</param>
        /// <returns>User details</returns>
        public async Task<FamilyDoctorOutput> GetByName(FamilyDoctorInput input)
        {
            FamilyDoctorOutput familyDoctorOutput = new FamilyDoctorOutput();
            try
            {
                string Roles = input.IsPatient == true ? "PATIENT" : "FAMILYDOCTOR";
                var user = await _userManager.GetUsersInRoleAsync(Roles);
                if (user != null)
                {
                    var list = new List<FamliyDoctorlist>();
                    if (input.IsPatient == true)
                    {
                        list = (from u in user.AsEnumerable()
                                where u.Name.ToLower().Contains(!string.IsNullOrEmpty(input.FirstName) ? input.FirstName.ToLower() : u.Name.ToLower())
                                && u.Surname.ToLower().Contains(!string.IsNullOrEmpty(input.LastName) ? input.LastName.ToLower() : u.Surname.ToLower())
                                && u.IsActive == true
                                select new FamliyDoctorlist
                                {
                                    DoctorFirstName = u.Name,
                                    DoctorLastName = u.Surname,
                                    DoctorEmailID = u.EmailAddress,
                                    DoctorAddress1 = u.Address,
                                    DoctorCity = u.City,
                                    DoctorState = u.State,
                                    DoctorCountry = u.Country,
                                    DoctorPostalCodes = u.PostalCode,
                                    DoctorDOB = u.DateOfBirth,
                                    FamilyDoctorId = u.UniqueUserId,
                                    DoctorTelePhone = u.PhoneNumber,
                                    DoctorGender = u.Gender,
                                    Title = u.Title
                                }).ToList();
                    }
                    else
                    {
                        if (input.UserId == Guid.Empty)
                        {
                            list = (from u in user.AsEnumerable()
                                    where u.Name.ToLower().Contains(!string.IsNullOrEmpty(input.FirstName) ? input.FirstName.ToLower() : u.Name.ToLower())
                                    && u.Surname.ToLower().Contains(!string.IsNullOrEmpty(input.LastName) ? input.LastName.ToLower() : u.Surname.ToLower())
                                    && u.IsActive == true
                                    select new FamliyDoctorlist
                                    {
                                        DoctorFirstName = u.Name,
                                        DoctorLastName = u.Surname,
                                        DoctorEmailID = u.EmailAddress,
                                        DoctorAddress1 = u.Address,
                                        DoctorCity = u.City,
                                        DoctorState = u.State,
                                        DoctorCountry = u.Country,
                                        DoctorPostalCodes = u.PostalCode,
                                        DoctorDOB = u.DateOfBirth,
                                        FamilyDoctorId = u.UniqueUserId,
                                        DoctorTelePhone = u.PhoneNumber,
                                        DoctorGender = u.Gender,
                                        Title = u.Title
                                    }).ToList();
                        }
                        else
                        {
                            list = (from u in user.AsEnumerable()
                                    join uc in _userconsentRepository.GetAll().Where(x => x.FamilyDoctorId != null).AsEnumerable()
                                    on u.UniqueUserId equals uc.FamilyDoctorId
                                    join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                    on uc.AppointmentId equals da.Id
                                    where u.Name.ToLower().Contains(!string.IsNullOrEmpty(input.FirstName) ? input.FirstName.ToLower() : u.Name.ToLower())
                                    && u.Surname.ToLower().Contains(!string.IsNullOrEmpty(input.LastName) ? input.LastName.ToLower() : u.Surname.ToLower())
                                    && u.IsActive == true && da.UserId == input.UserId && (da.IsBooked == 1 || da.IsBooked == 3)
                                    group u by u.UniqueUserId into g
                                    select new FamliyDoctorlist
                                    {
                                        DoctorFirstName = g.Select(x => x.Name).FirstOrDefault(),
                                        DoctorLastName = g.Select(x => x.Surname).FirstOrDefault(),
                                        DoctorEmailID = g.Select(x => x.EmailAddress).FirstOrDefault(),
                                        DoctorAddress1 = g.Select(x => x.Address).FirstOrDefault(),
                                        DoctorCity = g.Select(x => x.City).FirstOrDefault(),
                                        DoctorState = g.Select(x => x.State).FirstOrDefault(),
                                        DoctorCountry = g.Select(x => x.Country).FirstOrDefault(),
                                        DoctorPostalCodes = g.Select(x => x.PostalCode).FirstOrDefault(),
                                        DoctorDOB = g.Select(x => x.DateOfBirth).FirstOrDefault(),
                                        FamilyDoctorId = g.Key,
                                        DoctorTelePhone = g.Select(x => x.PhoneNumber).FirstOrDefault(),
                                        DoctorGender = g.Select(x => x.Gender).FirstOrDefault(),
                                        Title = g.Select(x => x.Title).FirstOrDefault()
                                    }).ToList();
                        }
                    }

                    familyDoctorOutput.Items = list;
                    familyDoctorOutput.StatusCode = 200;
                    familyDoctorOutput.Message = "Get details successfully.";
                }
                else
                {
                    familyDoctorOutput.StatusCode = 401;
                    familyDoctorOutput.Message = "No record found.";
                }
            }
            catch (Exception ex)
            {
                familyDoctorOutput.Message = ex.Message;
                familyDoctorOutput.StatusCode = 500;
                Logger.Error("patient page GetById" + ex.StackTrace);
            }
            return familyDoctorOutput;
        }

        public async Task<DoctorSearchOutput> Search(DoctorSearchInput input)
        {
            DoctorSearchOutput doctorSearchOutput = new DoctorSearchOutput();
            var slotlist = new List<DoctorAppointmentSlot>();
            string Type = string.Empty;
            try
            {
                if (AbpSession.UserId.HasValue)
                {
                    var user = await _userRepository.GetAsync(AbpSession.UserId.Value);
                    if (user != null)
                    {
                        Type = string.IsNullOrEmpty(user.UserType) ? "Patient" : user.UserType == "Insurance" ? "Medical Legal" : user.UserType == "MedicalLegal" ? "Medical Legal" : user.UserType;
                    }
                }
                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(input.TimeZone);

                var userlistbyspecialty = _userMetaDetailsRepository.GetAll().Include(x => x.Users).Where(x => x.IsActive == true && !string.IsNullOrEmpty(x.OncologySpecialty) && !string.IsNullOrEmpty(x.HospitalAffiliation))
                   .WhereIf(!input.SpecialtyName.IsNullOrWhiteSpace(), x => x.OncologySpecialty.ToLower().Contains(input.SpecialtyName.ToLower()))
                   .WhereIf(!input.HospitalName.IsNullOrWhiteSpace(), x => x.HospitalAffiliation.ToLower().Contains(input.HospitalName.ToLower())).ToList();
                var userlist = await _userManager.GetUsersInRoleAsync("CONSULTANT");
                slotlist = _doctorAppointmentSlotRepository.GetAll().Where(x => x.IsBooked == 0).ToList();
                DateTime newdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
                if (string.IsNullOrEmpty(input.NextAvailability))
                {
                    Logger.Info("Count" + Convert.ToString(slotlist.Count));
                    Logger.Info(newdate.ToString());
                    slotlist = slotlist.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi) >= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi)).OrderBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi).Date).ToList();
                    Logger.Info("Count" + Convert.ToString(slotlist.Count));
                }
                else if (Convert.ToDateTime(input.NextAvailability).Date == newdate.Date)
                {
                    slotlist = slotlist.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi).Date == newdate.Date && TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi) >= newdate).OrderBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi).Date).ToList();
                }
                else
                {
                    DateTime dateTime = Convert.ToDateTime(input.NextAvailability).Date;
                    slotlist = slotlist.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi).Date == dateTime.Date).OrderBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.SlotZoneTime, tzi).Date).ToList();

                }

                if (userlist != null)
                {

                    var finallist = (from u in userlist.AsEnumerable()
                                     join um in userlistbyspecialty.AsEnumerable()
                                     on u.UniqueUserId equals um.UserId
                                     join sl in slotlist.AsEnumerable()
                                     on u.UniqueUserId equals sl.UserId
                                     where u.FullName.ToLower().Contains(input.ConsultantName.ToLower())
                                     && um.ConsultationType.Contains(Type)
                                     group new { u.FullName, um.OncologySpecialty, um.HospitalAffiliation, u.UploadProfilePicture, u.IsBlobStorage, u.Title, sl } by new { u.UniqueUserId, TimeZoneInfo.ConvertTimeFromUtc(sl.SlotZoneTime, tzi).Date } into gr

                                     select new DoctorOutput
                                     {
                                         SpecialtyName = gr.Select(x => x.OncologySpecialty).FirstOrDefault(),
                                         HospitalName = gr.Select(x => x.HospitalAffiliation).FirstOrDefault(),
                                         ConsultantId = gr.Key.UniqueUserId,
                                         ConsultantName = gr.Select(x => x.FullName).FirstOrDefault(),
                                         profileUrl = gr.Select(x => x.IsBlobStorage).FirstOrDefault() == true ? _blobContainer.Download(gr.Select(x => x.UploadProfilePicture).FirstOrDefault()) : Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + gr.Select(x => x.UploadProfilePicture).FirstOrDefault(), _configuration[Utility.ENCRYPTION_KEY])),
                                         Title = gr.Select(x => x.Title).FirstOrDefault(),
                                         AvailabilitySlot = gr.Select(x => new TodaysAvailabilitySlotDto
                                         {
                                             AvailabilityDate = gr.Key.Date.ToString("MM/dd/yyyy"),
                                             timeslots = gr.Select(x => new Timeslots
                                             {
                                                 TimeZone = x.sl.TimeZone,
                                                 AvailabilityStartTime = x.sl.AvailabilityStartTime,
                                                 AvailabilityEndTime = x.sl.AvailabilityEndTime,
                                                 SlotStartTime = x.sl.SlotZoneTime,
                                                 SlotEndTime = x.sl.SlotZoneEndTime
                                             }).OrderBy(x => x.SlotStartTime).ToList()
                                         }).FirstOrDefault()

                                     }).GroupBy(x => x.ConsultantId, (key, g) => g.OrderBy(e => e.ConsultantId).First()).ToList();

                    doctorSearchOutput.Items = finallist;
                    doctorSearchOutput.Message = "Consultant list";
                    doctorSearchOutput.StatusCode = 200;
                }
                else
                {
                    doctorSearchOutput.Message = "No record find";
                    doctorSearchOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                doctorSearchOutput.Message = ex.Message;
                doctorSearchOutput.StatusCode = 500;
                Logger.Error("patient Search" + ex.StackTrace);
            }
            return doctorSearchOutput;
        }

        public async Task<PatientIntakeOutput> Create([FromForm] CreatePatientInput input)
        {
            Logger.Info("patient Intake : " + input);

            var stopwatch = Stopwatch.StartNew();
            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
            PatientIntakeOutput patientIntakeOutput = new PatientIntakeOutput();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            Guid? newfamilyId = null;
            try
            {
                if (input.UserConsentId != Guid.Empty && input.UserConsentId != null)
                {
                    var response = await Update(input);
                    patientIntakeOutput.Message = response.Message;
                    patientIntakeOutput.StatusCode = response.StatusCode;
                    patientIntakeOutput.UserConsetId = response.UserConsetId;
                }
                else
                {
                    if (input.Signature != null)
                    {

                        //using (var unitOfWork = _unitOfWorkManager.Begin())
                        //{
                        var newPatientId = await CreatePatientDetails(input);
                        if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
                        {
                            if (!string.IsNullOrEmpty(input.DoctorEmailID))
                            {
                                var registeredEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.DoctorEmailID && x.UserType == "FamilyDoctor");
                                if (registeredEmail != null)
                                {
                                    input.FamilyDoctorId = registeredEmail.UniqueUserId;
                                }
                                else
                                {
                                    newfamilyId = await InviteFamilyDoctor(input);
                                }

                            }

                        }
                        else if (input.FamilyDoctorId != Guid.Empty && input.FamilyDoctorId != null)
                        {
                            
                            //Send Notification to Family doctor

                            var familyDoctor = _userRepository.FirstOrDefault(x => x.UniqueUserId == input.FamilyDoctorId);
                            string Title = "Invite Family Doctor";
                            string Message = "You are associated with " + input.FirstName + " " + input.LastName + " for the appointment.";
                            _customNotificationAppService.NotificationPublish(familyDoctor.Id, Title, Message);

                            if (user != null)
                            {
                                var userMetaDetails = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == input.UserId);
                                if (userMetaDetails != null)
                                {
                                    userMetaDetails.FamilyDoctorId = input.FamilyDoctorId;
                                    await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                }

                            }
                        }

                        profileOutput = await UploadSignature(input.Signature, input.UserId, Guid.Empty);

                        var userconsetform = new UserConsentForm
                        {
                            ConsentName = input.FirstName + " " + input.LastName,
                            //SignaturePath = SignatureEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim(),
                            ConsentFormsMasterId = input.ConsentFormsMasterId,
                            CreatedOn = DateTime.UtcNow,
                            DateConfirmation = !string.IsNullOrEmpty(input.DateConfirmation) ? Convert.ToDateTime(input.DateConfirmation) : DateTime.UtcNow,
                            UserId = input.UserId
                        };
                        if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                        {
                            if (_uplodedFilePath.IsBlob)
                            {
                                userconsetform.SignaturePath = profileOutput.ProfilePath.Trim();
                                userconsetform.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                                userconsetform.VersionId = profileOutput.VersionId;
                                userconsetform.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                                userconsetform.EncryptionScope = profileOutput.EncryptionScope;
                                userconsetform.CreateRequestId = profileOutput.CreateRequestId;
                                userconsetform.IsBlobStorage = _uplodedFilePath.IsBlob;
                            }
                            else
                            {
                                userconsetform.SignaturePath = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                            }
                        }
                        if (_session.UniqueUserId != null)
                        {

                            userconsetform.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        userconsetform.TenantId = AbpSession.TenantId;
                        Guid newUserConsentFormId = await _userConsentFormRepository.InsertAndGetIdAsync(userconsetform);

                        var userconsent = new UserConsent
                        {
                            UserConsentPatientsDetailsId = newPatientId.PatientDetailsId,
                            //AppointmentId = input.AppointmentId,
                            UserConsentFormId = newUserConsentFormId,
                            CreatedOn = DateTime.UtcNow,
                            ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                            UserId = input.UserId,
                            FamilyDoctorId = input.FamilyDoctorId,
                            PatientId = newPatientId.PatientId
                        };
                        if (input.AppointmentId != Guid.Empty && input.AppointmentId != null)
                        {
                            userconsent.AppointmentId = input.AppointmentId;
                        }
                        if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
                        {
                            if (newfamilyId != null && newfamilyId != Guid.Empty)
                            {
                                userconsent.InviteUserId = newfamilyId;
                            }

                        }
                        if (_session.UniqueUserId != null)
                        {

                            userconsent.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        userconsent.TenantId = AbpSession.TenantId;

                        Guid newUserConsentId = await _userconsentRepository.InsertAndGetIdAsync(userconsent);

                        await UploadReports(input.ReportDocumentPaths, input.RadiationDocumentPaths, input.OtherDocumentPaths, newPatientId.PatientId, newUserConsentId);

                        var appointment = await _doctorAppointmentRepository.GetAsync(input.AppointmentId);
                        appointment.Status = "Intake";
                        appointment.IsBooked = 3;
                        appointment.MissedAppointment = false;
                        appointment.NoOfPages = input.NoOfPages;
                        await _doctorAppointmentRepository.UpdateAsync(appointment);
                        patientIntakeOutput.Message = "Your detail has been saved successfully.";
                        patientIntakeOutput.UserConsetId = newUserConsentId;

                        patientIntakeOutput.StatusCode = 200;
                        //    unitOfWork.Complete();
                        //}

                        stopwatch.Stop();

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(input);
                        createEventsInput.Operation = "Intake form has been submitted by user " + user.FullName;
                        createEventsInput.Component = "Patient";
                        createEventsInput.Action = "Create";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                    }
                    else
                    {
                        patientIntakeOutput.Message = "Signature is required.";
                        patientIntakeOutput.StatusCode = 401;
                    }
                }

            }
            catch (Exception ex)
            {
                patientIntakeOutput.Message = ex.Message;
                patientIntakeOutput.StatusCode = 500;
                Logger.Error("patient Intake" + ex.StackTrace);
            }
            return patientIntakeOutput;


        }

        public GetDocumentListOutput GetDocumentList(GetDocumentListInput input)
        {
            Logger.Info("Get Document List : " + input);
            GetDocumentListOutput output = new GetDocumentListOutput();
            try
            {
                if (input.AppoinmentId != Guid.Empty || input.UserId != Guid.Empty)
                {
                    var user = _userRepository.GetAllList();

                    if (input.AppoinmentId != Guid.Empty)
                    {
                        var userconsetlist = _userconsentRepository.GetAll().Where(x => x.AppointmentId == input.AppoinmentId).FirstOrDefault();
                        if (userconsetlist != null)
                        {

                            var documentlist = (from ud in _userDocumentRepository.GetAll().AsEnumerable()
                                                join nu in user.AsEnumerable()
                                                on ud.CreatedBy equals nu.UniqueUserId
                                                where ud.UserConsentsId == userconsetlist.Id && ud.IsActive == true
                                                orderby ud.CreatedOn descending
                                                select (new DocumentData
                                                {
                                                    DocumentId = ud.Id,
                                                    DocumentName = ud.Title,
                                                    DocumentDate = ud.CreatedOn,
                                                    UploadedBy = ud.CreatedBy,
                                                    RoleName = nu.UserType == "EmroAdmin" ? "Admin" : nu.UserType,
                                                    DocumentExtension = Path.GetExtension(ud.Path)
                                                })).ToList();
                            output.Count = documentlist.Count;
                            if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                            {
                                documentlist = documentlist.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                            }

                            output.Items = documentlist;
                            output.Message = "Get Document list successfully.";
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
                        //var user = _userRepository.GetAllList(x => x.UniqueUserId == input.UserId);
                        var documentlist1 = (from ud in _userDocumentRepository.GetAll().AsEnumerable()
                                             join u in user.AsEnumerable()
                                             on ud.UserId equals u.UniqueUserId
                                             join nu in user.AsEnumerable()
                                              on ud.CreatedBy equals nu.UniqueUserId
                                             where ud.UserId == input.UserId && ud.IsActive == true
                                             orderby ud.CreatedOn descending
                                             select (new DocumentData
                                             {
                                                 DocumentId = ud.Id,
                                                 DocumentName = ud.Title,
                                                 DocumentDate = ud.CreatedOn,
                                                 UploadedBy = ud.CreatedBy,
                                                 RoleName = nu.UserType == "EmroAdmin" ? "Admin" : nu.UserType,
                                                 DocumentExtension = Path.GetExtension(ud.Path)
                                             })).ToList();
                        output.Count = documentlist1.Count;
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Count > 0)
                        {
                            documentlist1 = documentlist1.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }
                        output.Items = documentlist1;
                        output.Message = "Get Document list successfully.";
                        output.StatusCode = 200;
                    }


                }
                else
                {
                    output.Message = "No record found.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.StatusCode = 500;
                Logger.Error("Get Document List" + ex.StackTrace);
            }
            return output;
        }

        public GetDocumentListOutput GetIntakeConsultantReportsByPatientId(GetIntakeConsultantReportsByPatientIdInput input)
        {
            Logger.Info("Get Document List : " + input);
            GetDocumentListOutput output = new GetDocumentListOutput();
            try
            {
                if (input.UserId != Guid.Empty)
                {
                    var user = _userRepository.GetAllList();

                    if (input.AppointmentId != Guid.Empty)
                    {
                        var userconsetlist = _userconsentRepository.GetAll().Where(x => x.UserId == input.UserId && x.AppointmentId == input.AppointmentId).ToList();
                        if (userconsetlist != null)
                        {
                            var ConsultantReportsIdList = (from ud in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                                           join nu in userconsetlist.AsEnumerable()
                                                           on ud.Id equals nu.UserConsentPatientsDetailsId
                                                           select ud.ConsultantReportsIds).ToList();

                            var ConsultantReportsGuidList = new List<string>();
                            ConsultantReportsIdList.Where(e => e != null).Select(e => e!).ToList().ForEach(x => {
                                x.Split(',').ToList().ForEach(y => { ConsultantReportsGuidList.Add(y); });
                            });
                            var FilteredConsultantReportsGuidList = new HashSet<string>(ConsultantReportsGuidList);

                            var documentlist = (from fcr in FilteredConsultantReportsGuidList
                                                join cr in _consultReportRepository.GetAllList().AsEnumerable()
                                                on new Guid(fcr) equals cr.Id
                                                join a in _doctorAppointmentRepository.GetAllList().AsEnumerable()
                                                on cr.AppointmentId equals a.Id
                                                join u in user.AsEnumerable()
                                                on a.DoctorId equals u.UniqueUserId
                                                select (new DocumentData
                                                {
                                                    DocumentId = cr.Id,
                                                    DocumentName = u.FullName,
                                                    DocumentDate = cr.CreatedOn,
                                                    UploadedBy = cr.CreatedBy,
                                                    RoleName = u.UserType == "EmroAdmin" ? "Admin" : u.UserType,
                                                    DocumentExtension = Path.GetExtension(cr.ReportPath)
                                                })).ToList();

                            output.Items = documentlist;
                            output.Message = "Get Document list successfully.";
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

                        var userconsetlist = _userconsentRepository.GetAll().Where(x => x.PatientId == input.UserId).ToList();
                        if (userconsetlist != null)
                        {
                            var ConsultantReportsIdList = (from ud in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                                           join nu in userconsetlist.AsEnumerable()
                                                           on ud.Id equals nu.UserConsentPatientsDetailsId
                                                           select ud.ConsultantReportsIds).ToList();

                            var ConsultantReportsGuidList = new List<string>();
                            ConsultantReportsIdList.Where(e => e != null).Select(e => e!).ToList().ForEach(x => {
                                x.Split(',').ToList().ForEach(y => { ConsultantReportsGuidList.Add(y); });
                            });
                            var FilteredConsultantReportsGuidList = new HashSet<string>(ConsultantReportsGuidList.Where(e => e != null && !e.IsNullOrEmpty()).Select(e => e!).ToList());

                            var documentlist = (from fcr in FilteredConsultantReportsGuidList
                                                join cr in _consultReportRepository.GetAllList().AsEnumerable()
                                                on new Guid(fcr) equals cr.Id
                                                join u in user.AsEnumerable()
                                                on cr.CreatedBy equals u.UniqueUserId
                                                select (new DocumentData
                                                {
                                                    DocumentId = cr.Id,
                                                    DocumentName = u.FullName,
                                                    DocumentDate = cr.CreatedOn,
                                                    UploadedBy = cr.CreatedBy,
                                                    RoleName = u.UserType == "EmroAdmin" ? "Admin" : u.UserType,
                                                    DocumentExtension = Path.GetExtension(cr.ReportPath)
                                                })).ToList();

                            output.Items = documentlist;
                            output.Message = "Get Document list successfully.";
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
                    output.Message = "No record found.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.StatusCode = 500;
                Logger.Error("Get Document List" + ex.StackTrace);
            }
            return output;
        }

        [HttpPost]
        public async Task<GetIntakeDetailsOutputDto> GetDetails(GetIntakeInput input)
        {
            GetIntakeDetailsOutputDto output = new GetIntakeDetailsOutputDto();
            try
            {
                if (input.AppoinmentId != Guid.Empty)
                {
                    var userconsetlist = _userconsentRepository.GetAll().Where(x => x.AppointmentId == input.AppoinmentId).FirstOrDefault();
                    var appointments = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppoinmentId);
                    var users = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointments.UserId);
                    if (users != null)
                    {
                        output.PatientId = users.UserType == "Patient" ? users.UniqueUserId : userconsetlist != null ? userconsetlist.PatientId : null;
                    }
                    var consentReport = await _consultReportRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppoinmentId);
                    if (consentReport != null)
                    {
                        output.ConsultId = consentReport.Id.ToString();
                    }
                    if (userconsetlist != null)
                    {
                        output.UserConsentId = userconsetlist.Id;
                        var patientDetails = await _userConsentPatientsDetailsRepository.GetAsync(userconsetlist.UserConsentPatientsDetailsId);
                        if (patientDetails != null)
                        {
                            output.FirstName = patientDetails.FirstName;
                            output.LastName = patientDetails.LastName;
                            output.Address = patientDetails.Address;
                            output.City = patientDetails.City;
                            output.State = patientDetails.State;
                            output.Country = patientDetails.Country;
                            output.PostalCode = patientDetails.PostalCode;
                            output.TelePhone = patientDetails.TelePhone;
                            output.EmailID = patientDetails.EmailID;
                            output.Gender = patientDetails.Gender;
                            output.ReasonForConsult = patientDetails.ReasonForConsult;
                            output.DiseaseDetails = patientDetails.DiseaseDetails;
                            output.DateOfBirth = patientDetails.DateOfBirth;
                            output.ConsultantReportsIds = patientDetails.ConsultantReportsIds.Split(',');
                            output.RelationshipWithPatient = patientDetails.RelationshipWithPatient;
                            output.RepresentativeFirstName = patientDetails.RepresentativeFirstName;
                            output.RepresentativeLastName = patientDetails.RepresentativeLastName;
                        }
                        if (userconsetlist.FamilyDoctorId != null && userconsetlist.FamilyDoctorId != Guid.Empty)
                        {
                            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == userconsetlist.FamilyDoctorId);
                            if (user != null)
                            {
                                output.DoctorFirstName = user.Name;
                                output.DoctorLastName = user.Surname;
                                output.DoctorAddress = user.Address;
                                output.DoctorCity = user.City;
                                output.DoctorState = user.State;
                                output.DoctorCountry = user.Country;
                                output.DoctorPostalCodes = user.PostalCode;
                                output.DoctorTelePhone = user.PhoneNumber;
                                output.DoctorEmailID = user.EmailAddress;
                                output.DoctorGender = user.Gender;
                                output.FamilyDoctorId = user.UniqueUserId;
                                output.Title = user.Title;
                                output.DoctorDateOfBirth = user.DateOfBirth;
                            }
                        }
                        else if (userconsetlist.InviteUserId != null && userconsetlist.InviteUserId != Guid.Empty)
                        {
                            var Inviteuser = await _inviteUserRepository.FirstOrDefaultAsync(x => x.Id == userconsetlist.InviteUserId);
                            if (Inviteuser != null)
                            {
                                output.DoctorFirstName = Inviteuser.FirstName;
                                output.DoctorLastName = Inviteuser.LastName;
                                output.DoctorAddress = Inviteuser.Address;
                                output.DoctorCity = Inviteuser.City;
                                output.DoctorState = Inviteuser.State;
                                output.DoctorCountry = Inviteuser.Country;
                                output.DoctorPostalCodes = Inviteuser.PostalCodes;
                                output.DoctorTelePhone = Inviteuser.TelePhone;
                                output.DoctorEmailID = Inviteuser.EmailAddress;
                                output.DoctorGender = Inviteuser.Gender;
                                output.DoctorDateOfBirth = Inviteuser.DateOfBirth;
                                output.Title = "Dr.";
                            }
                        }

                        output.ConsentMedicalInformationWithCancerCareProvider = userconsetlist.ConsentMedicalInformationWithCancerCareProvider;
                        output.AppointmentId = (Guid)userconsetlist.AppointmentId;
                        var consent = await _userConsentFormRepository.FirstOrDefaultAsync(x => x.Id == userconsetlist.UserConsentFormId);
                        if (consent != null)
                        {
                            output.ConsentFormsMasterId = (Guid)consent.ConsentFormsMasterId;
                            output.DateConfirmation = consent.DateConfirmation.ToString();
                            if (!string.IsNullOrEmpty(consent.SignaturePath))
                            {
                                if (consent.IsBlobStorage)
                                {
                                    output.Signature = await _blobContainer.DownloadSignature(consent.SignaturePath);
                                    //output.Signature = "";
                                }
                                else
                                {
                                    output.Signature = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + consent.SignaturePath, _configuration[Utility.ENCRYPTION_KEY]));

                                }
                            }
                        }
                        var appoinment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == userconsetlist.AppointmentId);
                        if (appoinment != null)
                        {
                            output.NoOfPages = appoinment.NoOfPages;
                        }

                        if (users.UserType == "Insurance" || users.UserType == "MedicalLegal")
                        {
                            output.Items = (from ud in _userDocumentRepository.GetAll().AsEnumerable()
                                            where (ud.UserConsentsId == userconsetlist.Id) && ud.IsActive == true
                                            orderby ud.CreatedOn descending
                                            select (new DocumentData
                                            {
                                                DocumentId = ud.Id,
                                                DocumentName = ud.Title,
                                                DocumentDate = ud.CreatedOn,
                                            })).ToList();
                        }

                        output.Message = "Get Intake details.";
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
                output.Message = ex.Message;
                output.StatusCode = 500;
                Logger.Error("Get Details " + ex.StackTrace);
            }
            return output;
        }

        protected async Task<PatientDetailsoutput> CreatePatientDetails(CreatePatientInput input)
        {
            PatientDetailsoutput output = new PatientDetailsoutput();

            var patient = new UserConsentPatientsDetails
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Address = input.Address,
                City = input.City,
                State = input.State,
                Country = input.Country,
                PostalCode = input.PostalCode,
                TelePhone = input.TelePhone,
                EmailID = input.EmailID == null ? "" : input.EmailID,
                Gender = input.Gender,
                ReasonForConsult = input.ReasonForConsult,
                DiseaseDetails = input.DiseaseDetails,
                CreatedOn = DateTime.UtcNow,
                UserId = input.UserId,
                ConsultantReportsIds = input.ConsultantReportsIds == null ? "" : string.Join(",", input.ConsultantReportsIds),
                RelationshipWithPatient = input.RelationshipWithPatient,
                RepresentativeFirstName = input.RepresentativeFirstName,
                RepresentativeLastName = input.RepresentativeLastName
            };
            if (!string.IsNullOrEmpty(input.DateOfBirth))
            {
                patient.DateOfBirth = input.DateOfBirth;
            }
            if (_session.UniqueUserId != null)
            {

                patient.CreatedBy = Guid.Parse(_session.UniqueUserId);
            }
            patient.TenantId = AbpSession.TenantId;
            Guid newPatientId = await _userConsentPatientsDetailsRepository.InsertAndGetIdAsync(patient);

            output.PatientDetailsId = newPatientId;

            if (!String.IsNullOrEmpty(input.EmailID))
            {
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailID);

                if (IsEmailExists != null)
                {
                    output.PatientId = IsEmailExists.UniqueUserId;
                }
                else
                {
                    /**
                     * Stop creating patient record during intake form.
                     */
                    //if (input.IsPatient == false)
                    //{
                    //    string newPassword = User.CreateRandomPassword();
                    //    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    //    string userName = GenrateUserName(input.FirstName, input.LastName);
                    //    var user = new User
                    //    {
                    //        EmailAddress = input.EmailID,
                    //        UserName = userName,
                    //        IsActive = true,
                    //        IsEmailConfirmed = true,
                    //        Password = newPassword,
                    //        Name = input.FirstName,
                    //        Surname = input.LastName,
                    //        Address = input.Address,
                    //        DateOfBirth = input.DateOfBirth,
                    //        Gender = input.Gender,
                    //        City = input.City,
                    //        State = input.State,
                    //        Country = input.Country,
                    //        PostalCode = input.PostalCode,
                    //        PhoneNumber = input.TelePhone,
                    //        UniqueUserId = Guid.NewGuid(),
                    //        Timezone = "AAmerica/Toronto"

                    //    };


                    //    user.TenantId = AbpSession.TenantId;
                    //    user.IsEmailConfirmed = true;

                    //    user.UserType = UserType.Patient.ToString();

                    //    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    //    await _userManager.CreateAsync(user, newPassword);
                    //    string[] RoleNames = { "Patient" };
                    //    await _userManager.SetRolesAsync(user, RoleNames);

                    //    CurrentUnitOfWork.SaveChanges();
                    //    string Name = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                    //    string message = "You can login to the <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                     + " <br /><br /> " + "UserName: " + userName
                    //                     + " <br /> " + "Password: " + newPassword;
                    //    string body = _templateAppService.GetTemplates(Name, message);
                    //    //string body = "Hello and Welcome " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //    //      + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //    //                 + " <br /><br /> " + "UserName: " + userName
                    //    //                 + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //    //                 + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                    //    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailID, "EMRO login credentials", body, adminmail));
                    //    var newUser = await _userManager.FindByEmailAsync(input.EmailID);
                    //    output.PatientId = user.UniqueUserId;
                    //}
                    //else
                    //{
                    output.PatientId = input.UserId;
                    //}
                }
            }

            return output;
        }

        protected string GenrateUserName(string firstName, string LastName)
        {
            string userName = string.Empty;
            string lname = LastName.Length >= 4 ? LastName.Substring(0, 4) : LastName.Substring(0, LastName.Length);
            userName = firstName.Substring(0, 1) + lname + GenerateRandomNo();
            var IsUserExists = _userManager.FindByNameOrEmail(userName);
            if (IsUserExists != null)
            {
                GenrateUserName(firstName, LastName);
            }
            return userName;
        }

        protected int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }
        protected async Task<Guid> InviteFamilyDoctor(CreatePatientInput input)
        {
            string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
            Guid newfamilyId = Guid.Empty;
            if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
            {
                var invite = await _inviteUserRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.DoctorEmailID && x.UserType == "FamilyDoctor");
                if (invite != null)
                {
                    newfamilyId = invite.Id;
                }
                else
                {
                    var familyDoctor = new InviteUser
                    {
                        FirstName = input.DoctorFirstName,
                        LastName = input.DoctorLastName,
                        Address = input.DoctorAddress,
                        City = input.DoctorCity,
                        State = input.DoctorState,
                        Country = input.DoctorCountry,
                        PostalCodes = input.DoctorPostalCodes,
                        TelePhone = input.DoctorTelePhone,
                        EmailAddress = input.DoctorEmailID,
                        CreatedOn = DateTime.UtcNow,
                        UserType = UserType.FamilyDoctor.ToString(),
                        //UserConsentPatientsDetailsId = newPatientId,
                        Gender = input.DoctorGender,
                        ReferedBy = input.UserId,
                        Status = "Pending",
                        IsActive = true
                    };
                    if (!string.IsNullOrEmpty(input.DoctorDateOfBirth))
                    {
                        familyDoctor.DateOfBirth = input.DoctorDateOfBirth;
                    }
                    if (_session.UniqueUserId != null)
                    {

                        familyDoctor.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    familyDoctor.TenantId = AbpSession.TenantId;
                    newfamilyId = await _inviteUserRepository.InsertAndGetIdAsync(familyDoctor);

                    string body1 = string.Empty;
                    string Name = string.Empty;
                    string message = string.Empty;
                    var user1 = await _userManager.GetUsersInRoleAsync("ADMIN");
                    var ur = user1.FirstOrDefault();
                    //string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    if (!string.IsNullOrEmpty(input.DoctorTelePhone))
                    {
                        Name = "ETeleHealth Admin";
                        message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                   + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                   + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                   // + "Hospital :-  " + input.Hospital + " <br />"
                                   + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                   + "Phone Number :-  " + input.DoctorTelePhone;
                        body1 = _templateAppService.GetTemplates(Name, message,"");
                        //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                        //          + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                        //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                        //           + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                        //           // + "Hospital :-  " + input.Hospital + " <br />"
                        //           + "Email Address :-  " + input.DoctorEmailID + " <br />"
                        //           + "Phone Number :-  " + input.DoctorTelePhone + " <br /><br />"
                        //           + "Regards," + " <br />" + "EMRO Team";
                    }
                    else
                    {
                        Name = "ETeleHealth Admin";
                        message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                   + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                   + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                   // + "Hospital :-  " + input.Hospital + " <br />"
                                   + "Email Address :-  " + input.DoctorEmailID;
                        //  + "Phone Number :-  " + input.DoctorTelePhone;
                        body1 = _templateAppService.GetTemplates(Name, message,"");
                        //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                        //             + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                        //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                        //             + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                        //             //+ "Hospital :-  " + input.Hospital + " <br />"
                        //             + "Email Address :-  " + input.DoctorEmailID + " <br /><br />"
                        //             //+ "PhoneNumber:-  " + input.PhoneNumber + " <br />"
                        //             + "Regards," + " <br />" + "EMRO Team";
                    }

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for FamilyDoctor", body1, adminmail));

                    string doctorName = input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase();
                    string doctormessage = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
                                 "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
                                 "If you have any questions regarding the invitation from " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                    string doctorbody = _templateAppService.GetTemplates(doctorName, doctormessage,"Welcome aboard,<br />ETeleHealth Team");

                    //string doctorbody = "Hello " + "<b>" + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + ",</b> <br /><br /> "
                    //                  + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                    //                 "also you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                    //                + "Regards," + " <br />" + "EMRO Team";

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.DoctorEmailID, "Invite to join ETeleHealth", doctorbody, adminmail));
                }

            }

            return newfamilyId;
        }

        protected async Task<UploadProfileOutput> UploadSignature(IFormFile formFile, Guid? UserId, Guid? UserConsentFormId)
        {
            string SignaturefileExt = string.Empty;
            string SignatureFileName = string.Empty;
            bool SignatureEncryptFileStatus = false;
            string SignatureEncryptedFilePath = string.Empty;
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string filePath = string.Empty;
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            if (formFile != null)
            {
                if (UserConsentFormId != Guid.Empty && UserConsentFormId != null)
                {
                    var consentform = await _userConsentFormRepository.GetAsync((Guid)UserConsentFormId);
                    if (consentform != null)
                    {
                        string path = _env.ContentRootPath + consentform.SignaturePath;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        //bool isStatus = _blobContainer.Delete(consentform.SignaturePath);
                    }
                }
                Guid Signatureobj = Guid.NewGuid();
                SignaturefileExt = Path.GetExtension(formFile.FileName);
                SignatureFileName = Signatureobj.ToString() + SignaturefileExt;
                if (_uplodedFilePath.IsBlob)
                {
                    filePath = Path.Combine(UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobSignatures);
                    filePath = filePath + SignatureFileName;
                    uploadDocumentInput.azureDirectoryPath = filePath;
                    uploadDocumentInput.Document = formFile;
                    var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                    if (blobResponse != null)
                    {
                        profileOutput.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                        profileOutput.VersionId = blobResponse.Value.VersionId;
                        profileOutput.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                        profileOutput.EncryptionScope = blobResponse.Value.EncryptionScope;
                        profileOutput.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                        profileOutput.ProfilePath = filePath;
                    }
                }
                else
                {
                    filePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.Signatures + UserId.ToString() + _uplodedFilePath.Slash);
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

                    profileOutput.ProfilePath = SignatureEncryptedFilePath;
                }

            }
            return profileOutput;
        }

        protected async Task UploadReports(List<IFormFile> ReportDocumentPaths, List<IFormFile> RadiationDocumentPaths, List<IFormFile> OtherDocumentPaths, Guid? UserId, Guid newUserConsentId)
        {
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string DocumentfileExt = string.Empty;
            bool DocumentEncryptFileStatus = false;
            string DocumentEncryptedFilePath = string.Empty;
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            if (ReportDocumentPaths != null && ReportDocumentPaths.Count > 0)
            {
                var stopwatch = Stopwatch.StartNew();
                foreach (IFormFile item in ReportDocumentPaths)
                {
                    Guid updobj = Guid.NewGuid();
                    string upfilePath = string.Empty;
                    DocumentfileExt = Path.GetExtension(item.FileName);
                    string titlename = Path.GetFileName(item.FileName);
                    string mimetype = item.ContentType;
                    string upfilename = updobj.ToString() + DocumentfileExt;
                    if (_uplodedFilePath.IsBlob)
                    {
                        upfilePath = Path.Combine(UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobDocumentPath + "Report" + _uplodedFilePath.Slash);
                        upfilePath = upfilePath + upfilename;
                        uploadDocumentInput.azureDirectoryPath = upfilePath;
                        uploadDocumentInput.Document = item;
                        var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                        if (blobResponse != null)
                        {
                            profileOutput.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                            profileOutput.VersionId = blobResponse.Value.VersionId;
                            profileOutput.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                            profileOutput.EncryptionScope = blobResponse.Value.EncryptionScope;
                            profileOutput.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                            profileOutput.ProfilePath = upfilePath;
                        }
                    }
                    else
                    {
                        upfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.DocumentPath + UserId.ToString() + _uplodedFilePath.Slash + "Reports\\");
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
                        profileOutput.ProfilePath = DocumentEncryptedFilePath;
                    }


                    var userDocument = new UserDocument
                    {
                        Category = "Report",
                        UserConsentsId = newUserConsentId,
                        // Path = DocumentEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim(),
                        MimeType = mimetype,
                        Title = titlename,
                        IsActive = true,
                        Status = "Uploaded",
                        CreatedOn = DateTime.UtcNow,
                        UserId = UserId
                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            userDocument.Path = profileOutput.ProfilePath;
                            userDocument.BlobSequenceNumber = profileOutput.BlobSequenceNumber.ToString();
                            userDocument.VersionId = profileOutput.VersionId;
                            userDocument.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            userDocument.EncryptionScope = profileOutput.EncryptionScope;
                            userDocument.CreateRequestId = profileOutput.CreateRequestId;
                            userDocument.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            userDocument.Path = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                        }
                    }

                    if (_session.UniqueUserId != null)
                    {

                        userDocument.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    userDocument.TenantId = AbpSession.TenantId;
                    var resultreport = await _userDocumentRepository.InsertAsync(userDocument);

                    //Log Audit Events
                    stopwatch.Stop();
                    var uploadedUser = _userRepository.FirstOrDefault(x => x.UniqueUserId == Guid.Parse(_session.UniqueUserId));
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(userDocument);
                    createEventsInput.Operation = " Report has uploaded successfully by " + uploadedUser.FullName;
                    createEventsInput.Component = "Patient";
                    createEventsInput.Action = "Upload Report";
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                }

            }

            if (RadiationDocumentPaths != null && RadiationDocumentPaths.Count > 0)
            {
                foreach (IFormFile item in RadiationDocumentPaths)
                {
                    Guid radiationdobj = Guid.NewGuid();
                    string upradiationfilePath = string.Empty;
                    string radiationDocumentfileExt = Path.GetExtension(item.FileName);
                    string raditationupfilename = radiationdobj.ToString() + radiationDocumentfileExt;
                    string rdtitlename = Path.GetFileName(item.FileName);
                    string rdmimetype = item.ContentType;
                    if (_uplodedFilePath.IsBlob)
                    {
                        upradiationfilePath = Path.Combine(UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobDocumentPath + "Radiation" + _uplodedFilePath.Slash);
                        upradiationfilePath = upradiationfilePath + raditationupfilename;
                        uploadDocumentInput.azureDirectoryPath = upradiationfilePath;
                        uploadDocumentInput.Document = item;
                        var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                        if (blobResponse != null)
                        {
                            profileOutput.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                            profileOutput.VersionId = blobResponse.Value.VersionId;
                            profileOutput.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                            profileOutput.EncryptionScope = blobResponse.Value.EncryptionScope;
                            profileOutput.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                            profileOutput.ProfilePath = upradiationfilePath;

                        }
                    }
                    else
                    {
                        upradiationfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.DocumentPath + UserId.ToString() + _uplodedFilePath.Slash + "Radiation\\");
                        if (!Directory.Exists(upradiationfilePath))
                        {
                            DirectoryInfo directoryInfo = Directory.CreateDirectory(upradiationfilePath);
                        }
                        string radiationDocumentEncryptedFilePath = upradiationfilePath + "Enc_" + raditationupfilename;
                        upradiationfilePath = upradiationfilePath + raditationupfilename;

                        using (var stream = new FileStream(upradiationfilePath, FileMode.Create, FileAccess.Write))
                        {
                            await item.CopyToAsync(stream);
                            stream.Flush();

                        }
                        DocumentEncryptFileStatus = Utility.EncryptFile(upradiationfilePath, radiationDocumentEncryptedFilePath, EncryptedKey);
                        if (DocumentEncryptFileStatus)
                        {

                            if (System.IO.File.Exists(upradiationfilePath))
                            {
                                System.IO.File.Delete(upradiationfilePath);
                            }
                        }
                        profileOutput.ProfilePath = radiationDocumentEncryptedFilePath;
                    }
                    var userDocument = new UserDocument
                    {
                        Category = "Radiation",
                        UserConsentsId = newUserConsentId,
                        //Path = radiationDocumentEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim(),
                        MimeType = rdmimetype,
                        Title = rdtitlename,
                        IsActive = true,
                        Status = "Uploaded",
                        CreatedOn = DateTime.UtcNow,
                        UserId = UserId
                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            userDocument.Path = profileOutput.ProfilePath;
                            userDocument.BlobSequenceNumber = profileOutput.BlobSequenceNumber.ToString();
                            userDocument.VersionId = profileOutput.VersionId;
                            userDocument.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            userDocument.EncryptionScope = profileOutput.EncryptionScope;
                            userDocument.CreateRequestId = profileOutput.CreateRequestId;
                            userDocument.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            userDocument.Path = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                        }
                    }
                    if (_session.UniqueUserId != null)
                    {

                        userDocument.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    userDocument.TenantId = AbpSession.TenantId;
                    var resultraditation = await _userDocumentRepository.InsertAsync(userDocument);
                }

            }

            if (OtherDocumentPaths != null && OtherDocumentPaths.Count > 0)
            {
                foreach (IFormFile item in OtherDocumentPaths)
                {
                    Guid otherupdobj = Guid.NewGuid();
                    string otherupfilePath = string.Empty;
                    string otherDocumentfileExt = Path.GetExtension(item.FileName);
                    string othherupfilename = otherupdobj.ToString() + otherDocumentfileExt;
                    string ottitlename = Path.GetFileName(item.FileName);
                    string otmimetype = item.ContentType;
                    if (_uplodedFilePath.IsBlob)
                    {
                        otherupfilePath = Path.Combine(UserId.ToString() + _uplodedFilePath.Slash + _uplodedFilePath.BlobDocumentPath + "Other" + _uplodedFilePath.Slash);
                        otherupfilePath = otherupfilePath + othherupfilename;
                        uploadDocumentInput.azureDirectoryPath = otherupfilePath;
                        uploadDocumentInput.Document = item;
                        var blobResponse = await _blobContainer.Upload(uploadDocumentInput);
                        if (blobResponse != null)
                        {
                            profileOutput.BlobSequenceNumber = blobResponse.Value.BlobSequenceNumber.ToString();
                            profileOutput.VersionId = blobResponse.Value.VersionId;
                            profileOutput.EncryptionKeySha256 = blobResponse.Value.EncryptionKeySha256;
                            profileOutput.EncryptionScope = blobResponse.Value.EncryptionScope;
                            profileOutput.CreateRequestId = blobResponse.GetRawResponse().ClientRequestId;
                            profileOutput.ProfilePath = otherupfilePath;

                        }
                    }
                    else
                    {
                        otherupfilePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.DocumentPath + UserId.ToString() + _uplodedFilePath.Slash + "Others\\");
                        if (!Directory.Exists(otherupfilePath))
                        {
                            DirectoryInfo directoryInfo = Directory.CreateDirectory(otherupfilePath);
                        }
                        string OtherDocumentEncryptedFilePath = otherupfilePath + "Enc_" + othherupfilename;
                        otherupfilePath = otherupfilePath + othherupfilename;

                        using (var stream = new FileStream(otherupfilePath, FileMode.Create, FileAccess.Write))
                        {
                            await item.CopyToAsync(stream);
                            stream.Flush();

                        }
                        DocumentEncryptFileStatus = Utility.EncryptFile(otherupfilePath, OtherDocumentEncryptedFilePath, EncryptedKey);
                        if (DocumentEncryptFileStatus)
                        {

                            if (System.IO.File.Exists(otherupfilePath))
                            {
                                System.IO.File.Delete(otherupfilePath);
                            }
                        }
                        profileOutput.ProfilePath = OtherDocumentEncryptedFilePath;
                    }

                    var userDocument = new UserDocument
                    {
                        Category = "other",
                        UserConsentsId = newUserConsentId,
                        // Path = OtherDocumentEncryptedFilePath.Replace(_env.ContentRootPath, "").Trim(),
                        MimeType = otmimetype,
                        Title = ottitlename,
                        IsActive = true,
                        Status = "Uploaded",
                        CreatedOn = DateTime.UtcNow,
                        UserId = UserId
                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            userDocument.Path = profileOutput.ProfilePath;
                            userDocument.BlobSequenceNumber = profileOutput.BlobSequenceNumber.ToString();
                            userDocument.VersionId = profileOutput.VersionId;
                            userDocument.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            userDocument.EncryptionScope = profileOutput.EncryptionScope;
                            userDocument.CreateRequestId = profileOutput.CreateRequestId;
                            userDocument.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            userDocument.Path = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                        }
                    }
                    if (_session.UniqueUserId != null)
                    {
                        userDocument.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    userDocument.TenantId = AbpSession.TenantId;
                    var resultother = await _userDocumentRepository.InsertAsync(userDocument);
                }

            }
        }

        public async Task<PatientIntakeOutput> Update([FromForm] CreatePatientInput input)
        {
            Logger.Info("patient Intake : " + input);

            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
            var stopwatch = Stopwatch.StartNew();

            UploadProfileOutput profileOutput = new UploadProfileOutput();
            PatientIntakeOutput patientIntakeOutput = new PatientIntakeOutput();
            Guid? newfamilyId = null;
            try
            {

                if (input.Signature != null)
                {
                    var userconsents = await _userconsentRepository.GetAsync(input.UserConsentId);
                    if (userconsents != null)
                    {
                        var newPatientId = await UpdatePatientDetails(input);
                        if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
                        {
                            if (!string.IsNullOrEmpty(input.DoctorEmailID))
                            {
                                var registeredEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.DoctorEmailID && x.UserType == "FamilyDoctor");
                                if (registeredEmail != null)
                                {
                                    input.FamilyDoctorId = registeredEmail.UniqueUserId;
                                }
                                else
                                {
                                    newfamilyId = await UpdateInviteFamilyDoctor(input);
                                }

                            }

                        }
                        else if (input.FamilyDoctorId != Guid.Empty && input.FamilyDoctorId == null)
                        {
                            if (user != null)
                            {
                                var userMetaDetails = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == input.UserId);
                                if (userMetaDetails != null)
                                {
                                    userMetaDetails.FamilyDoctorId = input.FamilyDoctorId;
                                    await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                }
                            }
                        }

                        profileOutput = await UploadSignature(input.Signature, input.UserId, userconsents.UserConsentFormId);

                        var consentform = await _userConsentFormRepository.GetAsync((Guid)userconsents.UserConsentFormId);
                        if (consentform != null)
                        {
                            consentform.ConsentName = input.FirstName + " " + input.LastName;

                            if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                            {
                                if (_uplodedFilePath.IsBlob)
                                {
                                    consentform.SignaturePath = profileOutput.ProfilePath.Trim();
                                    consentform.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                                    consentform.VersionId = profileOutput.VersionId;
                                    consentform.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                                    consentform.EncryptionScope = profileOutput.EncryptionScope;
                                    consentform.CreateRequestId = profileOutput.CreateRequestId;
                                    consentform.IsBlobStorage = _uplodedFilePath.IsBlob;
                                }
                                else
                                {
                                    consentform.SignaturePath = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "").Trim();
                                }
                            }

                            consentform.ConsentFormsMasterId = input.ConsentFormsMasterId;
                            consentform.CreatedOn = DateTime.UtcNow;
                            consentform.DateConfirmation = !string.IsNullOrEmpty(input.DateConfirmation) ? Convert.ToDateTime(input.DateConfirmation) : DateTime.UtcNow;
                            consentform.UserId = input.UserId;
                        }

                        if (_session.UniqueUserId != null)
                        {

                            consentform.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userConsentFormRepository.UpdateAsync(consentform);

                        await UploadReports(input.ReportDocumentPaths, input.RadiationDocumentPaths, input.OtherDocumentPaths, newPatientId.PatientId, userconsents.Id);
                        userconsents.UserConsentPatientsDetailsId = newPatientId.PatientDetailsId;
                        userconsents.AppointmentId = input.AppointmentId;
                        userconsents.CreatedOn = DateTime.UtcNow;
                        userconsents.ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider;
                        userconsents.UserId = input.UserId;
                        userconsents.FamilyDoctorId = input.FamilyDoctorId;
                        userconsents.PatientId = newPatientId.PatientId;
                        if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
                        {
                            if (newfamilyId != null && newfamilyId != Guid.Empty)
                            {
                                userconsents.InviteUserId = newfamilyId;
                            }

                        }
                        if (_session.UniqueUserId != null)
                        {

                            userconsents.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }

                        await _userconsentRepository.UpdateAsync(userconsents);

                        var appointment = await _doctorAppointmentRepository.GetAsync(input.AppointmentId);
                        if (appointment != null)
                        {
                            appointment.NoOfPages = input.NoOfPages;
                            appointment.UpdatedOn = DateTime.UtcNow;
                            if (_session.UniqueUserId != null)
                            {

                                appointment.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            await _doctorAppointmentRepository.UpdateAsync(appointment);
                        }

                        patientIntakeOutput.Message = "Your detail has been updated successfully.";

                        patientIntakeOutput.StatusCode = 200;

                        stopwatch.Stop();

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(input);
                        createEventsInput.Operation = "Intake form has been updated by user " + user.FullName;
                        createEventsInput.Component = "Patient";
                        createEventsInput.Action = "Update";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                    }

                }
                else
                {
                    patientIntakeOutput.Message = "Signature is required.";
                    patientIntakeOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                patientIntakeOutput.Message = ex.Message;
                patientIntakeOutput.StatusCode = 500;
                Logger.Error("patient Intake" + ex.StackTrace);
            }
            return patientIntakeOutput;
        }

        protected async Task<PatientDetailsoutput> UpdatePatientDetails(CreatePatientInput input)
        {
            PatientDetailsoutput output = new PatientDetailsoutput();

            var userconsents = await _userconsentRepository.GetAsync(input.UserConsentId);
            if (userconsents != null)
            {
                var patientDetails = await _userConsentPatientsDetailsRepository.GetAsync(userconsents.UserConsentPatientsDetailsId);
                if (patientDetails != null)
                {
                    patientDetails.FirstName = input.FirstName;
                    patientDetails.LastName = input.LastName;
                    patientDetails.Address = input.Address;
                    patientDetails.City = input.City;
                    patientDetails.State = input.State;
                    patientDetails.Country = input.Country;
                    patientDetails.PostalCode = input.PostalCode;
                    patientDetails.TelePhone = input.TelePhone;
                    patientDetails.EmailID = input.EmailID;
                    patientDetails.Gender = input.Gender;
                    patientDetails.ReasonForConsult = input.ReasonForConsult;
                    patientDetails.DiseaseDetails = input.DiseaseDetails;
                    patientDetails.ConsultantReportsIds = input.ConsultantReportsIds == null ? "" : string.Join(",", input.ConsultantReportsIds);
                    patientDetails.RelationshipWithPatient = input.RelationshipWithPatient;
                    patientDetails.RepresentativeFirstName = input.RepresentativeFirstName;
                    patientDetails.RepresentativeLastName = input.RepresentativeLastName;
                }
                if (!string.IsNullOrEmpty(input.DateOfBirth))
                {
                    patientDetails.DateOfBirth = input.DateOfBirth;
                }
                if (_session.UniqueUserId != null)
                {

                    patientDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                }

                await _userConsentPatientsDetailsRepository.UpdateAsync(patientDetails);
            }

            output.PatientDetailsId = userconsents.UserConsentPatientsDetailsId;
            if (!String.IsNullOrEmpty(input.EmailID))
            {
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailID);
                if (IsEmailExists != null)
                {
                    output.PatientId = IsEmailExists.UniqueUserId;
                }
            }
            return output;
        }

        protected async Task<Guid> UpdateInviteFamilyDoctor(CreatePatientInput input)
        {
            string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
            Guid newfamilyId = Guid.Empty;
            if (input.IsPatient == true && (input.FamilyDoctorId == Guid.Empty || input.FamilyDoctorId == null))
            {
                var userconsents = await _userconsentRepository.GetAsync(input.UserConsentId);
                if (userconsents.InviteUser != null && userconsents.InviteUserId != Guid.Empty)
                {
                    var inviteuser = await _inviteUserRepository.GetAsync((Guid)userconsents.InviteUserId);
                    if (inviteuser != null)
                    {
                        inviteuser.FirstName = input.DoctorFirstName;
                        inviteuser.LastName = input.DoctorLastName;
                        inviteuser.Address = input.DoctorAddress;
                        inviteuser.City = input.DoctorCity;
                        inviteuser.State = input.DoctorState;
                        inviteuser.Country = input.DoctorCountry;
                        inviteuser.PostalCodes = input.DoctorPostalCodes;
                        inviteuser.TelePhone = input.DoctorTelePhone;
                        inviteuser.EmailAddress = input.DoctorEmailID;
                        inviteuser.CreatedOn = DateTime.UtcNow;
                        inviteuser.UserType = UserType.FamilyDoctor.ToString();
                        //UserConsentPatientsDetailsId = newPatientId,
                        inviteuser.Gender = input.DoctorGender;
                        inviteuser.ReferedBy = input.UserId;
                        if (!string.IsNullOrEmpty(input.DoctorDateOfBirth))
                        {
                            inviteuser.DateOfBirth = input.DoctorDateOfBirth;
                        }
                        if (_session.UniqueUserId != null)
                        {

                            inviteuser.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _inviteUserRepository.UpdateAsync(inviteuser);
                        newfamilyId = inviteuser.Id;
                    }
                    else
                    {
                        var familyDoctor = new InviteUser
                        {
                            FirstName = input.DoctorFirstName,
                            LastName = input.DoctorLastName,
                            Address = input.DoctorAddress,
                            City = input.DoctorCity,
                            State = input.DoctorState,
                            Country = input.DoctorCountry,
                            PostalCodes = input.DoctorPostalCodes,
                            TelePhone = input.DoctorTelePhone,
                            EmailAddress = input.DoctorEmailID,
                            CreatedOn = DateTime.UtcNow,
                            UserType = UserType.FamilyDoctor.ToString(),
                            //UserConsentPatientsDetailsId = newPatientId,
                            Gender = input.DoctorGender,
                            ReferedBy = input.UserId,
                            Status = "Pending",
                            IsActive = true
                        };
                        if (!string.IsNullOrEmpty(input.DoctorDateOfBirth))
                        {
                            familyDoctor.DateOfBirth = input.DoctorDateOfBirth;
                        }
                        if (_session.UniqueUserId != null)
                        {

                            familyDoctor.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        familyDoctor.TenantId = AbpSession.TenantId;
                        newfamilyId = await _inviteUserRepository.InsertAndGetIdAsync(familyDoctor);

                        string body1 = string.Empty;
                        string Name = string.Empty;
                        string message = string.Empty;
                        var user1 = await _userManager.GetUsersInRoleAsync("ADMIN");
                        var ur = user1.FirstOrDefault();
                        //string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                        if (!string.IsNullOrEmpty(input.DoctorTelePhone))
                        {
                            Name = "ETeleHealth Admin";
                            message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                       + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                       + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                       // + "Hospital :-  " + input.Hospital + " <br />"
                                       + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                       + "Phone Number :-  " + input.DoctorTelePhone;
                            body1 = _templateAppService.GetTemplates(Name, message,"");
                            //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //          + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                            //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                            //           + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                            //           // + "Hospital :-  " + input.Hospital + " <br />"
                            //           + "Email Address :-  " + input.DoctorEmailID + " <br />"
                            //           + "Phone Number :-  " + input.DoctorTelePhone + " <br /><br />"
                            //           + "Regards," + " <br />" + "EMRO Team";
                        }
                        else
                        {
                            Name = "ETeleHealth Admin";
                            message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                       + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                       + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                       // + "Hospital :-  " + input.Hospital + " <br />"
                                       + "Email Address :-  " + input.DoctorEmailID;
                            //  + "Phone Number :-  " + input.DoctorTelePhone;
                            body1 = _templateAppService.GetTemplates(Name, message,"");
                            //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //             + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                            //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                            //             + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                            //             //+ "Hospital :-  " + input.Hospital + " <br />"
                            //             + "Email Address :-  " + input.DoctorEmailID + " <br /><br />"
                            //             //+ "PhoneNumber:-  " + input.PhoneNumber + " <br />"
                            //             + "Regards," + " <br />" + "EMRO Team";
                        }

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for FamilyDoctor", body1, adminmail));

                        string doctorName = input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase();
                        string doctormessage = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
             "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
             "If you have any questions regarding the invitation from " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                        string doctorbody = _templateAppService.GetTemplates(doctorName, doctormessage, "Welcome aboard,<br />ETeleHealth Team");

                        //string doctorbody = "Hello " + "<b>" + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + ",</b> <br /><br /> "
                        //                  + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //                 "also you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                        //                + "Regards," + " <br />" + "EMRO Team";

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.DoctorEmailID, "Invite to join ETeleHealth", doctorbody, adminmail));
                    }
                }
                else
                {
                    var familyDoctor = new InviteUser
                    {
                        FirstName = input.DoctorFirstName,
                        LastName = input.DoctorLastName,
                        Address = input.DoctorAddress,
                        City = input.DoctorCity,
                        State = input.DoctorState,
                        Country = input.DoctorCountry,
                        PostalCodes = input.DoctorPostalCodes,
                        TelePhone = input.DoctorTelePhone,
                        EmailAddress = input.DoctorEmailID,
                        CreatedOn = DateTime.UtcNow,
                        UserType = UserType.FamilyDoctor.ToString(),
                        //UserConsentPatientsDetailsId = newPatientId,
                        Gender = input.DoctorGender,
                        ReferedBy = input.UserId,
                        Status = "Pending"
                    };
                    if (!string.IsNullOrEmpty(input.DoctorDateOfBirth))
                    {
                        familyDoctor.DateOfBirth = input.DoctorDateOfBirth;
                    }
                    if (_session.UniqueUserId != null)
                    {

                        familyDoctor.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    familyDoctor.TenantId = AbpSession.TenantId;
                    newfamilyId = await _inviteUserRepository.InsertAndGetIdAsync(familyDoctor);

                    string body1 = string.Empty;
                    string Name = string.Empty;
                    string message = string.Empty;
                    var user1 = await _userManager.GetUsersInRoleAsync("ADMIN");
                    var ur = user1.FirstOrDefault();
                    //string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    if (!string.IsNullOrEmpty(input.DoctorTelePhone))
                    {
                        Name = "ETeleHealth Admin";
                        message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                   + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                   + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                   // + "Hospital :-  " + input.Hospital + " <br />"
                                   + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                   + "Phone Number :-  " + input.DoctorTelePhone;
                        body1 = _templateAppService.GetTemplates(Name, message,"");
                        //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                        //          + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                        //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                        //           + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                        //           // + "Hospital :-  " + input.Hospital + " <br />"
                        //           + "Email Address :-  " + input.DoctorEmailID + " <br />"
                        //           + "Phone Number :-  " + input.DoctorTelePhone + " <br /><br />"
                        //           + "Regards," + " <br />" + "EMRO Team";
                    }
                    else
                    {
                        Name = "ETeleHealth Admin";
                        message = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                   + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                   + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                   // + "Hospital :-  " + input.Hospital + " <br />"
                                   + "Email Address :-  " + input.DoctorEmailID;
                        //  + "Phone Number :-  " + input.DoctorTelePhone;
                        body1 = _templateAppService.GetTemplates(Name, message,"");
                        //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                        //             + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                        //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                        //             + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                        //             //+ "Hospital :-  " + input.Hospital + " <br />"
                        //             + "Email Address :-  " + input.DoctorEmailID + " <br /><br />"
                        //             //+ "PhoneNumber:-  " + input.PhoneNumber + " <br />"
                        //             + "Regards," + " <br />" + "EMRO Team";
                    }

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for FamilyDoctor", body1, adminmail));

                    string doctorName = input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase();
                    string doctormessage = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
                                 "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
                                 "If you have any questions regarding the invitation from " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                    string doctorbody = _templateAppService.GetTemplates(doctorName, doctormessage,"Welcome aboard,<br />ETeleHealth Team");

                    //string doctorbody = "Hello " + "<b>" + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + ",</b> <br /><br /> "
                    //                  + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                    //                 "also you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                    //                + "Regards," + " <br />" + "EMRO Team";

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.DoctorEmailID, "Invite to join ETeleHealth", doctorbody, adminmail));

                }


            }

            return newfamilyId;
        }

    }
}
