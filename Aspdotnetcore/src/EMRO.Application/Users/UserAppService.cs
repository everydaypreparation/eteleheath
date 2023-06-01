using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Json;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.AuditReport;
using EMRO.Authorization;
using EMRO.Authorization.Accounts;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common.AzureBlobStorage;
using EMRO.Common.AzureBlobStorage.Dto;
using EMRO.Common.Paubox;
using EMRO.Common.Templates;
using EMRO.DiagnosticsCases;
using EMRO.DiagnosticsRequestTest;
using EMRO.Email;
using EMRO.InviteUsers;
using EMRO.OncologyConsultReports;
using EMRO.Roles.Dto;
using EMRO.Sessions;
using EMRO.UserConsents;
using EMRO.Users.Dto;
using EMRO.UsersMetaInfo;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EMRO.Users
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]

    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly ConsultReportAppService _consultReportAppService;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        private readonly AppointmentAppService _appointmentAppService;
        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private IUserRepository _userRepository;
        private readonly IRepository<DoctorAppointment, Guid> _doctorAppointmentRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IMailer _mailer;
        private readonly EmroAppSession _session;
        private readonly IRepository<UserConsent, Guid> _userConsentRepository;
        private readonly IRepository<InviteUser, Guid> _inviteUserRepository;
        private readonly IRepository<OncologyConsultReport, Guid> _consultReportRepository;
        private readonly IRepository<DiagnosticsCase, Guid> _diagnosticsCaseRepository;
        private readonly IRepository<UserRole, long> _userRolesRepository;
        private readonly IBlobContainer _blobContainer;
        private readonly TemplateAppService _templateAppService;
        private IClientInfoProvider _clientInfoProvider;
        private readonly IPaubox _paubox;
        private readonly IRepository<InviteUser, Guid> _inviteRepository;
        private readonly IRepository<RequestTest, Guid> _requestTestRepository;
        private readonly IAuditReportAppService _auditReportAppService;
        private readonly IRepository<AuditEvent, Guid> _auditEventRepository;
        public UserAppService(
            IRepository<InviteUser, Guid> inviteRepository,
            IRepository<RequestTest, Guid> requestTestRepository,
            ConsultReportAppService consultReportAppService,
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IRepository<UserMetaDetails, Guid> userMetaDetailsRepository,
            IMailer mailer,
            AppointmentAppService appointmentAppService,
            IOptions<UplodedFilePath> uplodedFilePath,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IUserRepository userRepository
            , EmroAppSession session
            , IRepository<DoctorAppointment, Guid> doctorAppointmentRepository,
            IRepository<UserConsent, Guid> userConsentRepository
            , IRepository<InviteUser, Guid> inviteUserRepository
            , IRepository<OncologyConsultReport, Guid> consultReportRepository
            , IRepository<DiagnosticsCase, Guid> diagnosticsCaseRepository
            , IRepository<UserRole, long> userRolesRepository
            , IBlobContainer blobContainer
            , TemplateAppService templateAppService
            , IClientInfoProvider clientInfoProvider
            , IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository
            , IPaubox paubox
            , IAuditReportAppService auditReportAppService
            , IRepository<AuditEvent, Guid> auditEventRepository
            )
            : base(repository)
        {
            _consultReportAppService = consultReportAppService;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _userMetaDetailsRepository = userMetaDetailsRepository;
            _mailer = mailer;
            _appointmentAppService = appointmentAppService;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            _configuration = configuration;
            _userRepository = userRepository;
            _session = session;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _userConsentRepository = userConsentRepository;
            _inviteUserRepository = inviteUserRepository;
            _consultReportRepository = consultReportRepository;
            _diagnosticsCaseRepository = diagnosticsCaseRepository;
            _userRolesRepository = userRolesRepository;
            _blobContainer = blobContainer;
            _templateAppService = templateAppService;
            _clientInfoProvider = clientInfoProvider;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _paubox = paubox;
            _inviteRepository = inviteRepository;
            _requestTestRepository = requestTestRepository;
            _auditReportAppService = auditReportAppService;
            _auditEventRepository = auditEventRepository;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreateConsultant([FromForm] CreateUserDto input)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //var stopwatch = Stopwatch.StartNew();

            CheckCreatePermission();
            UserDto userDto = new UserDto();
            UserMetaDetails userMetadetails = new UserMetaDetails();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();
                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()


                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }

                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();


                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();

                    userMetadetails = new UserMetaDetails
                    {
                        HospitalAffiliation = input.HospitalAffiliation,
                        CurrentAffiliation = input.CurrentAffiliation,
                        ProfessionalBio = input.ProfessionalBio,
                        UndergraduateMedicalTraining = input.UndergraduateMedicalTraining,
                        OncologySpecialty = input.OncologySpecialty,
                        MedicalAssociationMembership = input.MedicalAssociationMembership,
                        LicensingNumber = input.LicensingNumber,
                        UserId = user.UniqueUserId,
                        DateConfirmed = input.DateConfirmed,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId,

                        Residency1 = input.Residency1,
                        Residency2 = input.Residency2,
                        Certificate = input.Certificate,
                        Fellowship = input.Fellowship,
                        ExperienceOrTraining = input.ExperienceOrTraining,
                        Credentials = input.Credentials,
                        AdminNotes = input.AdminNotes

                    };
                    if (input.OncologySubSpecialty != null)
                    {
                        userMetadetails.OncologySubSpecialty = string.Join(",", input.OncologySubSpecialty);
                    }
                    if (input.ConsultationType != null)
                    {
                        userMetadetails.ConsultationType = string.Join(",", input.ConsultationType);
                    }
                    if (_session.UniqueUserId != null)
                    {
                        userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;
                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " consultant has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Our mission is to provide an integrative and innovative virtual consulting telehealth platform for cancer care professionals."
                                     + " <br /><br /> " + "We provide the structure and tools that allows safe and timely connection between patients and health care professionals to discuss cancer diagnosis and available treatment options. We are invested in creating a safe environment for deposition and analysis of patient data, exchange of information and diagnostic results between cancer care providers and patients that is HIPAA and PHIPA compliant, follows international compliance frameworks and highest levels of cybersecurity guidelines."
                                     + " <br /><br /> " + "The platform can be accessed through <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with the credentials shown below:"
                                     + " <br />" + "UserName: " + userName
                                     + " <br /> " + "Password: " + newPassword
                                     + " <br /><br /> " + "As a first step, please login to the ETeleHealth Portal to:"
                                     + " <br /><ul style='line-height:160%'><li> " + "Update your profile, including profile picture, address, contact info, credentials, affiliations and speciality, as well as a short Bio. "
                                     + " </li><li> Add your availability for appointments/consults. The system will remind you to provide appointment slots if you haven&#39;t provided time slots or if you have 4 or less availabilities showing in the system."
                                     + " </li><li> Update your time zone, as current time zone is automatically set to Toronto/Canada EST"
                                     + " </li></ul> ";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";
                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdateConsultant([FromForm] UpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();

            // string profileUrl = string.Empty;
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();

                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;

                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    //if (!string.IsNullOrEmpty(profileUrl))
                    //{
                    //    user.UploadProfilePicture = profileUrl.Replace(_env.ContentRootPath, "");
                    //}

                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;

                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();


                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (userMetaDetails == null)
                    {
                        userMetadetails1 = new UserMetaDetails
                        {
                            HospitalAffiliation = input.HospitalAffiliation,
                            CurrentAffiliation = input.CurrentAffiliation,
                            ProfessionalBio = input.ProfessionalBio,
                            UndergraduateMedicalTraining = input.UndergraduateMedicalTraining,

                            //OncologySubSpecialty = input.OncologySubSpecialty,
                            MedicalAssociationMembership = input.MedicalAssociationMembership,
                            LicensingNumber = input.LicensingNumber,
                            DateConfirmed = input.DateConfirmed,
                            UserId = input.UserId,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            IsActive = true,
                            //ConsultationType = input.ConsultationType,
                            Residency1 = input.Residency1,
                            Residency2 = input.Residency2,
                            Certificate = input.Certificate,
                            Fellowship = input.Fellowship,
                            ExperienceOrTraining = input.ExperienceOrTraining,
                            OncologySpecialty = input.OncologySpecialty,
                            Credentials = input.Credentials,
                            AdminNotes = input.AdminNotes

                        };
                        if (input.OncologySubSpecialty != null)
                        {
                            userMetadetails1.OncologySubSpecialty = string.Join(",", input.OncologySubSpecialty);
                        }
                        if (input.ConsultationType != null)
                        {
                            userMetadetails1.ConsultationType = string.Join(",", input.ConsultationType);
                        }
                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails1);

                    }
                    else
                    {
                        userMetaDetails.HospitalAffiliation = input.HospitalAffiliation;
                        userMetaDetails.CurrentAffiliation = input.CurrentAffiliation;
                        userMetaDetails.ProfessionalBio = input.ProfessionalBio;
                        userMetaDetails.UndergraduateMedicalTraining = input.UndergraduateMedicalTraining;
                        userMetaDetails.OncologySpecialty = input.OncologySpecialty;
                        if (input.OncologySubSpecialty != null)
                        {
                            userMetaDetails.OncologySubSpecialty = string.Join(",", input.OncologySubSpecialty);
                        }
                        userMetaDetails.MedicalAssociationMembership = input.MedicalAssociationMembership;
                        userMetaDetails.LicensingNumber = input.LicensingNumber;
                        userMetaDetails.DateConfirmed = input.DateConfirmed;
                        userMetaDetails.UserId = input.UserId;
                        userMetaDetails.Id = userMetaDetails.Id;
                        userMetaDetails.UpdatedOn = DateTime.UtcNow;
                        if (input.ConsultationType != null)
                        {
                            userMetaDetails.ConsultationType = string.Join(",", input.ConsultationType);
                        }
                        userMetaDetails.Residency1 = input.Residency1;
                        userMetaDetails.Residency2 = input.Residency2;
                        userMetaDetails.Certificate = input.Certificate;
                        userMetaDetails.Fellowship = input.Fellowship;
                        userMetaDetails.ExperienceOrTraining = input.ExperienceOrTraining;
                        userMetaDetails.Credentials = input.Credentials;
                        userMetaDetails.AdminNotes = input.AdminNotes;
                        if (_session.UniqueUserId != null)
                        {
                            userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                    }

                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " consultant has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    //createEventsInput.BeforeValue = "";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreatePatient([FromForm] PatientCreateUserDto input)
        {
            CheckCreatePermission();
            UserDto userDto = new UserDto();
            // string profileUrl = string.Empty;
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            string Name = string.Empty;
            string message = string.Empty;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                UserMetaDetails userMetadetails = new UserMetaDetails();

                string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();

                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()

                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }

                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();
                    if (!string.IsNullOrWhiteSpace(input.DoctorFirstName) && !string.IsNullOrWhiteSpace(input.DoctorLastName) && !string.IsNullOrWhiteSpace(input.DoctorEmailID))
                    {

                        if (input.FamilyDoctorId != Guid.Empty)
                        {
                            userMetadetails = new UserMetaDetails
                            {
                                UserId = user.UniqueUserId,
                                FamilyDoctorId = input.FamilyDoctorId,
                                ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                AdminNotes = input.AdminNotes,
                                IsActive = true,
                                CreatedOn = DateTime.UtcNow,
                                TenantId = AbpSession.TenantId,

                            };
                            if (_session.UniqueUserId != null)
                            {
                                userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            await _userMetaDetailsRepository.InsertAsync(userMetadetails);

                        }
                        else
                        {
                            Guid newInviteId = Guid.Empty;
                            var invite = await _inviteUserRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.DoctorEmailID && x.UserType == "FamilyDoctor");
                            if (invite != null)
                            {
                                newInviteId = invite.Id;
                            }
                            else
                            {
                                var inviteUser = new InviteUser
                                {
                                    ReferedBy = user.UniqueUserId,
                                    FirstName = input.DoctorFirstName,
                                    LastName = input.DoctorLastName,
                                    Address = input.DoctorAddress,
                                    City = input.DoctorCity,
                                    State = input.DoctorState,
                                    Country = input.DoctorCountry,
                                    PostalCodes = input.DoctorPostalCodes,
                                    TelePhone = input.DoctorTelePhone,
                                    EmailAddress = input.DoctorEmailID,
                                    //ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                    IsActive = true,
                                    CreatedOn = DateTime.UtcNow,
                                    TenantId = AbpSession.TenantId,
                                    UserType = UserType.FamilyDoctor.ToString(),
                                    Status = "Pending"
                                };
                                if (_session.UniqueUserId != null)
                                {
                                    inviteUser.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                newInviteId = await _inviteUserRepository.InsertAndGetIdAsync(inviteUser);
                            }


                            userMetadetails = new UserMetaDetails
                            {
                                UserId = user.UniqueUserId,
                                ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                AdminNotes = input.AdminNotes,
                                InviteUserId = newInviteId,
                                IsActive = true,
                                CreatedOn = DateTime.UtcNow,
                                TenantId = AbpSession.TenantId,

                            };
                            if (_session.UniqueUserId != null)
                            {
                                userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                            string body1 = string.Empty;

                            var user1 = await _userManager.GetUsersInRoleAsync("ADMIN");
                            var ur = user1.FirstOrDefault();

                            if (!string.IsNullOrEmpty(input.PhoneNumber))
                            {

                                Name = "ETeleHealth Admin";
                                message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                          + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                          + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                          + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                          + "Phone Number :-  " + input.PhoneNumber;
                                body1 = _templateAppService.GetTemplates(Name, message, "");

                                //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                                //          + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                                //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                                //           + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                //           + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                //           + "Phone Number :-  " + input.PhoneNumber + " <br /><br />"
                                //           + "Regards," + " <br />" + "EMRO Team";
                            }
                            else
                            {
                                Name = "ETeleHealth Admin";
                                message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                            + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                            + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                            + "Email Address :-  " + input.DoctorEmailID;
                                body1 = _templateAppService.GetTemplates(Name, message, "");
                                //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                                //             + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                                //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                                //             + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                //             + "Email Address :-  " + input.DoctorEmailID + " <br /><br />"
                                //             + "Regards," + " <br />" + "EMRO Team";
                            }

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for FamilyDoctor", body1, adminmail));

                            //string doctorbody = "Hello " + "<b>" + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + ",</b> <br /><br /> "
                            //                  + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                            //                 "also you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                            //                + "Regards," + " <br />" + "EMRO Team";

                            Name = input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase();
                            message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
             "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
             "If you have any questions regarding the invitation from " + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                            string doctorbody = _templateAppService.GetTemplates(Name, message, "Welcome aboard,<br />ETeleHealth Team");
                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.DoctorEmailID, "Invite to join ETeleHealth", doctorbody, adminmail));
                        }
                    }
                    else
                    {
                        userMetadetails = new UserMetaDetails
                        {
                            UserId = user.UniqueUserId,
                            ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                            AdminNotes = input.AdminNotes,
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId
                        };
                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails);


                    }
                    //string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.FullName + " patient has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Your health is essential."
                                + " <br /><br /> " + "Whether you have a new cancer diagnosis or you are looking for secondary opinion on your current diagnosis, our platform will safely connect you with cancer specialists to discuss your cancer diagnosis and available treatment options towards achieving a complete picture of your disease. Our mission is to provide accessible, fast, and high-quality cancer care anywhere and anytime."
                                + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials -"
                                + " <br /><br /> " + "UserName: " + userName
                                    + " <br /> " + "Password: " + newPassword
                                + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";
                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                }
                // }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdatePatient([FromForm] PatientUpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();
            //string profileUrl = string.Empty;
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            string Name = string.Empty;
            string message = string.Empty;

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();

                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;


                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }
                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(input.DoctorFirstName) && !string.IsNullOrWhiteSpace(input.DoctorLastName) && !string.IsNullOrWhiteSpace(input.DoctorEmailID))
                    {
                        if (userMetaDetails == null)
                        {
                            if (input.FamilyDoctorId != Guid.Empty)
                            {
                                userMetadetails1 = new UserMetaDetails
                                {
                                    UserId = input.UserId,
                                    FamilyDoctorId = input.FamilyDoctorId,
                                    ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                    IsActive = true,
                                    CreatedOn = DateTime.UtcNow,
                                    TenantId = AbpSession.TenantId,
                                    AdminNotes = input.AdminNotes
                                };
                                if (_session.UniqueUserId != null)
                                {
                                    userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _userMetaDetailsRepository.InsertAsync(userMetadetails1);
                            }
                            else
                            {
                                Guid newInviteId = Guid.Empty;
                                var invite = await _inviteUserRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.DoctorEmailID && x.UserType == "FamilyDoctor");
                                if (invite != null)
                                {
                                    newInviteId = invite.Id;
                                }
                                else
                                {
                                    var inviteUser = new InviteUser
                                    {
                                        ReferedBy = user.UniqueUserId,
                                        FirstName = input.DoctorFirstName,
                                        LastName = input.DoctorLastName,
                                        Address = input.DoctorAddress,
                                        City = input.DoctorCity,
                                        State = input.DoctorState,
                                        Country = input.DoctorCountry,
                                        PostalCodes = input.DoctorPostalCodes,
                                        TelePhone = input.DoctorTelePhone,
                                        EmailAddress = input.DoctorEmailID,
                                        //ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                        IsActive = true,
                                        CreatedOn = DateTime.UtcNow,
                                        TenantId = AbpSession.TenantId,
                                        UserType = UserType.FamilyDoctor.ToString(),
                                        Status = "Pending"
                                    };
                                    if (_session.UniqueUserId != null)
                                    {
                                        inviteUser.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                    }
                                    newInviteId = await _inviteUserRepository.InsertAndGetIdAsync(inviteUser);
                                }

                                userMetadetails1 = new UserMetaDetails
                                {
                                    UserId = user.UniqueUserId,
                                    ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                    InviteUserId = newInviteId,
                                    IsActive = true,
                                    CreatedOn = DateTime.UtcNow,
                                    TenantId = AbpSession.TenantId,
                                    AdminNotes = input.AdminNotes,

                                };
                                if (_session.UniqueUserId != null)
                                {
                                    userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _userMetaDetailsRepository.InsertAsync(userMetadetails1);
                                string body1 = string.Empty;
                                var user1 = await _userManager.GetUsersInRoleAsync("ADMIN");
                                var ur = user1.FirstOrDefault();
                                string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                                if (!string.IsNullOrEmpty(input.PhoneNumber))
                                {

                                    Name = "ETeleHealth Admin";
                                    message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                              + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                              + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                              + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                              + "Phone Number :-  " + input.PhoneNumber;
                                    body1 = _templateAppService.GetTemplates(Name, message, "");

                                    //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                                    //          + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                                    //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                                    //           + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                    //           + "Email Address :-  " + input.DoctorEmailID + " <br />"
                                    //           + "Phone Number :-  " + input.PhoneNumber + " <br /><br />"
                                    //           + "Regards," + " <br />" + "EMRO Team";
                                }
                                else
                                {
                                    Name = "ETeleHealth Admin";
                                    message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                                + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                                + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                                + "Email Address :-  " + input.DoctorEmailID;
                                    body1 = _templateAppService.GetTemplates(Name, message, "");
                                    //body1 = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                                    //             + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + " might be willing to connect with a Consultant who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                                    //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                                    //             + "Name :-  " + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + " <br />"
                                    //             + "Email Address :-  " + input.DoctorEmailID + " <br /><br />"
                                    //             + "Regards," + " <br />" + "EMRO Team";
                                }

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for FamilyDoctor", body1, adminmail));

                                //string doctorbody = "Hello " + "<b>" + input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase() + ",</b> <br /><br /> "
                                //                  + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                                //                 "also you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                                //                + "Regards," + " <br />" + "EMRO Team";

                                Name = input.DoctorFirstName.ToPascalCase() + " " + input.DoctorLastName.ToPascalCase();
                                message = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
             "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
             "If you have any questions regarding the invitation from " + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                                string doctorbody = _templateAppService.GetTemplates(Name, message, "Welcome aboard,<br />ETeleHealth Team");
                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.DoctorEmailID, "Invite to join ETeleHealth", doctorbody, adminmail));
                            }

                        }
                        else
                        {
                            if (input.FamilyDoctorId != Guid.Empty)
                            {
                                userMetaDetails.FamilyDoctorId = input.FamilyDoctorId;
                                userMetaDetails.ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider;
                                userMetaDetails.AdminNotes = input.AdminNotes;
                                if (_session.UniqueUserId != null)
                                {
                                    userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                            }
                            else
                            {
                                if (userMetaDetails.InviteUserId != null && userMetaDetails.InviteUserId != Guid.Empty)
                                {
                                    var invite = await _inviteUserRepository.GetAsync((Guid)userMetaDetails.InviteUserId);
                                    if (invite != null)
                                    {
                                        invite.ReferedBy = user.UniqueUserId;
                                        invite.FirstName = input.DoctorFirstName;
                                        invite.LastName = input.DoctorLastName;
                                        invite.Address = input.DoctorAddress;
                                        invite.City = input.DoctorCity;
                                        invite.State = input.DoctorState;
                                        invite.Country = input.DoctorCountry;
                                        invite.PostalCodes = input.DoctorPostalCodes;
                                        invite.TelePhone = input.DoctorTelePhone;
                                        invite.EmailAddress = input.DoctorEmailID;
                                        if (_session.UniqueUserId != null)
                                        {
                                            invite.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                        }
                                        await _inviteUserRepository.UpdateAsync(invite);
                                    }
                                }
                                else
                                {
                                    var inviteUser = new InviteUser
                                    {
                                        ReferedBy = user.UniqueUserId,
                                        FirstName = input.DoctorFirstName,
                                        LastName = input.DoctorLastName,
                                        Address = input.DoctorAddress,
                                        City = input.DoctorCity,
                                        State = input.DoctorState,
                                        Country = input.DoctorCountry,
                                        PostalCodes = input.DoctorPostalCodes,
                                        TelePhone = input.DoctorTelePhone,
                                        EmailAddress = input.DoctorEmailID,
                                        // ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider,
                                        IsActive = true,
                                        CreatedOn = DateTime.UtcNow,
                                        TenantId = AbpSession.TenantId,
                                        UserType = UserType.FamilyDoctor.ToString(),
                                        Status = "Pending"
                                    };
                                    if (_session.UniqueUserId != null)
                                    {
                                        inviteUser.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                    }
                                    Guid newInviteId = await _inviteUserRepository.InsertAndGetIdAsync(inviteUser);

                                    userMetaDetails.InviteUserId = newInviteId;
                                    userMetaDetails.ConsentMedicalInformationWithCancerCareProvider = input.ConsentMedicalInformationWithCancerCareProvider;
                                    userMetaDetails.AdminNotes = input.AdminNotes;
                                    if (_session.UniqueUserId != null)
                                    {
                                        userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                    }
                                    await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                }


                            }
                        }

                    }
                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " patient has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreateFamilyDoctor([FromForm] FamilyCreateUserDto input)
        {
            CheckCreatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();
                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()

                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }


                    }
                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();

                    var userMetadetails = new UserMetaDetails
                    {
                        HospitalAffiliation = input.HospitalAffiliation,
                        CurrentAffiliation = input.CurrentAffiliation,
                        ProfessionalBio = input.ProfessionalBio,
                        UndergraduateMedicalTraining = input.UndergraduateMedicalTraining,
                        MedicalAssociationMembership = input.MedicalAssociationMembership,
                        LicensingNumber = input.LicensingNumber,
                        DateConfirmed = input.DateConfirmed,
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId,
                        IsActive = true,
                        Residency1 = input.Residency1,
                        Residency2 = input.Residency2,
                        Certificate = input.Certificate,
                        Fellowship = input.Fellowship,
                        ExperienceOrTraining = input.ExperienceOrTraining,
                        OncologySpecialty = input.OncologySpecialty,
                        Credentials = input.Credentials,
                        UserId = user.UniqueUserId,
                        AdminNotes = input.AdminNotes

                    };

                    if (_session.UniqueUserId != null)
                    {
                        userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                    await AssociateFamilydoctor(input.EmailAddress, user.UniqueUserId);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " family doctor has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Our mission is to provide an integrative and innovative virtual consulting telehealth platform for cancer care professionals."
                                     + " <br /><br /> " + "We provide the structure and tools that allows safe and timely connection between patients and health care professionals to discuss cancer diagnosis and available treatment options. We are invested in creating a safe environment for deposition and analysis of patient data, exchange of information and diagnostic results between cancer care providers and patients that is HIPAA and Ontario&#39;s PHIPA compliant, follows international compliance frameworks and highest levels of cybersecurity guidelines."
                                     + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials - "
                                     + " <br /><br /> " + "UserName: " + userName
                                     + " <br /> " + "Password: " + newPassword
                                     + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";
                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                }
                // }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdateFamilyDoctor([FromForm] FamilyUpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();

                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;

                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }


                    }
                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }
                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (userMetaDetails == null)
                    {
                        userMetadetails1 = new UserMetaDetails
                        {
                            HospitalAffiliation = input.HospitalAffiliation,
                            CurrentAffiliation = input.CurrentAffiliation,
                            ProfessionalBio = input.ProfessionalBio,
                            UndergraduateMedicalTraining = input.UndergraduateMedicalTraining,
                            MedicalAssociationMembership = input.MedicalAssociationMembership,
                            LicensingNumber = input.LicensingNumber,
                            DateConfirmed = input.DateConfirmed,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            IsActive = true,
                            Residency1 = input.Residency1,
                            Residency2 = input.Residency2,
                            Certificate = input.Certificate,
                            Fellowship = input.Fellowship,
                            ExperienceOrTraining = input.ExperienceOrTraining,
                            OncologySpecialty = input.OncologySpecialty,
                            Credentials = input.Credentials,
                            AdminNotes = input.AdminNotes

                        };

                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails1);

                    }
                    else
                    {
                        userMetaDetails.HospitalAffiliation = input.HospitalAffiliation;
                        userMetaDetails.CurrentAffiliation = input.CurrentAffiliation;
                        userMetaDetails.ProfessionalBio = input.ProfessionalBio;
                        userMetaDetails.UndergraduateMedicalTraining = input.UndergraduateMedicalTraining;
                        userMetaDetails.OncologySpecialty = input.OncologySpecialty;
                        userMetaDetails.MedicalAssociationMembership = input.MedicalAssociationMembership;
                        userMetaDetails.LicensingNumber = input.LicensingNumber;
                        userMetaDetails.DateConfirmed = input.DateConfirmed;
                        userMetaDetails.UserId = input.UserId;
                        userMetaDetails.Id = userMetaDetails.Id;
                        userMetaDetails.UpdatedOn = DateTime.UtcNow;
                        userMetaDetails.Residency1 = input.Residency1;
                        userMetaDetails.Residency2 = input.Residency2;
                        userMetaDetails.Certificate = input.Certificate;
                        userMetaDetails.Fellowship = input.Fellowship;
                        userMetaDetails.ExperienceOrTraining = input.ExperienceOrTraining;
                        userMetaDetails.Credentials = input.Credentials;
                        userMetaDetails.AdminNotes = input.AdminNotes;
                        if (_session.UniqueUserId != null)
                        {
                            userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                    }

                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " family doctor has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreateDiagnostic([FromForm] DiagnosticCreateUserDto input)
        {
            CheckCreatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();
                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()

                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }

                    }

                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();
                    var userMetadetails = new UserMetaDetails
                    {
                        UserId = user.UniqueUserId,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId,
                        AdminNotes = input.AdminNotes

                    };
                    if (_session.UniqueUserId != null)
                    {
                        userMetadetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " Diagnostic has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Our mission is to provide an integrative and innovative digital healthcare platform for patients and cancer care providers that allows seamless integration with diagnostics."
                                    + " <br /><br /> " + "We provide the structure and tools that allows safe and timely flow of diagnostic requests between patients and health care professionals and diagnostic centers or physician responsible for the diagnostic procedures. We are invested in creating a safe environment for exchange of diagnostic requests and results between cancer care providers and diagnostic service providers that is HIPAA and Ontario&#39;s PHIPA compliant, follows international compliance frameworks and highest levels of cybersecurity guidelines."
                                    + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials - "
                                    + " <br /><br /> " + "UserName: " + userName
                                    + " <br /> " + "Password: " + newPassword
                                    + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";

                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";

                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                }
                // }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdateDiagnostic([FromForm] DiagnosticUpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();

                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;

                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;

                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();


                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (userMetaDetails == null)
                    {
                        userMetadetails1 = new UserMetaDetails
                        {
                            UserId = input.UserId,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            IsActive = true,
                            AdminNotes = input.AdminNotes
                        };
                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails1);

                    }
                    else
                    {
                        userMetaDetails.UserId = input.UserId;
                        userMetaDetails.Id = userMetaDetails.Id;
                        userMetaDetails.UpdatedOn = DateTime.UtcNow;
                        userMetaDetails.AdminNotes = input.AdminNotes;
                        if (_session.UniqueUserId != null)
                        {
                            userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                    }


                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " Diagnostic has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreateMedicalLegal([FromForm] InsuranceCreateUserDto input)
        {
            CheckCreatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();
                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()

                    };

                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    //if (!string.IsNullOrEmpty(profileUrl))
                    //{
                    //    user.UploadProfilePicture = profileUrl.Replace(_env.ContentRootPath, "");
                    //}
                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();


                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();

                    var userMetadetails = new UserMetaDetails
                    {
                        Company = input.Company,
                        RequestedOncologySubspecialty = input.RequestedOncologySubspecialty,
                        UserId = user.UniqueUserId,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId,
                        AdminNotes = input.AdminNotes,
                        AmountDeposit = input.AmountDeposit

                    };
                    if (_session.UniqueUserId != null)
                    {
                        userMetadetails.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " Medical Legal has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Our mission is to provide an integrative and innovative digital healthcare platform for cancer care professionals and medical legal teams."
                                    + " <br /><br /> " + "We provide the structure and tools that allows safe and timely connection between your team and cancer care professionals to discuss and provide expert opinion on your legal question. We are invested in creating a safe environment for storage of patient data and exchange of information between cancer care providers and your team that is HIPAA and Ontario&#39;s PHIPA compliant, follows international compliance frameworks and highest levels of cybersecurity guidelines."
                                    + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials - "
                                    + " <br /><br /> " + "UserName: " + userName
                                    + " <br /> " + "Password: " + newPassword
                                    + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";

                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                }
                // }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdateMedicalLegal([FromForm] InsuranceUpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;

                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;

                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }
                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (userMetaDetails == null)
                    {
                        userMetadetails1 = new UserMetaDetails
                        {
                            Company = input.Company,
                            RequestedOncologySubspecialty = input.RequestedOncologySubspecialty,
                            UserId = input.UserId,
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            AdminNotes = input.AdminNotes,
                            AmountDeposit = input.AmountDeposit

                        };
                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails1);

                    }
                    else
                    {
                        userMetaDetails.Company = input.Company == null ? userMetaDetails.HospitalAffiliation : input.Company;
                        userMetaDetails.RequestedOncologySubspecialty = input.RequestedOncologySubspecialty == null ? userMetaDetails.RequestedOncologySubspecialty : input.RequestedOncologySubspecialty;
                        userMetaDetails.UserId = input.UserId;
                        userMetaDetails.Id = userMetaDetails.Id;
                        userMetaDetails.UpdatedOn = DateTime.UtcNow;
                        userMetaDetails.AdminNotes = input.AdminNotes;
                        userMetaDetails.AmountDeposit = input.AmountDeposit;
                        if (_session.UniqueUserId != null)
                        {
                            userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                    }

                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " Medical Legal has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> CreateInsurance([FromForm] InsuranceCreateUserDto input)
        {
            CheckCreatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string UserRolename = input.RoleNames[0].ToString().ToLower();
                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, 0);
                    if (IsPhoneExists)
                    {
                        userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                        userDto.StatusCode = 401;
                        return userDto;
                    }
                }
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    userDto.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    userDto.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            userDto.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }
                    string userName = GenrateUserName(input.Name, input.Surname);
                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, 0, userName);
                    }
                    var user = new User
                    {
                        EmailAddress = input.EmailAddress,
                        UserName = userName,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = newPassword,
                        Name = input.Name,
                        Surname = input.Surname,
                        Timezone = input.Timezone,
                        Address = input.Address,
                        DateOfBirth = input.DateOfBirth,
                        Gender = input.Gender,
                        Title = input.Title,
                        City = input.City,
                        State = input.State,
                        Country = input.Country,
                        PostalCode = input.PostalCode,
                        PhoneNumber = input.PhoneNumber,
                        UniqueUserId = Guid.NewGuid()

                    };
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    user.TenantId = AbpSession.TenantId;
                    user.IsEmailConfirmed = true;

                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();

                    var userMetadetails = new UserMetaDetails
                    {
                        Company = input.Company,
                        RequestedOncologySubspecialty = input.RequestedOncologySubspecialty,
                        UserId = user.UniqueUserId,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        TenantId = AbpSession.TenantId,
                        AdminNotes = input.AdminNotes,
                        AmountDeposit = input.AmountDeposit

                    };
                    if (_session.UniqueUserId != null)
                    {
                        userMetadetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _userMetaDetailsRepository.InsertAsync(userMetadetails);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    userDto = MapToEntityDto(user);
                    userDto.password = newPassword;
                    userDto.Message = "User created successfully.";
                    userDto.StatusCode = 200;
                    stopwatch.Stop();

                    //Log Audit Events
                    string userAuditDetails = string.Empty;
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetadetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        userAuditDetails = UserJsonObj.ToString();

                    }
                    else
                    {
                        userAuditDetails = Utility.Serialize(user);
                    }

                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = userAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " Insurance has been created.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Our mission is to provide an integrative and innovative digital healthcare platform for cancer care professionals and medical legal teams."
                                    + " <br /><br /> " + "We provide the structure and tools that allows safe and timely connection between your team and cancer care professionals to discuss and provide expert opinion on your legal question. We are invested in creating a safe environment for storage of patient data and exchange of information between cancer care providers and your team that is HIPAA and Ontario&#39;s PHIPA compliant, follows international compliance frameworks and highest levels of cybersecurity guidelines."
                                    + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials - "
                                    + " <br /><br /> " + "UserName: " + userName
                                    + " <br /> " + "Password: " + newPassword
                                    + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";

                    string body = _templateAppService.GetTemplates(Name, message, signature);
                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //       + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                  + " <br /><br /> " + "UserName: " + userName
                    //                  + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                  + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body, adminmail));

                }
                // }

            }
            catch (Exception ex)
            {
                Logger.Error("Create user" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> UpdateInsurance([FromForm] InsuranceUpdateUserDtoOutput input)
        {
            CheckUpdatePermission();
            UserDto userDto = new UserDto();
            UploadProfileOutput profileOutput = new UploadProfileOutput();

            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            UserMetaDetails userMetadetails1 = new UserMetaDetails();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(input.PhoneNumber))
                    {
                        var IsPhoneExists = await _userRepository.CheckDuplicatePhone(input.PhoneNumber, user.Id);
                        if (IsPhoneExists)
                        {
                            userDto.Message = "PhoneNumber" + " " + input.PhoneNumber + " already exists.";
                            userDto.StatusCode = 401;
                            return userDto;
                        }
                    }

                    //Create Audit log for before update
                    string beforeValueAuditDetails = string.Empty;
                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject UserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                        UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = UserJsonObj.ToString();
                    }
                    else
                    {
                        beforeValueAuditDetails = Utility.Serialize(user);
                    }
                    createEventsInput.BeforeValue = beforeValueAuditDetails;

                    if (input.UploadProfilePicture != null)
                    {
                        profileOutput = await SaveProfilePic(input.UploadProfilePicture, user.Id, user.UserName);
                    }
                    user.Name = input.Name == null ? user.Name : input.Name;
                    user.Surname = input.Surname == null ? user.Surname : input.Surname;
                    user.Timezone = input.Timezone;
                    user.Address = input.Address;
                    user.DateOfBirth = input.DateOfBirth;
                    user.Gender = input.Gender;
                    user.Title = input.Title;
                    if (!string.IsNullOrEmpty(profileOutput.ProfilePath))
                    {
                        if (_uplodedFilePath.IsBlob)
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath;
                            user.BlobSequenceNumber = profileOutput.BlobSequenceNumber;
                            user.VersionId = profileOutput.VersionId;
                            user.EncryptionKeySha256 = profileOutput.EncryptionKeySha256;
                            user.EncryptionScope = profileOutput.EncryptionScope;
                            user.CreateRequestId = profileOutput.CreateRequestId;
                            user.IsBlobStorage = _uplodedFilePath.IsBlob;
                        }
                        else
                        {
                            user.UploadProfilePicture = profileOutput.ProfilePath.Replace(_env.ContentRootPath, "");
                        }
                    }
                    user.City = input.City;
                    user.State = input.State;
                    user.Country = input.Country;
                    user.PostalCode = input.PostalCode;
                    user.PhoneNumber = input.PhoneNumber;

                    string UserRolename = input.RoleNames[0].ToString().ToLower();
                    user.UserType = UserRolename == "patient" ? UserType.Patient.ToString() : UserRolename == "consultant" ? UserType.Consultant.ToString() : UserRolename == "diagnostic" ? UserType.Diagnostic.ToString() : UserRolename == "insurance" ? UserType.Insurance.ToString() : UserRolename == "familydoctor" ? UserType.FamilyDoctor.ToString() : UserRolename == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();

                    CheckErrors(await _userManager.UpdateAsync(user));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }
                    //var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                    if (userMetaDetails == null)
                    {
                        userMetadetails1 = new UserMetaDetails
                        {
                            Company = input.Company,
                            RequestedOncologySubspecialty = input.RequestedOncologySubspecialty,
                            UserId = input.UserId,
                            CreatedOn = DateTime.UtcNow,
                            TenantId = AbpSession.TenantId,
                            IsActive = true,
                            AdminNotes = input.AdminNotes,
                            AmountDeposit = input.AmountDeposit
                        };
                        if (_session.UniqueUserId != null)
                        {
                            userMetadetails1.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.InsertAsync(userMetadetails1);

                    }
                    else
                    {
                        userMetaDetails.Company = input.Company == null ? userMetaDetails.Company : input.Company;
                        userMetaDetails.RequestedOncologySubspecialty = input.RequestedOncologySubspecialty == null ? userMetaDetails.RequestedOncologySubspecialty : input.RequestedOncologySubspecialty;
                        userMetaDetails.UserId = input.UserId;
                        userMetaDetails.Id = userMetaDetails.Id;
                        userMetaDetails.UpdatedOn = DateTime.UtcNow;
                        userMetaDetails.AdminNotes = input.AdminNotes;
                        userMetaDetails.AmountDeposit = input.AmountDeposit;
                        if (_session.UniqueUserId != null)
                        {
                            userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                    }

                    userDto = MapToEntityDto(user);
                    userDto.password = string.Empty;
                    userDto.Message = "User updated successfully.";
                    userDto.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    JObject updatedUserJsonObj = JObject.Parse(Utility.Serialize(user));
                    JObject updatedUserMetaDetailsJsonObj = new JObject();
                    if (userMetaDetails != null)
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                    }
                    else
                    {
                        updatedUserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetadetails1));
                    }
                    updatedUserJsonObj.Merge(updatedUserMetaDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedUserJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = "The information of " + user.Title + " " + user.FullName + " insurance has been updated successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }
                else
                {
                    userDto.Message = "User not exists.";
                    userDto.StatusCode = 401;

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Update User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<UserDto> DeleteUser(GetUserDto input)
        {
            UserDto userDto = new UserDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                int count = 0;
                var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                if (user != null)
                {
                    var roleIds = user.Roles.Select(x => x.RoleId).ToArray();
                    var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                    var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1 || x.AppointmentSlotId == Guid.Empty);
                    if (roles.Contains("DIAGNOSTIC"))
                    {
                        var result = await _diagnosticsCaseRepository.GetAllListAsync(x => x.IsCompleted == false && x.UserId == input.UserId);
                        count = result.Count();
                    }
                    else if (roles.Contains("PATIENT") || roles.Contains("INSURANCE") || roles.Contains("MEDICALLEGAL"))
                    {
                        count = (from da in appointment.AsEnumerable()
                                 join cr in _consultReportRepository.GetAll().AsEnumerable()
                                 on da.Id equals cr.AppointmentId
                                 where da.UserId == input.UserId
                                 && cr.IsCompleted == false
                                 select da).ToList().Count();
                    }
                    else if (roles.Contains("CONSULTANT"))
                    {
                        count = (from da in appointment.AsEnumerable()
                                 join cr in _consultReportRepository.GetAll().AsEnumerable()
                                 on da.Id equals cr.AppointmentId
                                 where da.DoctorId == input.UserId
                                 && cr.IsCompleted == false
                                 select da).ToList().Count();
                    }
                    else if (roles.Contains("FAMILYDOCTOR"))
                    {
                        var userconsent = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId == input.UserId);
                        if (userconsent != null && userconsent.Count > 0)
                        {
                            count = (from c in userconsent.AsEnumerable()
                                     join uc in _consultReportRepository.GetAll().AsEnumerable()
                                     on c.AppointmentId equals uc.AppointmentId
                                     join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                     on c.AppointmentId equals da.Id
                                     where
                                      uc.IsCompleted == false
                                     select c).ToList().Count();
                        }

                    }
                    if (count == 0)
                    {
                        var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                        if (userMetaDetails != null)
                        {
                            //if (user != null)
                            //{
                            user.IsActive = false;
                            userMetaDetails.IsActive = false;
                            userMetaDetails.DeletedOn = DateTime.UtcNow;
                            if (_session.UniqueUserId != null)
                            {
                                userMetaDetails.DeletedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                            await _userManager.DeleteAsync(user);
                            userDto.Message = "User deleted successfully.";
                            userDto.StatusCode = 200;
                            //}
                            //else
                            //{
                            //    userDto.Message = "No user found.";
                            //    userDto.StatusCode = 401;
                            //}

                            stopwatch.Stop();

                            //Log Audit Events
                            string userAuditDetails = string.Empty;
                            JObject UserJsonObj = JObject.Parse(Utility.Serialize(user));
                            JObject UserMetaDetailsJsonObj = new JObject();

                            UserMetaDetailsJsonObj = JObject.Parse(Utility.Serialize(userMetaDetails));
                            UserJsonObj.Merge(UserMetaDetailsJsonObj, new JsonMergeSettings
                            {
                                MergeArrayHandling = MergeArrayHandling.Concat
                            });
                            userAuditDetails = UserJsonObj.ToString();

                            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                            createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                            createEventsInput.Parameters = userAuditDetails;
                            createEventsInput.Operation = user.Title + " " + user.FullName + " has been deleted.";
                            createEventsInput.Component = "User";
                            createEventsInput.Action = "Delete";

                            await _auditReportAppService.CreateAuditEvents(createEventsInput);
                        }
                        else
                        {
                            //if (user != null)
                            //{
                            user.IsActive = false;
                            await _userManager.DeleteAsync(user);
                            userDto.Message = "User deleted successfully.";
                            userDto.StatusCode = 200;
                            //}
                            //else
                            //{
                            //    userDto.Message = "No user found.";
                            //    userDto.StatusCode = 401;
                            //}
                            stopwatch.Stop();

                            //Log Audit Events

                            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                            createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                            createEventsInput.Parameters = Utility.Serialize(user);
                            createEventsInput.Operation = user.Title + " " + user.FullName + " has been deleted.";
                            createEventsInput.Component = "User";
                            createEventsInput.Action = "Delete";

                            await _auditReportAppService.CreateAuditEvents(createEventsInput);
                        }
                    }
                    else
                    {
                        userDto.Message = "You can't delete this user due to some booking(s) are associated with this user.";
                        userDto.StatusCode = 401;
                    }

                }
                else
                {
                    userDto.Message = "No user found.";
                    userDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Delete User" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        [AbpAuthorize]
        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        //[AbpAuthorize]
        protected override UserDto MapToEntityDto(User user)
        {
            var userDto = new UserDto();
            try
            {
                if (user != null)
                {
                    var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                    var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                    userDto = base.MapToEntityDto(user);
                    if (!string.IsNullOrEmpty(user.UploadProfilePicture))
                    {
                        if (user.IsBlobStorage)
                        {
                            userDto.ProfileUrl = _blobContainer.Download(user.UploadProfilePicture);
                        }
                        else
                        {
                            string path = _env.ContentRootPath + user.UploadProfilePicture;
                            string DocumentfileExt = Path.GetExtension(path);
                            byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                            userDto.ProfileUrl = Convert.ToBase64String(b);
                        }

                    }
                    userDto.RoleNames = roles.ToArray();
                    userDto.Message = "Get user detrails.";
                    userDto.StatusCode = 200;
                }
                else
                {
                    userDto.Message = "No user is available.";
                    userDto.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("MapToEntityDto" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }


            return userDto;
        }

        [AbpAuthorize]
        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        [AbpAuthorize]
        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = new User();
            try
            {

                user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            }
            catch (Exception ex)
            {
                Logger.Error("GetEntityByIdAsync" + ex.StackTrace);

            }
            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);

        }

        [AbpAuthorize]
        public async Task<ChangePasswordOutputDto> ChangePassword(ChangePasswordDto input)
        {
            ChangePasswordOutputDto output = new ChangePasswordOutputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();

                if (_abpSession.UserId == null)
                {
                    output.Message = "Please log in before attemping to change password.";
                    output.StatusCode = 401;
                }
                long userId = _abpSession.UserId.Value;
                var user = await _userManager.GetUserByIdAsync(userId);
                var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
                if (loginAsync.Result != AbpLoginResultType.Success)
                {
                    output.Message = "Your 'Existing Password' did not match the one on record.  Please try again or contact an administrator for assistance in resetting your password.";
                    output.StatusCode = 401;
                }
                else if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
                {
                    output.Message = "Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.";
                    output.StatusCode = 401;
                }
                else
                {
                    user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                    output.Ischanged = true;
                    output.Message = "Password changed succesufully.";
                    output.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(user);
                    createEventsInput.Operation = user.Title + " " + user.FullName + " has changed the password successfully.";
                    createEventsInput.Component = "User";
                    createEventsInput.Action = "Change Password";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }

                CurrentUnitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Delete Request Error:" + ex.StackTrace);
            }

            return output;
        }

        [AbpAuthorize]
        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to reset password.");
            }
            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();
            }

            return true;
        }

        public async Task<SignUpOutput> SignUp(SignUpInput input)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                string newPassword = User.CreateRandomPassword();
                string userName = GenrateUserName(input.Name, input.Surname);
                Guid newUserId = Guid.NewGuid();
                var user = new User
                {
                    EmailAddress = input.EmailAddress,
                    UserName = userName,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    Password = newPassword,
                    Name = input.Name,
                    Surname = input.Surname,
                    UniqueUserId = newUserId,
                    Timezone = "America/Toronto",
                    UserType = "Patient"

                };

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
                var IsUserExists = await _userManager.FindByNameAsync(userName);
                SignUpOutput signUpOutput = new SignUpOutput();
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists != null)
                {
                    signUpOutput.Message = "EmailId" + " " + input.EmailAddress + " already exists.";
                    signUpOutput.StatusCode = 401;
                }
                else
                {
                    for (int i = 0; i < input.RoleNames.Length; i++)
                    {
                        var IsRoleExists = await _roleManager.FindByNameAsync(input.RoleNames[i]);
                        if (IsRoleExists == null)
                        {
                            signUpOutput.Message = "Role" + " " + input.RoleNames[i] + " not exists.";
                            signUpOutput.StatusCode = 401;
                            return signUpOutput;
                        }
                    }

                    CheckErrors(await _userManager.CreateAsync(user, newPassword));

                    if (input.RoleNames != null)
                    {
                        CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                    }

                    CurrentUnitOfWork.SaveChanges();
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    signUpOutput.Password = newPassword;
                    signUpOutput.Message = "User has been registered in the system.Please check your email.";
                    signUpOutput.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    var auditEvent = new AuditEvent
                    {
                        ExecutionTime = DateTime.UtcNow,
                        BrowserInfo = _clientInfoProvider.BrowserInfo,
                        ClientName = _clientInfoProvider.ComputerName,
                        ClientIpAddress = _clientInfoProvider.ClientIpAddress,
                        ExecutionDuration = stopwatch.ElapsedMilliseconds,
                        Parameters = Utility.Serialize(user),
                        ImpersonatorUserId = AbpSession.ImpersonatorUserId == null ? null : AbpSession.ImpersonatorUserId,
                        TenantId = AbpSession.TenantId,
                        UserId = user.Id,
                        ImpersonatorTenantId = AbpSession.ImpersonatorTenantId == null ? null : AbpSession.ImpersonatorTenantId,
                        UniqueUserId = newUserId,
                        Operation = user.Title + " " + user.FullName + " has been registered in ETeleHealth system.",
                        Component = "User",
                        Action = "Sign up",
                        BeforeValue = null,
                        AfterValue = null,
                        IsImpersonating = AbpSession.ImpersonatorUserId == null ? false : true
                    };

                    await _auditEventRepository.InsertAndGetIdAsync(auditEvent);

                    string Name = input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase();
                    string message = "Welcome to ETeleHealth Doctors: Cancer Care On-Demand. Your health is essential."
                                + " <br /><br /> " + "Whether you have a new cancer diagnosis or you are looking for secondary opinion on your current diagnosis, our platform will safely connect you with cancer specialists to discuss your cancer diagnosis and available treatment options towards achieving a complete picture of your disease. Our mission is to provide accessible, fast, and high-quality cancer care anywhere and anytime."
                                + " <br /><br /> " + "The platform can be accessed through: <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">ETeleHealth Portal</a>  with below credentials -"
                                + " <br /><br /> " + "UserName: " + userName
                                    + " <br /> " + "Password: " + newPassword
                                + " <br /><br /> " + "**Your current time zone will be automatically set to �Toronto EST�. You will be able to change it once you completed your Profile.**";
                    string signature = "Thank you,"
                                    + " <br /> " + "Dr. John Doe"
                                    + " <br /> " + "CEO, ETeleHealth Doctors Inc.";

                    string body = _templateAppService.GetTemplates(Name, message, signature);

                    //string body = "Hello and Welcome " + "<b>" + input.Name.ToPascalCase() + " " + input.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Thank you for registering with EMRO." + "</b> <br /><br /> "
                    //     + "You can login to the <a href=\"" + _configuration["PORTAL_URL"] + "/#/signin" + "\">EMRO Portal</a>  with below credentials - "
                    //                + " <br /><br /> " + "UserName: " + userName
                    //                + " <br /> " + "Password: " + newPassword + " <br /><br />"
                    //                + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                    BackgroundJob.Enqueue<IPaubox>(x => x.PauboxSendEmailAsync(input.EmailAddress, "Welcome to ETeleHealth Doctors", body));

                    //BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, "EMRO login credentials", body, adminmail));

                }
                return signUpOutput;

            }
            catch (Exception ex)
            {
                SignUpOutput signUpOutput = new SignUpOutput();
                signUpOutput.Message = "Something went wrong, please try again.";
                signUpOutput.StatusCode = 500;
                Logger.Error("SignUp API" + ex.StackTrace);
                return signUpOutput;
            }

        }

        [AbpAuthorize]
        public async Task<GetUserDtoOutput> GetUserDetailsId(GetUserDto input)
        {
            GetUserDtoOutput getUserDtoOutput = new GetUserDtoOutput();
            try
            {
                if (input.UserId != Guid.Empty)
                {

                    var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId && x.IsActive == true);
                    string basestring = string.Empty;
                    if (user != null)
                    {
                        var usermetadata = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == input.UserId && x.IsActive == true).FirstOrDefault();

                        var roles = await _userManager.GetRolesAsync(user);
                        if (!string.IsNullOrEmpty(user.UploadProfilePicture))
                        {
                            if (user.IsBlobStorage)
                            {
                                basestring = _blobContainer.Download(user.UploadProfilePicture);
                            }
                            else
                            {
                                string path = _env.ContentRootPath + user.UploadProfilePicture;
                                string DocumentfileExt = Path.GetExtension(path);
                                byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                                basestring = Convert.ToBase64String(b);
                            }

                        }
                        if (roles.Contains("Admin") || roles.Contains("Diagnostic"))
                        {
                            DaignosticListOutput daignostic = new DaignosticListOutput();
                            daignostic.Name = user.Name;
                            daignostic.Surname = user.Surname;
                            daignostic.IsActive = user.IsActive;
                            daignostic.RoleNames = roles.ToArray();
                            daignostic.Timezone = user.Timezone;
                            daignostic.Address = user.Address;
                            daignostic.City = user.City;
                            daignostic.State = user.State;
                            daignostic.Country = user.Country;
                            daignostic.PostalCode = user.PostalCode;
                            daignostic.DateOfBirth = user.DateOfBirth;
                            daignostic.Gender = user.Gender == "0" ? "" : user.Gender;
                            daignostic.UserType = user.UserType;
                            daignostic.Title = user.Title;
                            daignostic.PhoneNumber = user.PhoneNumber;
                            daignostic.EmailAddress = user.EmailAddress;
                            daignostic.UploadProfilePicture = basestring;

                            if (usermetadata != null)
                            {
                                daignostic.AdminNotes = usermetadata.AdminNotes;
                            }
                            getUserDtoOutput.Daignostic = daignostic;
                        }
                        else if (roles.Contains("Patient"))
                        {
                            PatientListOutput patient = new PatientListOutput();
                            patient.Name = user.Name;
                            patient.Surname = user.Surname;
                            patient.IsActive = user.IsActive;
                            patient.RoleNames = roles.ToArray();
                            patient.Timezone = user.Timezone;
                            patient.Address = user.Address;
                            patient.City = user.City;
                            patient.State = user.State;
                            patient.Country = user.Country;
                            patient.PostalCode = user.PostalCode;
                            patient.DateOfBirth = user.DateOfBirth;
                            patient.Gender = user.Gender == "0" ? "" : user.Gender;
                            patient.UserType = user.UserType;
                            patient.Title = user.Title;
                            patient.PhoneNumber = user.PhoneNumber;
                            patient.EmailAddress = user.EmailAddress;
                            patient.UploadProfilePicture = basestring;
                            patient.ID = user.Id;

                            if (usermetadata != null)
                            {
                                if (usermetadata.FamilyDoctorId != null && usermetadata.FamilyDoctorId != Guid.Empty)
                                {
                                    string familybasestring = string.Empty;
                                    var family = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == usermetadata.FamilyDoctorId);
                                    patient.FamilyDoctorId = (Guid)usermetadata.FamilyDoctorId;
                                    patient.FamilyDoctorTitle = family.Title;
                                    patient.DoctorFirstName = family.Name;
                                    patient.DoctorLastName = family.Surname;
                                    patient.DoctorAddress = family.Address;
                                    patient.DoctorCity = family.City;
                                    patient.DoctorCountry = family.Country;
                                    patient.DoctorState = family.State;
                                    patient.DoctorPostalCodes = family.PostalCode;
                                    patient.DoctorTelePhone = family.PhoneNumber;
                                    patient.DoctorEmailID = family.EmailAddress;
                                    patient.AdminNotes = usermetadata.AdminNotes;
                                    patient.HospitalAffiliation = usermetadata.HospitalAffiliation;
                                    patient.OncologySpecialty = usermetadata.OncologySpecialty;

                                    if (!string.IsNullOrEmpty(family.UploadProfilePicture))
                                    {
                                        if (family.IsBlobStorage)
                                        {
                                            familybasestring = _blobContainer.Download(family.UploadProfilePicture);
                                        }
                                        else
                                        {
                                            string path = _env.ContentRootPath + family.UploadProfilePicture;
                                            string DocumentfileExt = Path.GetExtension(path);
                                            byte[] b = Utility.DecryptFile(path, _configuration[Utility.ENCRYPTION_KEY]);
                                            familybasestring = Convert.ToBase64String(b);
                                        }
                                        patient.familydoctorUploadProfilePicture = familybasestring;
                                    }
                                    // patient.ConsentMedicalInformationWithCancerCareProvider = usermetadata.ConsentForSharingAndAccessingMedicalInformationWithDoctors;
                                }
                                else
                                {
                                    if (usermetadata.InviteUserId != null && usermetadata.InviteUserId != Guid.Empty)
                                    {
                                        var invite = await _inviteUserRepository.GetAsync((Guid)usermetadata.InviteUserId);
                                        patient.FamilyDoctorId = Guid.Empty;
                                        patient.DoctorFirstName = invite.FirstName;
                                        patient.DoctorLastName = invite.LastName;
                                        patient.DoctorAddress = invite.Address;
                                        patient.DoctorCity = invite.City;
                                        patient.DoctorCountry = invite.Country;
                                        patient.DoctorState = invite.State;
                                        patient.DoctorPostalCodes = invite.PostalCodes;
                                        patient.DoctorTelePhone = invite.TelePhone;
                                        patient.DoctorEmailID = invite.EmailAddress;
                                    }


                                }
                                patient.ConsentMedicalInformationWithCancerCareProvider = usermetadata.ConsentMedicalInformationWithCancerCareProvider;
                                patient.AdminNotes = usermetadata.AdminNotes;

                            }

                            getUserDtoOutput.Patient = patient;
                        }
                        else if (roles.Contains("Consultant"))
                        {
                            ConsuntantListOutput consuntant = new ConsuntantListOutput();
                            consuntant.Name = user.Name;
                            consuntant.Surname = user.Surname;
                            consuntant.IsActive = user.IsActive;
                            consuntant.RoleNames = roles.ToArray();
                            consuntant.Timezone = user.Timezone;
                            consuntant.Address = user.Address;
                            consuntant.City = user.City;
                            consuntant.State = user.State;
                            consuntant.Country = user.Country;
                            consuntant.PostalCode = user.PostalCode;
                            consuntant.DateOfBirth = user.DateOfBirth;
                            consuntant.Gender = user.Gender == "0" ? "" : user.Gender;
                            consuntant.UserType = user.UserType;
                            consuntant.Title = user.Title;
                            consuntant.PhoneNumber = user.PhoneNumber;
                            consuntant.EmailAddress = user.EmailAddress;
                            consuntant.UploadProfilePicture = basestring;


                            if (usermetadata != null)
                            {
                                consuntant.HospitalAffiliation = usermetadata.HospitalAffiliation;
                                consuntant.CurrentAffiliation = usermetadata.CurrentAffiliation;
                                consuntant.ConsultationType = usermetadata.ConsultationType.Split(",");
                                consuntant.ProfessionalBio = usermetadata.ProfessionalBio;
                                consuntant.UndergraduateMedicalTraining = usermetadata.UndergraduateMedicalTraining;
                                consuntant.OncologySpecialty = usermetadata.OncologySpecialty;
                                consuntant.OncologySubSpecialty = usermetadata.OncologySubSpecialty.Split(",");
                                consuntant.MedicalAssociationMembership = usermetadata.MedicalAssociationMembership;
                                consuntant.LicensingNumber = usermetadata.LicensingNumber;
                                if (usermetadata.DateConfirmed != null)
                                {
                                    consuntant.DateConfirmed = usermetadata.DateConfirmed;

                                }
                                consuntant.Certificate = usermetadata.Certificate;
                                consuntant.Residency1 = usermetadata.Residency1;
                                consuntant.Residency2 = usermetadata.Residency2;
                                consuntant.Fellowship = usermetadata.Fellowship;
                                consuntant.ExperienceOrTraining = usermetadata.ExperienceOrTraining;
                                consuntant.Credentials = usermetadata.Credentials;
                                consuntant.AdminNotes = usermetadata.AdminNotes;

                            }
                            getUserDtoOutput.Consuntant = consuntant;
                        }
                        else if (roles.Contains("FamilyDoctor"))
                        {
                            FamilyDoctorListOutput familyDoctor = new FamilyDoctorListOutput();
                            familyDoctor.Name = user.Name;
                            familyDoctor.Surname = user.Surname;
                            familyDoctor.IsActive = user.IsActive;
                            familyDoctor.RoleNames = roles.ToArray();
                            familyDoctor.Timezone = user.Timezone;
                            familyDoctor.Address = user.Address;
                            familyDoctor.City = user.City;
                            familyDoctor.State = user.State;
                            familyDoctor.Country = user.Country;
                            familyDoctor.PostalCode = user.PostalCode;
                            familyDoctor.DateOfBirth = user.DateOfBirth;
                            familyDoctor.Gender = user.Gender == "0" ? "" : user.Gender;
                            familyDoctor.UserType = user.UserType;
                            familyDoctor.Title = user.Title;
                            familyDoctor.PhoneNumber = user.PhoneNumber;
                            familyDoctor.EmailAddress = user.EmailAddress;
                            familyDoctor.UploadProfilePicture = basestring;

                            if (usermetadata != null)
                            {
                                familyDoctor.HospitalAffiliation = usermetadata.HospitalAffiliation;
                                familyDoctor.CurrentAffiliation = usermetadata.CurrentAffiliation;
                                familyDoctor.ProfessionalBio = usermetadata.ProfessionalBio;
                                familyDoctor.UndergraduateMedicalTraining = usermetadata.UndergraduateMedicalTraining;
                                familyDoctor.OncologySpecialty = usermetadata.OncologySpecialty;
                                familyDoctor.MedicalAssociationMembership = usermetadata.MedicalAssociationMembership;
                                familyDoctor.LicensingNumber = usermetadata.LicensingNumber;
                                if (usermetadata.DateConfirmed != null)
                                {
                                    familyDoctor.DateConfirmed = usermetadata.DateConfirmed;

                                }
                                familyDoctor.Certificate = usermetadata.Certificate;
                                familyDoctor.Residency1 = usermetadata.Residency1;
                                familyDoctor.Residency2 = usermetadata.Residency2;
                                familyDoctor.Fellowship = usermetadata.Fellowship;
                                familyDoctor.ExperienceOrTraining = usermetadata.ExperienceOrTraining;
                                familyDoctor.Credentials = usermetadata.Credentials;
                                familyDoctor.AdminNotes = usermetadata.AdminNotes;
                            }

                            getUserDtoOutput.FamilyDoctor = familyDoctor;
                        }
                        else if (roles.Contains("Insurance"))
                        {
                            InsuranceListOutput insurance = new InsuranceListOutput();
                            insurance.Name = user.Name;
                            insurance.Surname = user.Surname;
                            insurance.IsActive = user.IsActive;
                            insurance.RoleNames = roles.ToArray();
                            insurance.Timezone = user.Timezone;
                            insurance.Address = user.Address;
                            insurance.City = user.City;
                            insurance.State = user.State;
                            insurance.Country = user.Country;
                            insurance.PostalCode = user.PostalCode;
                            insurance.DateOfBirth = user.DateOfBirth;
                            insurance.Gender = user.Gender == "0" ? "" : user.Gender;
                            insurance.UserType = user.UserType;
                            insurance.Title = user.Title;
                            insurance.PhoneNumber = user.PhoneNumber;
                            insurance.EmailAddress = user.EmailAddress;
                            insurance.UploadProfilePicture = basestring;
                            if (usermetadata != null)
                            {
                                insurance.Company = usermetadata.Company;
                                insurance.RequestedOncologySubspecialty = usermetadata.RequestedOncologySubspecialty;
                                insurance.AdminNotes = usermetadata.AdminNotes;
                                insurance.AmountDeposit = usermetadata.AmountDeposit;

                            }
                            getUserDtoOutput.Insurance = insurance;
                        }
                        else if (roles.Contains("MedicalLegal"))
                        {
                            MedicalLegalListOutput medicalLegal = new MedicalLegalListOutput();
                            medicalLegal.Name = user.Name;
                            medicalLegal.Surname = user.Surname;
                            medicalLegal.IsActive = user.IsActive;
                            medicalLegal.RoleNames = roles.ToArray();
                            medicalLegal.Timezone = user.Timezone;
                            medicalLegal.Address = user.Address;
                            medicalLegal.City = user.City;
                            medicalLegal.State = user.State;
                            medicalLegal.Country = user.Country;
                            medicalLegal.PostalCode = user.PostalCode;
                            medicalLegal.DateOfBirth = user.DateOfBirth;
                            medicalLegal.Gender = user.Gender == "0" ? "" : user.Gender;
                            medicalLegal.UserType = user.UserType;
                            medicalLegal.Title = user.Title;
                            medicalLegal.PhoneNumber = user.PhoneNumber;
                            medicalLegal.EmailAddress = user.EmailAddress;
                            medicalLegal.UploadProfilePicture = basestring;
                            if (usermetadata != null)
                            {
                                medicalLegal.Company = usermetadata.Company;
                                medicalLegal.RequestedOncologySubspecialty = usermetadata.RequestedOncologySubspecialty;
                                medicalLegal.AdminNotes = usermetadata.AdminNotes;
                                medicalLegal.AmountDeposit = usermetadata.AmountDeposit;

                            }
                            getUserDtoOutput.MedicalLegal = medicalLegal;
                        }

                        getUserDtoOutput.StatusCode = 200;
                        getUserDtoOutput.Message = "Get details successfully.";
                    }
                    else
                    {
                        getUserDtoOutput.StatusCode = 401;
                        getUserDtoOutput.Message = "No record found.";
                    }
                }
                else
                {
                    getUserDtoOutput.StatusCode = 401;
                    getUserDtoOutput.Message = "Bad Request.";
                }

            }
            catch (Exception ex)
            {
                getUserDtoOutput.StatusCode = 500;
                getUserDtoOutput.Message = ex.Message;
                Logger.Error("Get user by ID" + ex.StackTrace);
            }
            return getUserDtoOutput;
        }

        [AbpAuthorize]
        public async Task<UserDetailsListOutput> GetUserByRoles(GetUsetByRoleInput input)
        {
            UserDetailsListOutput userDetailsListOutput = new UserDetailsListOutput();
            var list = new List<GetUserDetailsListOutput>();
            try
            {
                var user = await _userManager.GetUsersInRoleAsync(input.RoleName.ToUpper());
                var usermetadata = _userMetaDetailsRepository.GetAll().Where(x => x.IsActive == true).ToList();
                list = (from users in user.AsEnumerable()
                        join usetmeta in usermetadata.AsEnumerable()
                        on users.UniqueUserId equals usetmeta.UserId
                        into gj
                        from subpet in gj.DefaultIfEmpty()
                            //where users.IsActive == true
                        select new GetUserDetailsListOutput
                        {

                            Name = users.Name,
                            Surname = users.Surname,
                            IsActive = users.IsActive,
                            Timezone = users.Timezone,
                            Address1 = users.Address,
                            CityId = users.City,
                            StateId = users.State,
                            CountryId = users.Country,
                            PostalCodeId = users.PostalCode,
                            DateOfBirth = users.DateOfBirth,
                            Gender = users.Gender == "0" ? "" : users.Gender,
                            UserType = users.UserType,
                            Title = users.Title,
                            OncologySpecialty = subpet?.OncologySpecialty ?? String.Empty,
                            OncologySubSpecialty = subpet?.OncologySubSpecialty ?? String.Empty,
                            LicensingNumber = subpet?.LicensingNumber ?? String.Empty,
                            Company = subpet?.Company ?? String.Empty,
                            RequestedOncologySubspecialty = subpet?.RequestedOncologySubspecialty ?? String.Empty,
                            EmailAddress = users.EmailAddress,
                            PhoneNumber = users.PhoneNumber,
                            UserId = users.UniqueUserId,
                            Id = users.Id
                        }).OrderBy(x => x.Name).ToList();

                userDetailsListOutput.getUserDetailsListOutputs = list;
                userDetailsListOutput.Message = "List get successfully.";
                userDetailsListOutput.StatusCode = 200;
            }
            catch (Exception ex)
            {
                userDetailsListOutput.Message = ex.Message;
                userDetailsListOutput.StatusCode = 500;
                Logger.Error("User List " + ex.StackTrace);
            }
            return userDetailsListOutput;
        }

        [AbpAuthorize]
        public async Task<UserDto> IsUserStatus(GetUserDto input)
        {
            UserDto userDto = new UserDto();
            try
            {
                if (input.UserId != Guid.Empty)
                {
                    var user = await Repository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);

                    var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId && x.IsActive == true).FirstOrDefault();
                    if (user != null)
                    {
                        if (userMetaDetails != null)
                        {
                            if (user.IsActive == true)
                            {
                                userMetaDetails.IsActive = false;
                                userMetaDetails.UpdatedOn = DateTime.UtcNow;
                                if (_session.UniqueUserId != null)
                                {
                                    userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                user.IsActive = false;
                                await _userManager.UpdateAsync(user);
                                userDto.Message = "User deactivated successfully.";
                                userDto.StatusCode = 200;
                            }
                            else
                            {
                                userMetaDetails.UpdatedOn = DateTime.UtcNow;
                                if (_session.UniqueUserId != null)
                                {
                                    userMetaDetails.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                user.IsActive = true;
                                await _userManager.UpdateAsync(user);
                                userDto.Message = "User activated successfully.";
                                userDto.StatusCode = 200;
                            }
                        }
                        else
                        {

                            if (user.IsActive == true)
                            {
                                user.IsActive = false;
                                await _userManager.UpdateAsync(user);
                                userDto.Message = "User deactivated successfully.";
                                userDto.StatusCode = 200;
                            }
                            else
                            {
                                user.IsActive = true;
                                await _userManager.UpdateAsync(user);
                                userDto.Message = "User activated successfully.";
                                userDto.StatusCode = 200;
                            }
                        }
                    }
                    else
                    {
                        userDto.Message = "No record found.";
                        userDto.StatusCode = 200;
                    }
                }
                else
                {
                    userDto.Message = "Bad Request.";
                    userDto.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("User Status" + ex.StackTrace);
                userDto.Message = ex.Message;
                userDto.StatusCode = 500;
            }
            return userDto;
        }

        [AbpAuthorize]
        public async Task<GetUserlistoutput> GetUserEmails(Guid? Id)
        {
            GetUserlistoutput getUserlistoutput = new GetUserlistoutput();
            var list = new List<GetUserlist>();
            try
            {
                var user = await Repository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == Id).FirstOrDefaultAsync();
                if (user != null)
                {
                    var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                    var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                    var users = await _userManager.GetUsersInRoleAsync("ADMIN");

                    if (roles.Contains("ADMIN"))
                    {
                        list = Repository.GetAll().Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                        {
                            Id = x.UniqueUserId,
                            EmailId = x.EmailAddress,
                            FullName = x.FullName
                        }).ToList();
                    }
                    else
                    {

                        if (roles.Contains("FAMILYDOCTOR"))
                        {
                            var consent = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == Id);
                            list = users.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                            {
                                Id = x.UniqueUserId,
                                EmailId = x.EmailAddress,
                                FullName = x.FullName
                            }).ToList();

                            if (consent.Count > 0)
                            {
                                var patientuser = (from c in consent.AsEnumerable()
                                                   join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                   on c.AppointmentId equals da.Id
                                                   join ud in _userRepository.GetAll().AsEnumerable()
                                                   on da.UserId equals ud.UniqueUserId
                                                   select new GetUserlist
                                                   {
                                                       Id = ud.UniqueUserId,
                                                       EmailId = ud.EmailAddress,
                                                       FullName = ud.FullName
                                                   }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();
                                var doctoruser = (from c in consent.AsEnumerable()
                                                  join da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                  on c.AppointmentId equals da.Id
                                                  join ud in _userRepository.GetAll().AsEnumerable()
                                                  on da.DoctorId equals ud.UniqueUserId
                                                  select new GetUserlist
                                                  {
                                                      Id = ud.UniqueUserId,
                                                      EmailId = ud.EmailAddress,
                                                      FullName = ud.FullName
                                                  }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();
                                list = list.Union(patientuser.Union(doctoruser)).ToList();

                            }
                        }

                        else if (roles.Contains("CONSULTANT"))
                        {
                            var appoinment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == Id);
                            list = users.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                            {
                                Id = x.UniqueUserId,
                                EmailId = x.EmailAddress,
                                FullName = x.FullName
                            }).ToList();

                            if (appoinment.Count > 0)
                            {
                                var patientuser = (from da in appoinment.AsEnumerable()
                                                   join ud in _userRepository.GetAll().AsEnumerable()
                                                   on da.UserId equals ud.UniqueUserId
                                                   select new GetUserlist
                                                   {
                                                       Id = ud.UniqueUserId,
                                                       EmailId = ud.EmailAddress,
                                                       FullName = ud.FullName
                                                   }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();
                                var doctoruser = (from da in appoinment.AsEnumerable()
                                                  join cf in _userConsentRepository.GetAll().Where(x => x.FamilyDoctorId != null).AsEnumerable()
                                                  on da.Id equals cf.AppointmentId
                                                  join ud in _userRepository.GetAll().AsEnumerable()
                                                  on cf.FamilyDoctorId equals ud.UniqueUserId
                                                  select new GetUserlist
                                                  {
                                                      Id = ud.UniqueUserId,
                                                      EmailId = ud.EmailAddress,
                                                      FullName = ud.FullName
                                                  }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();

                                var requestTest = await _requestTestRepository.GetAllListAsync(x => x.ConsultantId == Id);

                                var diaguser = (from rt in requestTest.AsEnumerable()
                                                join ud in _userRepository.GetAll().AsEnumerable()
                                                on rt.DiagnosticId equals ud.UniqueUserId
                                                select new GetUserlist
                                                {
                                                    Id = ud.UniqueUserId,
                                                    EmailId = ud.EmailAddress,
                                                    FullName = ud.FullName
                                                }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                .Select(g => g.First())
                                                                .ToList();

                                list = list.Union(patientuser.Union(doctoruser.Union(diaguser))).ToList();
                            }
                        }
                        else if (roles.Contains("DIAGNOSTIC"))
                        {
                            var appointment = await _diagnosticsCaseRepository.GetAllListAsync(x => x.UserId == Id);
                            list = users.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                            {
                                Id = x.UniqueUserId,
                                EmailId = x.EmailAddress,
                                FullName = x.FullName
                            }).ToList();
                            if (appointment.Count > 0)
                            {
                                var patientuser = (from da in appointment.AsEnumerable()
                                                   join ud in _userRepository.GetAll().AsEnumerable()
                                                   on da.PatientId equals ud.UniqueUserId
                                                   select new GetUserlist
                                                   {
                                                       Id = ud.UniqueUserId,
                                                       EmailId = ud.EmailAddress,
                                                       FullName = ud.FullName
                                                   }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                     .Select(g => g.First())
                                                                     .ToList();

                                list = list.Union(patientuser).ToList();
                            }

                            var requestTest = await _requestTestRepository.GetAllListAsync(x => x.DiagnosticId == Id);

                            var patiUser = (from rt in requestTest.AsEnumerable()
                                            join ud in _userRepository.GetAll().AsEnumerable()
                                            on rt.PatientId equals ud.UniqueUserId
                                            select new GetUserlist
                                            {
                                                Id = ud.UniqueUserId,
                                                EmailId = ud.EmailAddress,
                                                FullName = ud.FullName
                                            }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                            .Select(g => g.First())
                                                            .ToList();

                            var consUser = (from rt in requestTest.AsEnumerable()
                                            join ud in _userRepository.GetAll().AsEnumerable()
                                            on rt.ConsultantId equals ud.UniqueUserId
                                            select new GetUserlist
                                            {
                                                Id = ud.UniqueUserId,
                                                EmailId = ud.EmailAddress,
                                                FullName = ud.FullName
                                            }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                            .Select(g => g.First())
                                                            .ToList();

                            list = list.Union(consUser.Union(patiUser)).ToList();

                        }
                        else
                        {
                            var appoinment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == Id);
                            list = users.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                            {
                                Id = x.UniqueUserId,
                                EmailId = x.EmailAddress,
                                FullName = x.FullName
                            }).ToList();

                            if (appoinment.Count > 0)
                            {
                                var patientuser = (from da in appoinment.AsEnumerable()
                                                   join ud in _userRepository.GetAll().AsEnumerable()
                                                   on da.DoctorId equals ud.UniqueUserId
                                                   select new GetUserlist
                                                   {
                                                       Id = ud.UniqueUserId,
                                                       EmailId = ud.EmailAddress,
                                                       FullName = ud.FullName
                                                   }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();
                                var doctoruser = (from da in appoinment.AsEnumerable()
                                                  join cf in _userConsentRepository.GetAll().Where(x => x.FamilyDoctorId != null).AsEnumerable()
                                                  on da.Id equals cf.AppointmentId
                                                  join ud in _userRepository.GetAll().AsEnumerable()
                                                  on cf.FamilyDoctorId equals ud.UniqueUserId
                                                  select new GetUserlist
                                                  {
                                                      Id = ud.UniqueUserId,
                                                      EmailId = ud.EmailAddress,
                                                      FullName = ud.FullName
                                                  }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                 .Select(g => g.First())
                                                                 .ToList();

                                if (roles.Contains("PATIENT"))
                                {
                                    var requestTest = await _requestTestRepository.GetAllListAsync(x => x.PatientId == Id);

                                    var diaguser = (from rt in requestTest.AsEnumerable()
                                                    join ud in _userRepository.GetAll().AsEnumerable()
                                                    on rt.DiagnosticId equals ud.UniqueUserId
                                                    select new GetUserlist
                                                    {
                                                        Id = ud.UniqueUserId,
                                                        EmailId = ud.EmailAddress,
                                                        FullName = ud.FullName
                                                    }).GroupBy(g => new { g.Id, g.EmailId, g.FullName })
                                                                    .Select(g => g.First())
                                                                    .ToList();

                                    list = list.Union(diaguser).ToList();
                                }

                                list = list.Union(patientuser.Union(doctoruser)).ToList();

                            }
                        }
                    }

                    //    list =  Repository.GetAll().Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new GetUserlist
                    //{
                    //    Id = x.UniqueUserId,
                    //    EmailId = x.EmailAddress,
                    //    FullName = x.FullName
                    //}).ToList();
                    list = list.GroupBy(x => x.Id).Select(y => y.First()).ToList(); //remove duplication ;)

                    if (Id.HasValue)
                    {
                        list = list.Where(x => x.Id != Id.Value).ToList();
                    }
                    getUserlistoutput.Items = list;
                    getUserlistoutput.Message = "Get Emails List.";
                    getUserlistoutput.StatusCode = 200;
                }
                else
                {
                    getUserlistoutput.Message = "User not exists.";
                    getUserlistoutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {

                getUserlistoutput.Message = ex.Message;
                getUserlistoutput.StatusCode = 500;
                Logger.Error("Get User list" + ex.StackTrace);
            }
            return getUserlistoutput;
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
        protected async Task<UploadProfileOutput> SaveProfilePic(IFormFile formFile, long UserId, string UserName)
        {
            string ProfileExt = string.Empty;
            string ProfileFileName = string.Empty;
            bool ProfileFileStatus = false;
            string ProfileFilePath = string.Empty;
            string EncryptedKey = _configuration[Utility.ENCRYPTION_KEY];
            string filePath = string.Empty;
            UploadDocumentInput uploadDocumentInput = new UploadDocumentInput();
            UploadProfileOutput profileOutput = new UploadProfileOutput();
            if (formFile != null)
            {
                if (UserId > 0)
                {
                    var userprofile = await Repository.GetAsync(UserId);
                    if (userprofile != null)
                    {
                        string path = _env.ContentRootPath + userprofile.UploadProfilePicture;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        //bool isdeleted = _blobContainer.Delete(userprofile.UploadProfilePicture);
                    }
                }
                Guid Profileobj = Guid.NewGuid();
                ProfileExt = Path.GetExtension(formFile.FileName);
                ProfileFileName = Profileobj.ToString() + ProfileExt;
                if (_uplodedFilePath.IsBlob)
                {
                    filePath = Path.Combine(_uplodedFilePath.BlobProfilePicturePath + UserName + _uplodedFilePath.Slash);
                    filePath = filePath + ProfileFileName;
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
                        profileOutput.IsBlobStorage = _uplodedFilePath.IsBlob;
                        profileOutput.ProfilePath = filePath;

                    }

                }
                else
                {
                    filePath = Path.Combine(_env.ContentRootPath + _uplodedFilePath.Slash + _uplodedFilePath.ProfilePicturePath + UserName.ToString() + _uplodedFilePath.Slash);


                    if (!Directory.Exists(filePath))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(filePath);
                    }
                    ProfileFilePath = filePath + "Enc_" + ProfileFileName;
                    filePath = filePath + ProfileFileName;

                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await formFile.CopyToAsync(stream);
                        stream.Flush();

                    }
                    ProfileFileStatus = Utility.EncryptFile(filePath, ProfileFilePath, EncryptedKey);
                    if (ProfileFileStatus)
                    {

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    profileOutput.ProfilePath = ProfileFilePath;
                }

            }
            return profileOutput;

        }

        [AbpAuthorize]
        public async Task<UserDto> GetById(Guid Id)
        {
            var userDto = new UserDto();
            try
            {

                var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.UniqueUserId == Id);
                userDto = MapToEntityDto(user);
                var usermetadata = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == Id && x.IsActive == true).FirstOrDefault();
                if (usermetadata != null)
                {
                    userDto.DepositAmount = usermetadata.AmountDeposit;
                }
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == user.UniqueUserId);
                var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                if (roles.Contains("PATIENT"))
                {
                    int bookingCount = (from da in appointment.AsEnumerable()
                                        join us in _userRepository.GetAll().AsEnumerable()
                                        on da.UserId equals us.UniqueUserId
                                        join cr in _consultReportRepository.GetAll().AsEnumerable()
                                        on da.Id equals cr.AppointmentId
                                        where us.UserType == "Patient" && cr.IsCompleted == false
                                        join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                        on da.AppointmentSlotId equals aps.Id
                                        where da.MissedAppointment == true && Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                        select da).Count();
                    if (bookingCount == 0)
                    {
                        userDto.IsAllowtoNewBooking = true;
                    }

                    int missedCount = (from da in appointment.AsEnumerable()
                                       join us in _userRepository.GetAll().AsEnumerable()
                                       on da.UserId equals us.UniqueUserId
                                       join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                       on da.AppointmentSlotId equals aps.Id
                                       where da.MissedAppointment == true && Convert.ToDateTime(aps.SlotZoneEndTime) <= DateTime.UtcNow
                                       select da).Count();

                    if (missedCount != 0)
                    {
                        userDto.IsMissedAppointment = true;
                    }
                    else
                    {
                        userDto.IsMissedAppointment = false;
                    }

                }
                if (roles.Contains("FAMILYDOCTOR"))
                {
                    var activeCase = await _consultReportAppService.ActiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                    {
                        UserId = Id,
                        RoleName = "FAMILYDOCTOR"
                    });

                    var archiveCase = await _consultReportAppService.ArchiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                    {
                        UserId = Id,
                        RoleName = "FAMILYDOCTOR"
                    });

                    var inviteUser = await _inviteRepository.GetAllListAsync(x => x.CreatedBy == Id && (x.UserType == "Patient" || x.UserType == "Consultant"));

                    if ((activeCase.Items == null || activeCase.Items.Count == 0) && (archiveCase.Items == null || archiveCase.Items.Count == 0) && inviteUser.Count == 0)
                    {
                        userDto.IsCase = false;
                    }
                    else
                    {
                        userDto.IsCase = true;
                    }

                    var appoinment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == Id);
                    if (appoinment.Count > 0)
                    {
                        userDto.IsAppointment = true;
                        userDto.IsPayment = true;
                    }
                    else
                    {
                        userDto.IsAppointment = false;
                        userDto.IsPayment = false;
                    }
                }
                else if (roles.Contains("DIAGNOSTIC"))
                {

                    var activeCase = await _consultReportAppService.ActiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                    {
                        UserId = Id,
                        RoleName = "DIAGNOSTIC"
                    });

                    var archiveCase = await _consultReportAppService.ArchiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                    {
                        UserId = Id,
                        RoleName = "DIAGNOSTIC"
                    });

                    if ((activeCase.Items == null || activeCase.Items.Count == 0) && (archiveCase.Items == null || archiveCase.Items.Count == 0))
                    {
                        userDto.IsCase = false;
                    }
                    else
                    {
                        userDto.IsCase = true;
                    }

                    var appoinment = await _diagnosticsCaseRepository.GetAllListAsync(x => x.UserId == Id);
                    if (appoinment.Count > 0)
                    {
                        userDto.IsAppointment = true;
                    }
                    else
                    {
                        userDto.IsAppointment = false;
                    }
                }
                else
                {
                    if (roles.Contains("INSURANCE"))
                    {
                        var activeCase = await _consultReportAppService.ActiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                        {
                            UserId = Id,
                            RoleName = "INSURANCE"
                        });

                        var archiveCase = await _consultReportAppService.ArchiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                        {
                            UserId = Id,
                            RoleName = "INSURANCE"
                        });

                        if ((activeCase.Items == null || activeCase.Items.Count == 0) && (archiveCase.Items == null || archiveCase.Items.Count == 0))
                        {
                            userDto.IsCase = false;
                        }
                        else
                        {
                            userDto.IsCase = true;
                        }
                    }
                    if (roles.Contains("MEDICALLEGAL"))
                    {
                        var activeCase = await _consultReportAppService.ActiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                        {
                            UserId = Id,
                            RoleName = "MEDICALLEGAL"
                        });

                        var archiveCase = await _consultReportAppService.ArchiveCase(new OncologyConsultReports.Dto.ActiveCaseInput
                        {
                            UserId = Id,
                            RoleName = "MEDICALLEGAL"
                        });

                        if ((activeCase.Items == null || activeCase.Items.Count == 0) && (archiveCase.Items == null || archiveCase.Items.Count == 0))
                        {
                            userDto.IsCase = false;
                        }
                        else
                        {
                            userDto.IsCase = true;
                        }
                    }


                    if (appointment.Count() > 0)
                    {

                        //if (appointment.Where(x => x.IsBooked == 3).Count() > 0)
                        //{
                        //    var list = appointment.LastOrDefault(x => x.IsBooked == 3);
                        //    //if (roles.Contains("PATIENT"))
                        //    //{
                        //    //    userDto.IsAllowtoNewBooking = true;
                        //    //}
                        //    //else
                        //    //{
                        //    userDto.IsPayment = false;
                        //    userDto.IsIntake = true;
                        //    //}
                        //    userDto.AppoinmentId = list.Id;
                        //    userDto.DoctorId = list.DoctorId;
                        //    userDto.IsBookLater = list.AppointmentSlotId == Guid.Empty ? true : false;
                        //    userDto.IsAppointment = false;
                        //    userDto.NumberofPages = list.NoOfPages;
                        //}
                        //else

                        if (appointment.Where(x => x.IsBooked == 1).Count() == 0)
                        {
                            var list = appointment.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
                            if (list != null)
                            {
                                if (list.IsBooked == 0)
                                {
                                    userDto.IsAppointment = false;
                                    userDto.IsPayment = false;
                                    userDto.IsIntake = false;
                                    userDto.AppoinmentId = list.Id;
                                    userDto.DoctorId = list.DoctorId;

                                }
                                else if (list.IsBooked == 3)
                                {

                                    userDto.IsPayment = false;
                                    userDto.IsIntake = true;
                                    userDto.AppoinmentId = list.Id;
                                    userDto.DoctorId = list.DoctorId;
                                    userDto.IsBookLater = list.AppointmentSlotId == Guid.Empty ? true : false;
                                    userDto.IsAppointment = false;
                                    userDto.NumberofPages = list.NoOfPages;
                                }
                                else
                                {
                                    userDto.IsAppointment = false;
                                    userDto.IsPayment = false;
                                    userDto.IsIntake = false;
                                }
                                userDto.IsBookLater = list.AppointmentSlotId == Guid.Empty ? true : false;
                            }
                            else
                            {
                                userDto.IsAppointment = false;
                                userDto.IsPayment = false;
                                userDto.IsIntake = false;

                            }

                        }
                        else
                        {
                            var list = appointment.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
                            if (list.IsBooked == 0)
                            {
                                userDto.IsPayment = false;
                                userDto.IsAppointment = false;
                                userDto.IsIntake = false;


                            }
                            else if (list.IsBooked == 3)
                            {
                                //var list = appointment.LastOrDefault(x => x.IsBooked == 3);
                                //if (roles.Contains("PATIENT"))
                                //{
                                //    userDto.IsAllowtoNewBooking = true;
                                //}
                                //else
                                //{
                                userDto.IsPayment = false;
                                userDto.IsIntake = true;
                                //}
                                userDto.AppoinmentId = list.Id;
                                userDto.DoctorId = list.DoctorId;
                                userDto.IsBookLater = list.AppointmentSlotId == Guid.Empty ? true : false;
                                userDto.IsAppointment = false;
                                userDto.NumberofPages = list.NoOfPages;
                            }
                            else
                            {
                                userDto.IsPayment = true;
                                userDto.IsAppointment = true;
                                userDto.IsIntake = true;
                            }
                            userDto.AppoinmentId = list.Id;
                            userDto.DoctorId = list.DoctorId;
                            userDto.IsBookLater = list.AppointmentSlotId == Guid.Empty ? true : false;
                            //}
                        }
                    }
                    else
                    {
                        userDto.IsAppointment = false;
                        userDto.IsPayment = false;
                        userDto.IsIntake = false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("GetById" + ex.StackTrace);

            }
            return userDto;
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<UserDashBoardOutputDto> DashBoard()
        {
            UserDashBoardOutputDto output = new UserDashBoardOutputDto();
            try
            {
                var patient = await _userManager.GetUsersInRoleAsync("PATIENT");
                var consultant = await _userManager.GetUsersInRoleAsync("CONSULTANT");
                var familyDoctor = await _userManager.GetUsersInRoleAsync("FAMILYDOCTOR");
                var insurance = await _userManager.GetUsersInRoleAsync("INSURANCE");
                var medicalLegal = await _userManager.GetUsersInRoleAsync("MEDICALLEGAL");
                var diagnostic = await _userManager.GetUsersInRoleAsync("DIAGNOSTIC");

                output.Patient = patient.Count();
                output.Consultant = consultant.Count();
                output.FamilyDoctor = familyDoctor.Count();
                output.Diagnostic = diagnostic.Count();
                output.Insurance = insurance.Count();
                output.MedicalLegal = medicalLegal.Count();
                output.Message = "Get dashboard counts.";
                output.StatusCode = 200;

            }
            catch (Exception ex)
            {
                Logger.Error("GetById" + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        /// <summary>
        /// In this function we will associate the family doctor to patient if this doctor referred by the patient
        /// </summary>
        /// <param name="EmailID"> user email id</param>
        /// <param name="FamilyDoctorId"> user id for association </param>
        /// <returns></returns>
        protected async Task AssociateFamilydoctor(string EmailID, Guid FamilyDoctorId)
        {
            try
            {
                var invite = await _inviteUserRepository.FirstOrDefaultAsync(x => x.EmailAddress == EmailID && x.UserType == "FamilyDoctor");
                if (invite != null)
                {
                    var user = await _userMetaDetailsRepository.GetAllListAsync(x => x.InviteUserId != null && x.InviteUserId == invite.Id);
                    if (user.Count > 0)
                    {
                        foreach (var item in user)
                        {
                            var usermetadetails = await _userMetaDetailsRepository.GetAsync(item.Id);
                            usermetadetails.FamilyDoctorId = FamilyDoctorId;
                            await _userMetaDetailsRepository.UpdateAsync(usermetadetails);

                            var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                     join fa in _userConsentRepository.GetAll().AsEnumerable()
                                                     on da.Id equals fa.AppointmentId
                                                     where fa.FamilyDoctorId == null
                                                     && da.UserId == item.UserId
                                                     select new { da.UserId, fa.Id }).ToList();
                            if (doctorAppointment.Count > 0)
                            {
                                foreach (var appoint in doctorAppointment)
                                {
                                    var userConsent = await _userConsentRepository.GetAsync(appoint.Id);
                                    if (userConsent != null)
                                    {
                                        userConsent.FamilyDoctorId = FamilyDoctorId;
                                        //userConsent.InviteUserId = null;
                                        await _userConsentRepository.UpdateAsync(userConsent);
                                    }

                                }
                            }
                        }

                    }

                    var appointment = await _userConsentRepository.GetAllListAsync(x => x.InviteUserId != null && x.InviteUserId == invite.Id && x.FamilyDoctorId == null);

                    if (appointment.Count > 0)
                    {
                        var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                 join fa in appointment.AsEnumerable()
                                                 on da.Id equals fa.AppointmentId
                                                 where fa.FamilyDoctorId == null
                                                 select new { da.UserId, fa.Id }).ToList();
                        if (doctorAppointment != null && doctorAppointment.Count > 0)
                        {
                            foreach (var item in doctorAppointment)
                            {
                                var userConsent = await _userConsentRepository.GetAsync(item.Id);
                                if (userConsent != null)
                                {
                                    userConsent.FamilyDoctorId = FamilyDoctorId;
                                    //userConsent.InviteUserId = null;
                                    await _userConsentRepository.UpdateAsync(userConsent);
                                }
                                var users = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);
                                if (users != null)
                                {
                                    var usermetadetails = await _userMetaDetailsRepository.GetAsync(item.Id);
                                    usermetadetails.FamilyDoctorId = FamilyDoctorId;
                                    await _userMetaDetailsRepository.UpdateAsync(usermetadetails);
                                }

                            }

                        }
                    }



                }

            }
            catch (Exception ex)
            {
                Logger.Error("Associate Family doctor" + ex.Message + "Error :-" + ex.StackTrace);
            }
        }
    }
}

