using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common.Templates;
using EMRO.Consultant.Dto;
using EMRO.DiagnosticsRequestTest;
using EMRO.DoctorRequest;
using EMRO.Email;
using EMRO.InviteUsers;
using EMRO.Sessions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Consultant
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class ConsultantAppService : ApplicationService, IConsultantAppService
    {
        private readonly IRepository<RequestConsultant, Guid> _requestConsultantRepository;
        private readonly IRepository<RequestTest, Guid> _requestTestRepository;
        private readonly IRepository<InviteUser, Guid> _inviteUserRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly EmroAppSession _session;
        private readonly UserManager _userManager;
        private readonly IMailer _mailer;
        private IConfiguration _configuration;
        private readonly TemplateAppService _templateAppService;
        private readonly IAuditReportAppService _auditReportAppService;
        public ConsultantAppService(IRepository<RequestConsultant, Guid> requestConsultantRepository,
            IRepository<User, long> userRepository,
            IRepository<RequestTest, Guid> requestTestRepository,
        EmroAppSession session,
            UserManager userManager,
             IMailer mailer,
             IRepository<InviteUser, Guid> inviteUserRepository,
              IConfiguration configuration
            , TemplateAppService templateAppService
            , IAuditReportAppService auditReportAppService)
        {
            _requestConsultantRepository = requestConsultantRepository;
            _userRepository = userRepository;
            _session = session;
            _userManager = userManager;
            _mailer = mailer;
            _inviteUserRepository = inviteUserRepository;
            _configuration = configuration;
            _templateAppService = templateAppService;
            _requestTestRepository = requestTestRepository;
            _auditReportAppService = auditReportAppService;
        }

        public async Task<RequestOutputDto> Create(RequestInputDto input)
        {
            RequestOutputDto output = new RequestOutputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                string Type = input.RoleName.ToLower() == "patient" ? UserType.Patient.ToString() : input.RoleName.ToLower() == "consultant" ? UserType.Consultant.ToString() : input.RoleName.ToLower() == "diagnostic" ? UserType.Diagnostic.ToString() : input.RoleName.ToLower() == "insurance" ? UserType.Insurance.ToString() : input.RoleName.ToLower() == "familydoctor" ? UserType.FamilyDoctor.ToString() : input.RoleName.ToLower() == "medicallegal" ? UserType.MedicalLegal.ToString() : UserType.EmroAdmin.ToString();
                var IsEmailExists = await _userManager.FindByEmailAsync(input.EmailAddress);
                if (IsEmailExists == null)
                {
                    var request = new InviteUser
                    {
                        FirstName = input.FirstName,
                        LastName = input.LastName,
                        Hospital = input.Hospital,
                        TelePhone = input.PhoneNumber,
                        EmailAddress = input.EmailAddress,
                        CreatedOn = DateTime.UtcNow,
                        ReferedBy = input.UserId,
                        IsCompleted = false,
                        Status = "Pending",
                        UserType = Type,
                        IsActive = true
                    };

                    if (_session.UniqueUserId != null)
                    {
                        request.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    request.TenantId = AbpSession.TenantId;
                    Guid result = await _inviteUserRepository.InsertAndGetIdAsync(request);
                    output.Id = result;

                    stopwatch.Stop();

                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(request);
                    createEventsInput.Operation = "Invite sent to the " + Type + " to connect with ETeleHealth Cloud system.";
                    createEventsInput.Component = "Consultant";
                    createEventsInput.Action = "Create";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
                }

                var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);

                var user = await _userManager.GetUsersInRoleAsync("ADMIN");
                if (user != null)
                {
                    var ur = user.FirstOrDefault();
                    string body = string.Empty;
                    string Name = string.Empty;
                    string message = string.Empty;
                    if (input.RoleName.ToLower() == "consultant")
                    {
                        if (!string.IsNullOrEmpty(input.PhoneNumber))
                        {

                            Name = "ETeleHealth Admin";
                            message = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                      + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                      + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                                      + "Hospital :-  " + input.Hospital + " <br />"
                                      + "Email Address :-  " + input.EmailAddress + " <br />"
                                      + "Phone Number :-  " + input.PhoneNumber;
                            body = _templateAppService.GetTemplates(Name, message,"");
                            //body = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //          + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                            //           + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                            //           + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                            //           + "Hospital :-  " + input.Hospital + " <br />"
                            //           + "Email Address :-  " + input.EmailAddress + " <br />"
                            //           + "Phone Number :-  " + input.PhoneNumber + " <br /><br />"
                            //           + "Regards," + " <br />" + "EMRO Team";
                        }
                        else
                        {
                            Name = "ETeleHealth Admin";
                            message = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the ETeleHealth cloud system or he might have not yet entered his availability slots in the system."
                                      + "Please connect with consultant and onboard him in to the ETeleHealth Cloud system. Details of consultant is below -" + " <br /><br /> "
                                      + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                                      + "Hospital :-  " + input.Hospital + " <br />"
                                      + "Email Address :-  " + input.EmailAddress;
                            body = _templateAppService.GetTemplates(Name, message,"");

                            //body = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //             + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the EMRO cloud system or he might have not yet entered his availability slots in the system."
                            //             + "Please connect with consultant and onboard him in to the EMRO Cloud system. Details of consultant is below -" + " <br /><br /> "
                            //             + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                            //             + "Hospital :-  " + input.Hospital + " <br />"
                            //             + "Email Address :-  " + input.EmailAddress + " <br /><br />"
                            //             //+ "PhoneNumber:-  " + input.PhoneNumber + " <br />"
                            //             + "Regards," + " <br />" + "EMRO Team";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(input.PhoneNumber))
                        {
                            Name = "ETeleHealth Admin";
                            message = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the ETeleHealth cloud system."
                                       + "Please connect with him and onboard him in to the ETeleHealth Cloud system. Details of " + Type + " is below -" + " <br /><br /> "
                                       + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                                       + "Hospital :-  " + input.Hospital + " <br />"
                                       + "Email Address :-  " + input.EmailAddress + " <br />"
                                       + "Phone Number :-  " + input.PhoneNumber;
                            body = _templateAppService.GetTemplates(Name, message,"");

                            //body = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //          + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the EMRO cloud system."
                            //           + "Please connect with him and onboard him in to the EMRO Cloud system. Details of " + Type + " is below -" + " <br /><br /> "
                            //           + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                            //           + "Hospital :-  " + input.Hospital + " <br />"
                            //           + "Email Address :-  " + input.EmailAddress + " <br />"
                            //           + "Phone Number :-  " + input.PhoneNumber + " <br /><br />"
                            //           + "Regards," + " <br />" + "EMRO Team";
                        }
                        else
                        {
                            Name = "ETeleHealth Admin";
                            message = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the ETeleHealth cloud system."
                                         + "Please connect with him and onboard him in to the ETeleHealth Cloud system. Details of " + Type + " is below -" + " <br /><br /> "
                                         + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                                         + "Hospital :-  " + input.Hospital + " <br />"
                                         + "Email Address :-  " + input.EmailAddress;
                            body = _templateAppService.GetTemplates(Name, message,"");

                            //body = "Hello " + "<b>" + "EMRO Admin" + ",</b> <br /><br /> "
                            //             + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + " might be willing to connect with a " + Type + " who either is not in the EMRO cloud system."
                            //             + "Please connect with him and onboard him in to the EMRO Cloud system. Details of " + Type + " is below -" + " <br /><br /> "
                            //             + "Name :-  " + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + " <br />"
                            //             + "Hospital :-  " + input.Hospital + " <br />"
                            //             + "Email Address :-  " + input.EmailAddress + " <br /><br />"
                            //             //+ "PhoneNumber:-  " + input.PhoneNumber + " <br />"
                            //             + "Regards," + " <br />" + "EMRO Team";
                        }
                    }

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(ur.EmailAddress, "Request for " + Type, body, adminmail));
                    string doctorbody = string.Empty;
                    string userName = string.Empty;
                    string usermessage = string.Empty;
                    string emailSubject = string.Empty;
                    if (input.RoleName.ToLower() == "consultant" || input.RoleName.ToLower() == "familydoctor")
                    {
                        emailSubject = "Invite to join ETeleHealth";
                        userName = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                        usermessage = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and patients to discuss cancer diagnosis and available treatment options. " + " <br /><br /> " +
                                 "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
                                 "If you have any questions regarding the invitation from " + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                        doctorbody = _templateAppService.GetTemplates(userName, usermessage,"Welcome aboard,<br />ETeleHealth Team");
                        //doctorbody = "Hello " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> "
                        //           + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //         "If you are already in the EMRO cloud system then please add your availability slots and make yourself searchable to the patients else you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                        //         + "Regards," + " <br />" + "EMRO Team";
                    }
                    else if (input.RoleName.ToLower() == "medicallegal" || input.RoleName.ToLower() == "insurance")
                    {
                        emailSubject = "Invite to join ETeleHealth";
                        userName = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                        usermessage = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform. ETeleHealth.Cloud provides a safe and fast connection between Cancer Care providers and legal teamstowards gaining expert opinions on cancer diagnosis. " + " <br /><br /> " +
                                 "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
                                 "If you have any questions regarding the invitation from " + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                        doctorbody = _templateAppService.GetTemplates(userName, usermessage, "Welcome aboard,<br />ETeleHealth Team");
                        //doctorbody = "Hello " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> "
                        //           + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //         "If you are already in the EMRO cloud system then please add your availability slots and make yourself searchable to the patients else you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                        //         + "Regards," + " <br />" + "EMRO Team";
                    }
                    else if (input.RoleName.ToLower() == "diagnostic")
                    {
                        emailSubject = "Invite to join ETeleHealth";
                        userName = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                        usermessage = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has invited you to join ETeleHealth.Cloud, a virtual cancer care platform.  " + " <br /><br /> " +
                                 "ETeleHealth.Could provides: <br />" +
                                 "1. easy access to diagnostic requests from cancer care providers <br />" + 
                                 "2. safe and fast sharing of diagnostic reports with physicians and patients. <br /><br />" + 
                                 "Use this link below to set up your account and get started <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "\">ETeleHealth Portal</a> . " + " <br /><br /> " +
                                 "If you have any questions regarding the invitation from " + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ", you can reply to this email, and they will get immediately notified. Alternatively, feel free to contact the ETeleHealth Administrative team anytime. ";
                        doctorbody = _templateAppService.GetTemplates(userName, usermessage, "Welcome aboard,<br />ETeleHealth Team");
                        //doctorbody = "Hello " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> "
                        //           + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //         "If you are already in the EMRO cloud system then please add your availability slots and make yourself searchable to the patients else you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                        //         + "Regards," + " <br />" + "EMRO Team";
                    }
                    else if (input.RoleName.ToLower() == "patient")
                    {
                        emailSubject = "Request for " + Type;
                        userName = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                        usermessage = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the ETeleHealth Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                                  "Please register to click on <a style='color: #fff;' href=\"" + _configuration["PORTAL_URL"] + "/#/signup" + "\">ETeleHealth Portal</a> . ";
                        doctorbody = _templateAppService.GetTemplates(userName, usermessage,"");

                        //doctorbody = "Hello " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> "
                        //          + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //          "Please register to click on <a href=\"" + _configuration["PORTAL_URL"] + "/#/signup" + "\">EMRO Portal</a> . " + " <br /><br /> "
                        //        + "Regards," + " <br />" + "EMRO Team";
                    }
                    else
                    {
                        emailSubject = "Request for " + Type;
                        userName = input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase();
                        usermessage = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the ETeleHealth Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                                "If you are already in the ETeleHealth cloud system then please make yourself searchable to the persone else you may also contact ETeleHealth Admin " + ur.EmailAddress + " to be onboarded.";
                        doctorbody = _templateAppService.GetTemplates(userName, usermessage,"");

                        //doctorbody = "Hello " + "<b>" + input.FirstName.ToPascalCase() + " " + input.LastName.ToPascalCase() + ",</b> <br /><br /> "
                        //          + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + "  has requested and referred you to join the EMRO Cloud system as he may want to connect with you for some consultation. " + " <br /> " +
                        //        "If you are already in the EMRO cloud system then please make yourself searchable to the persone else you may also contact EMRO Admin " + ur.EmailAddress + " to be onboarded." + " <br /><br /> "
                        //        + "Regards," + " <br />" + "EMRO Team";
                    }

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.EmailAddress, emailSubject, doctorbody, adminmail));
                }

                if (IsEmailExists != null)
                {
                    output.Message = Type + " already exists and we will inform him about your request.";

                }
                else
                {
                    output.Message = "Your request has been sent successfully.";
                }
                output.StatusCode = 200;


            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Delete Request Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<RequestOutputDto> Delete(Guid RequestId)
        {
            RequestOutputDto output = new RequestOutputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (RequestId != Guid.Empty)
                {
                    var request = await _inviteUserRepository.GetAsync(RequestId);
                    request.DeletedOn = DateTime.UtcNow;
                    request.IsDeleted = true;
                    if (_session.UniqueUserId != null)
                    {
                        request.DeletedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    await _inviteUserRepository.UpdateAsync(request);
                    output.Message = "Request deleted successfully.";
                    output.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(request);
                    createEventsInput.Operation = request.FirstName.ToPascalCase() + " "+ request.LastName.ToPascalCase() + " User request has been deleted successfully.";
                    createEventsInput.Component = "Consultant";
                    createEventsInput.Action = "Delete";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);
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
                Logger.Error("Delete Request Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetRequestByIdDto> Get(Guid Id)
        {
            GetRequestByIdDto output = new GetRequestByIdDto();
            try
            {
                if (Id != Guid.Empty)
                {
                    var request = await _inviteUserRepository.GetAsync(Id);
                    if (request != null)
                    {
                        var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == request.ReferedBy);
                        if (user != null)
                        {
                            output.FirstName = user.Name;
                            output.LastName = user.Surname;
                            output.EmailAddress = user.EmailAddress;
                        }

                        output.DoctorFirstName = request.FirstName;
                        output.DoctorLastName = request.LastName;
                        output.DoctorEmailAddress = request.EmailAddress;
                        output.Hospital = request.Hospital;
                        output.UserId = request.ReferedBy;
                        output.PhoneNumber = request.TelePhone;
                        output.Id = request.Id;
                        output.IsCompleted = request.IsCompleted;
                        output.Status = request.Status;
                        output.Message = "Get details successfully.";
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
                Logger.Error("Get Request Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetRequestOutputDto> GetAll(GetAllRequestInputDto getAllRequestInputDto)
        {
            GetRequestOutputDto output = new GetRequestOutputDto();
            try
            {
                var list = await _inviteUserRepository.GetAllListAsync();
                output.Items = (from r in list.AsEnumerable()
                                join u in _userRepository.GetAll().AsEnumerable()
                                on r.ReferedBy equals u.UniqueUserId
                                select new GetRequestDto
                                {
                                    FirstName = u.Name,
                                    LastName = u.Surname,
                                    DoctorFirstName = r.FirstName,
                                    DoctorLastName = r.LastName,
                                    DoctorEmailAddress = r.EmailAddress,
                                    Hospital = r.Hospital,
                                    UserId = r.ReferedBy,
                                    PhoneNumber = r.TelePhone,
                                    EmailAddress = u.EmailAddress,
                                    Id = r.Id,
                                    IsCompleted = r.IsCompleted,
                                    Status = r.Status,
                                    CreatedOn = Convert.ToDateTime(r.CreatedOn),
                                    RoleName = r.UserType == "FamilyDoctor" ? "Family Doctor".ToUpper() 
                                    : r.UserType == "MedicalLegal" ? "Medical Legal".ToUpper() 
                                    : r.UserType == "EmroAdmin" ? "ADMIN" : r.UserType.ToUpper(),
                                    IsOnboarded = r.IsOnboarded
                                }).OrderByDescending(x => x.CreatedOn).ToList();
                output.Count = output.Items.Count;
                if (Convert.ToInt32(getAllRequestInputDto.limit) > 0 && Convert.ToInt32(getAllRequestInputDto.page) > 0 && output.Items.Count > 0)
                {
                    output.Items = output.Items.Skip((Convert.ToInt32(getAllRequestInputDto.page) - 1) * Convert.ToInt32(getAllRequestInputDto.limit)).Take(Convert.ToInt32(getAllRequestInputDto.limit)).ToList();
                }
                output.Message = "Get All Request successfully.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get All Request Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<RequestOutputDto> RequestForTest(RequestForTestInputDto input)
        {
            RequestOutputDto output = new RequestOutputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (input.PatientId != Guid.Empty && input.ConsultantId != Guid.Empty)
                {
                    var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.PatientId);
                    var consultant = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.ConsultantId);
                    var diagnostic = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.DiagnosticId);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    if (patient != null && consultant != null)
                    {

                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(patient.Timezone);
                        DateTime newdate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(input.DueDate), tzi);


                        string Name = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase();
                        string message = consultant.Name.ToPascalCase() + " " + consultant.Surname.ToPascalCase() + "  has requested you for " + input.ReportType + ". Please see below details :-" + " <br />" +
                            "Diagnostic Name :-  " + diagnostic.FullName + " <br />"
                            + "Report Type :-  " + input.ReportType + " <br />"
                            + "Report Details :-  " + input.ReportDetails + " <br />"
                            + "Diagnostic Email :- " + diagnostic.EmailAddress + " <br /><br />"
                            + "Due Date :-  " + newdate.Date;
                        string doctorbody = _templateAppService.GetTemplates(Name, message,"");

                        //string doctorbody = "Hello " + "<b>" + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ",</b> <br /><br /> "
                        //           + consultant.Name.ToPascalCase() + " " + consultant.Surname.ToPascalCase() + "  has requested you for " + input.ReportType + ". Please see below details :-" + " <br />" +
                        //              "Report Type :-  " + input.ReportType + " <br />"
                        //              + "Report Details :-  " + input.ReportDetails + " <br /><br />"
                        //              + "Due Date :-  " + newdate.Date + " <br /><br />"
                        //              + "Regards," + " <br />" + "EMRO Team";

                        string diagName = diagnostic.Name.ToPascalCase() + " " + diagnostic.Surname.ToPascalCase();
                        string diagMessage = consultant.Name.ToPascalCase() + " " + consultant.Surname.ToPascalCase() + " has requested you for " + input.ReportType + ". Please see below details :-" + " <br />" +
                            "Patient Name :- " + patient.FullName + " <br />" +
                            "Report Type :-  " + input.ReportType + " <br />" +
                            "Report Details :-  " + input.ReportDetails + " <br /><br />" +
                            "Due Date :-  " + newdate.Date;
                        string diagBody = _templateAppService.GetTemplates(diagName, diagMessage,"");

                        var request = new RequestTest
                        {
                            PatientId = input.PatientId,
                            ConsultantId = input.ConsultantId,
                            DiagnosticId = input.DiagnosticId,
                            DueDate = input.DueDate,
                            ReportType = input.ReportType,
                            CreatedOn = DateTime.UtcNow,
                            ReportDetails = input.ReportDetails
                        };

                        if (_session.UniqueUserId != null)
                        {
                            request.CreatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        request.TenantId = AbpSession.TenantId;
                        Guid result = await _requestTestRepository.InsertAndGetIdAsync(request);

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patient.EmailAddress, "Request for Test", doctorbody, adminmail));
                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(diagnostic.EmailAddress, "Request for Test", diagBody, adminmail));
                        output.Message = "Request sent successfully.";
                        output.StatusCode = 200;

                        stopwatch.Stop();

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(request);
                        createEventsInput.Operation = "User has requested for " + input.ReportType;
                        createEventsInput.Component = "Consultant";
                        createEventsInput.Action = "Request for Test";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
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
            }
            catch (Exception ex)
            {

                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Request For Test:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<RequestOutputDto> Update(RequestUpdateInputDto input)
        {
            RequestOutputDto output = new RequestOutputDto();
            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (input.Id != Guid.Empty)
                {
                    var list = await _inviteUserRepository.GetAsync(input.Id);
                    if (list != null)
                    {
                        //Create Audit log for before update
                        createEventsInput.BeforeValue = Utility.Serialize(list);

                        list.IsCompleted = input.IsCompleted;
                        list.IsOnboarded = input.IsCompleted;
                        list.OnboardedOn = DateTime.UtcNow;
                        if (input.IsCompleted)
                        {
                            list.Status = "Completed";
                            output.Message = "Request completed successfully.";
                        }
                        else
                        {
                            list.Status = "Pending";
                            output.Message = "Request status changed successfully.";
                        }
                        if (_session.UniqueUserId != null)
                        {
                            list.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        list.UpdatedOn = DateTime.UtcNow;

                        await _inviteUserRepository.UpdateAsync(list);

                        output.StatusCode = 200;

                        stopwatch.Stop();

                        //Log Audit Events
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(list);
                        createEventsInput.Operation = list.FirstName.ToPascalCase() + " " + list.LastName.ToPascalCase() + " Request status has been updated successfully.";
                        createEventsInput.Component = "Consultant";
                        createEventsInput.Action = "Update"; 
                        createEventsInput.AfterValue = Utility.Serialize(list);
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
                Logger.Error("Get All Request Error:" + ex.StackTrace);
            }
            return output;
        }
    }
}
