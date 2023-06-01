using Abp.Application.Services;
using Abp.Domain.Repositories;
using EMRO.Appointment.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRO.Authorization.Users;
using Abp.Runtime.Security;
using System.Linq;
using EMRO.Email;
using EMRO.AppointmentSlot;
using EMRO.UserConsents;
using EMRO.Patients.IntakeForm;
using EMRO.UsersMetaInfo;
using EMRO.Authorization.Roles;
using Microsoft.Extensions.Options;
using EMRO.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using EMRO.OncologyConsultReports;
using EMRO.Sessions;
using Hangfire;
using Abp.Domain.Uow;
using EMRO.Common.AzureBlobStorage;
using EMRO.InviteUsers;
using Abp.Extensions;
using EMRO.Common.Templates;
using EMRO.AuditReport;
using Abp.Auditing;
using System.Diagnostics;
using Newtonsoft.Json;
using EMRO.Common.CustomNotification;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common.AuditReport;
using Newtonsoft.Json.Linq;

namespace EMRO.Appointment
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class AppointmentAppService : ApplicationService, IAppointmentAppService
    {
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        //private readonly IRepository<AuditEvent, Guid> _auditEventRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IRepository<UserConsent, Guid> _userConsentRepository;
        private readonly IRepository<UserConsentPatientsDetails, Guid> _userConsentPatientsDetailsRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IMailer _mailer;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        private readonly RoleManager _roleManager;
        private readonly ConsultFee _consultFee;
        private readonly UplodedFilePath _uplodedFilePath;
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private readonly IRepository<OncologyConsultReport, Guid> _consultReportRepository;
        private readonly EmroAppSession _session;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IBlobContainer _blobContainer;
        private readonly IRepository<InviteUser, Guid> _inviteRepository;
        private readonly TemplateAppService _templateAppService;
        private IClientInfoProvider _clientInfoProvider;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        private readonly IAuditReportAppService _auditReportAppService;
        public AppointmentAppService(IDoctorAppointmentRepository doctorAppointmentRepository,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IRepository<User, long> userRepository, IRepository<UserConsent, Guid> userConsentRepository,
            IRepository<UserConsentPatientsDetails, Guid> userConsentPatientsDetailsRepository, IMailer mailer
            , IRepository<UserMetaDetails, Guid> userMetaDetailsRepository, RoleManager roleManager
            , IOptions<ConsultFee> consultFee
            , IOptions<UplodedFilePath> uplodedFilePath, IWebHostEnvironment env,
            IConfiguration configuration
            , IRepository<OncologyConsultReport, Guid> consultReportRepository
            , EmroAppSession session
            , IUnitOfWorkManager unitOfWorkManager
             , IBlobContainer blobContainer
            , IRepository<InviteUser, Guid> inviteRepository
            //, IRepository<AuditEvent, Guid> auditEventRepository
            , IClientInfoProvider clientInfoProvider
            , TemplateAppService templateAppService
            , ICustomNotificationAppService customNotificationAppService
            , IAuditReportAppService auditReportAppService)
        {
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _userConsentRepository = userConsentRepository;
            _userRepository = userRepository;
            _userConsentPatientsDetailsRepository = userConsentPatientsDetailsRepository;
            _mailer = mailer;
            _userMetaDetailsRepository = userMetaDetailsRepository;
            _roleManager = roleManager;
            _consultFee = consultFee.Value;
            _uplodedFilePath = uplodedFilePath.Value;
            _env = env;
            _configuration = configuration;
            _consultReportRepository = consultReportRepository;
            _session = session;
            _unitOfWorkManager = unitOfWorkManager;
            _blobContainer = blobContainer;
            _inviteRepository = inviteRepository;
            _templateAppService = templateAppService;
            //_auditEventRepository = auditEventRepository;
            _clientInfoProvider = clientInfoProvider;
            _customNotificationAppService = customNotificationAppService;
            _auditReportAppService = auditReportAppService;
        }


        public async Task<CreateBookAppointmentOutput> Create(CreateBookAppointmentInput input)
        {

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            CreateBookAppointmentOutput bookAppointmentOutput = new CreateBookAppointmentOutput();
            try
            {
                Logger.Info("Creating Appointment for input: " + input);

                //if (input.SlotId != Guid.Empty)
                //{
                //var solts = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.AppointmentSlotId == input.SlotId);
                //if (solts == null)
                //{
                var stopwatch = Stopwatch.StartNew();
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {

                    Guid userId = Guid.Parse(_session.UniqueUserId);


                    int? TenantId = null;

                    if (AbpSession.TenantId.HasValue)
                    {
                        TenantId = AbpSession.TenantId.Value;
                    }

                    var appointment = new DoctorAppointment
                    {

                        TenantId = TenantId,
                        Agenda = input.Agenda,
                        Title = input.Title,
                        DoctorId = input.DoctorId,
                        //PatientId = input.PatientId,
                        IsBooked = 0,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        Referral = input.Referral,
                        Type = input.Type,
                        Status = input.Status,
                        Flag = 1,
                        UserId = input.UserId,
                        meetingId = input.meetingId,
                        MissedAppointment = false
                    };
                    //if (input.SlotId != Guid.Empty && input.SlotId != null)
                    //{
                    appointment.AppointmentSlotId = input.SlotId;
                    //}
                    bookAppointmentOutput.Id = await _doctorAppointmentRepository.InsertAndGetIdAsync(appointment);

                    bookAppointmentOutput.Message = "Appointment booked successfully";
                    bookAppointmentOutput.StatusCode = 200;
                    bookAppointmentOutput.OriginalPaymentAmount = _consultFee.Amount;
                    unitOfWork.Complete();
                    stopwatch.Stop();

                    //var auditEvent = new AuditEvent
                    //{
                    //    ExecutionTime = DateTime.Now,
                    //    BrowserInfo = _clientInfoProvider.BrowserInfo,
                    //    ClientName = _clientInfoProvider.ComputerName,
                    //    ClientIpAddress = _clientInfoProvider.ClientIpAddress,
                    //    ExecutionDuration = stopwatch.ElapsedMilliseconds,
                    //    Parameters = JsonConvert.SerializeObject(appointment),
                    //    ImpersonatorUserId = AbpSession.ImpersonatorUserId,
                    //    TenantId = AbpSession.TenantId,
                    //    UserId = AbpSession.UserId,
                    //    ImpersonatorTenantId = AbpSession.ImpersonatorTenantId,
                    //    UserName = "",
                    //    UniqueUserId = appointment.UserId,
                    //    UserTitle = "",
                    //    Operation = appointment.UserId + " Appointment has been created with " + appointment.DoctorId,
                    //    Component = "Appointment",
                    //    Action = "Create",
                    //    BeforeValue = "",
                    //    AfterValue = "",
                    //    IsImpersonating = AbpSession.ImpersonatorTenantId == null ? false : true
                    //};

                    //await _auditEventRepository.InsertAsync(auditEvent);
                    var userDetails = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
                    var doctorDetails = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.DoctorId); 
                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(appointment);
                    createEventsInput.Operation = userDetails.Title + userDetails.FullName + " has initiated an appointment with doctor " + doctorDetails.FullName;
                    createEventsInput.Component = "Appointment";
                    createEventsInput.Action = "Create";
                    //createEventsInput.BeforeValue = "";
                    //createEventsInput.AfterValue = "";
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                }

            }
            catch (Exception ex)
            {
                Logger.Error("Create Appointment Error" + ex.StackTrace);

                bookAppointmentOutput.Message = "Error while booking appointment.";
                bookAppointmentOutput.StatusCode = 401;
            }
            return bookAppointmentOutput;
        }

        public GetBookAppointmentOutput Get(GetBookAppointmentInput input)
        {
            var appointmentDtoList = new List<BookAppointmentDto>();
            try
            {
                var appointments = _doctorAppointmentRepository.GetAllAppointments(input.AppointmentId);
                appointmentDtoList = appointments
                .Select(bookings => new BookAppointmentDto
                {
                    AppointmentId = bookings.Id,
                    Title = bookings.Title,
                    Agenda = bookings.Agenda,
                    Referral = bookings.Referral,
                    Type = bookings.Type,
                    CreatedOn = bookings.CreatedOn,
                    CreatedBy = Guid.Parse(_session.UniqueUserId),
                    Status = bookings.Status,
                    Flag = bookings.Flag,
                    meetingId = bookings.meetingId,
                }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Appointment Error" + ex.StackTrace);
            }
            return new GetBookAppointmentOutput { BookAppointments = appointmentDtoList };
        }

        public async Task<BookAppointmentOutput> Reschedule(UpdateBookAppointmentInput input)
        {
            BookAppointmentOutput bookAppointmentOutput = new BookAppointmentOutput();
            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            try
            {
                Logger.Info("Updating/Rescheduling an appointment for input: " + input);
                var stopwatch = Stopwatch.StartNew();
                var availableslot = await _doctorAppointmentRepository.GetAllListAsync(x => x.Id != input.Id && x.AppointmentSlotId == input.SlotId && x.IsBooked == 1);
                if (availableslot.Count() > 0)
                {
                    bookAppointmentOutput.Message = "This slot is not available";
                    bookAppointmentOutput.StatusCode = 401;
                }
                else
                {
                    Guid UserId = Guid.Parse(_session.UniqueUserId);

                    int? TenantId = null;

                    if (AbpSession.TenantId.HasValue)
                    {
                        TenantId = AbpSession.TenantId.Value;
                    }

                    var ModifiedUserName = _userRepository.FirstOrDefault(x => x.UniqueUserId == UserId).UserName;
                    var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == (Guid)input.Id);

                    //Create Audit log for before update
                    createEventsInput.BeforeValue = Utility.Serialize(appointment);

                    if (appointment.IsBooked == 1)
                    {
                        var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);

                        //Create Audit log for before update
                        string beforeValueAuditDetails = string.Empty;
                        JObject appointmentJsonObj = JObject.Parse(Utility.Serialize(appointment));
                        JObject slotDetailsJsonObj = new JObject();

                        slotDetailsJsonObj = JObject.Parse(Utility.Serialize(slot));
                        appointmentJsonObj.Merge(slotDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat
                        });
                        beforeValueAuditDetails = appointmentJsonObj.ToString();
                        createEventsInput.BeforeValue = beforeValueAuditDetails;

                        TimeSpan t = slot.SlotZoneTime.Subtract(DateTime.UtcNow);
                        if (t.TotalHours >= 48)
                        {
                            slot.IsBooked = 0;
                            await _doctorAppointmentSlotRepository.UpdateAsync(slot);
                        }
                        else if (appointment.MissedAppointment == true)
                        {
                            slot.IsBooked = 0;
                            await _doctorAppointmentSlotRepository.UpdateAsync(slot);
                        }
                        else
                        {
                            bookAppointmentOutput.Message = "You are not allowed to reschedule your appointment within 48 hours of slot time.";
                            bookAppointmentOutput.StatusCode = 200;
                            return bookAppointmentOutput;
                        }
                    }
                    appointment.DoctorId = input.DoctorId;
                    appointment.UserId = appointment.UserId;
                    appointment.UpdatedOn = DateTime.UtcNow;
                    appointment.UpdatedBy = UserId;
                    appointment.Status = input.Status;
                    appointment.AppointmentSlotId = input.SlotId;

                    await _doctorAppointmentRepository.UpdateAsync(appointment);

                    var newSlot = await _doctorAppointmentSlotRepository.GetAsync(input.SlotId);
                    newSlot.IsBooked = 1;
                    await _doctorAppointmentSlotRepository.UpdateAsync(newSlot);

                    bookAppointmentOutput.Id = appointment.Id;
                    bookAppointmentOutput.Message = "Appointment updated successfully";
                    bookAppointmentOutput.StatusCode = 200;

                    var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                    var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;

                    DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(newSlot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));
                    DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(newSlot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));

                    stopwatch.Stop();
                    //Log Audit Events
                    JObject updatedAppointmentJsonObj = JObject.Parse(Utility.Serialize(appointment));
                    JObject updatedSlotDetailsJsonObj = JObject.Parse(Utility.Serialize(newSlot));
                    updatedAppointmentJsonObj.Merge(updatedSlotDetailsJsonObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    string AfterValueAuditDetails = updatedAppointmentJsonObj.ToString();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = AfterValueAuditDetails;
                    createEventsInput.Operation = user.Title + " " + user.FullName + " has been reschedule an appointment with doctor " + doctor.FullName +
                        " on " + newSlot.AvailabilityDate + " at " + DateTime.Parse(newSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(newSlot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + newSlot.TimeZone + ")"; 
                    createEventsInput.Component = "Appointment";
                    createEventsInput.Action = "Reschedule";
                    createEventsInput.AfterValue = AfterValueAuditDetails;
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                    string message = "Your appointment  has been rescheduled." + " <br /><br /><br /> " + "Appointee Name: " + user.FullName
                                   + " <br /><br /><br /> " + "Appointment Date and Time: " + Convert.ToDateTime(newSlot.AvailabilityDate).Date + " "
                                   + DateTime.Parse(newSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(newSlot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + newSlot.TimeZone + ")" + " <br /><br />"
                                   + "Link to join the meeting will be send 10 minutes before the meeting time.";
                    string doctorMailBody = _templateAppService.GetTemplates(Name, message,"");

                    string userName = user.Name.ToPascalCase() + " " + user.Surname.ToPascalCase();
                    string usermessage = "Your appointment with Doctor has been rescheduled." 
                                   + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                                   + " <br /><br /><br /> " + "Appointment Date and Time: " + patientStartTime.ToString("MM/dd/yyyy") + " "
                                   + patientStartTime.ToString("hh:mm tt") + " to " + patientEndTime.ToString("hh:mm tt") + " (" + user.Timezone + ")" + " <br /><br />"
                                   + "Link to join the meeting will be send 10 minutes before the meeting time.";
                    string userMailBody = _templateAppService.GetTemplates(userName, usermessage,"");

                    //string doctorMailBody = " Hello " + "<b>" + doctor.Name + " " + doctor.Surname + ",</b> <br /><br /> " + "Your appointment  has been rescheduled." + " <br /><br /><br /> " + "Appointee Name: " + user.FullName
                    //                + " <br /><br /><br /> " + "Appointment Date and Time: " + Convert.ToDateTime(newSlot.AvailabilityDate).Date + " "
                    //                + DateTime.Parse(newSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(newSlot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + newSlot.TimeZone + ")" + " <br /><br />"
                    //                + "Link to join the meeting will be send 10 minutes before the meeting time."
                    //                //+ "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + input.Id + "/" + appointment.meetingId + "/" + input.UserId + "/" + user.UserType + "'>Click to Join</a> <br /><br />"
                    //                + " <br /><br /><br /> " + "Regards," + " <br />" + "Emro Team";

                    //string userMailBody = " Hello " + "<b>" + user.Name + " " + user.Surname + ",</b> <br /><br /> " + "Your appointment with Doctor has been rescheduled." + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                    //               + " <br /><br /><br /> " + "Appointment Date and Time: " + Convert.ToDateTime(newSlot.AvailabilityDate).Date + " "
                    //               + DateTime.Parse(newSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(newSlot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + newSlot.TimeZone + ")" + " <br /><br />"
                    //               + "Link to join the meeting will be send 10 minutes before the meeting time."
                    //               //+ "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + input.Id + "/" + appointment.meetingId + "/" + input.UserId + "/" + user.UserType + "'>Click to Join</a> <br /><br />"
                    //               + " <br /><br /><br /> " + "Regards," + " <br />" + "Emro Team";

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Reschedule", doctorMailBody, adminmail));

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, "Appointment Reschedule", userMailBody, adminmail));
                }



            }
            catch (Exception ex)
            {
                bookAppointmentOutput.Message = "Error while updating/Rescheduling appointment";
                bookAppointmentOutput.StatusCode = 401;
                Logger.Error("Update/Rescheduling Appointment Error" + ex.StackTrace);
            }
            return bookAppointmentOutput;
        }

        public async Task<BookAppointmentOutput> Delete(DeleteBookedAppointmentInput input)
        {
            BookAppointmentOutput bookAppointmentOutput = new BookAppointmentOutput();
            try
            {
                Logger.Info("Delete Booked Appointment for input: " + input);

                var bookedAppointment = await _doctorAppointmentRepository.GetAsync(input.Id);
                if (bookedAppointment != null)
                {
                    await _doctorAppointmentRepository.DeleteAsync(bookedAppointment);
                    bookAppointmentOutput.Message = "Appointment deleted successfully";
                    bookAppointmentOutput.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                bookAppointmentOutput.Message = "Error while deleting appointment";
                bookAppointmentOutput.StatusCode = 401;
                Logger.Error("Delete Appointment Error" + ex.StackTrace);
            }
            return bookAppointmentOutput;
        }

        public async Task<UpcomingBookAppointmentOutput> GetBookedAppointment(UpcomingBookAppointmentInput input)
        {
            UpcomingBookAppointmentOutput upcomingBookAppointmentOutput = new UpcomingBookAppointmentOutput();

            Logger.Info("Get Booked Appointment for input: " + input.Id);
            try
            {
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }
                if (AbpSession.UserId.HasValue)
                {
                    //if (AbpSession.UserId == Id)
                    //{
                    var family = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == input.Id).FirstOrDefault();
                    if (family != null)
                    {
                        var roleIds = family.Roles.Select(x => x.RoleId).ToArray();

                        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                        if (roles.Contains("FAMILYDOCTOR"))
                        {
                            var appoinment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == input.Id);
                            if (appoinment != null && appoinment.Count > 0)
                            {
                                var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                         join fa in appoinment.AsEnumerable()
                                                         on da.Id equals fa.AppointmentId
                                                         where da.IsBooked == 1
                                                         select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                                if (doctorAppointment != null && doctorAppointment.Count > 0)
                                {
                                    var list = (from da in doctorAppointment.AsEnumerable()
                                                join us in _userRepository.GetAll().AsEnumerable()
                                                on da.UserId equals us.UniqueUserId
                                                join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                on da.AppointmentSlotId equals aps.Id
                                                join apsn in _userRepository.GetAll().AsEnumerable()
                                                on da.DoctorId equals apsn.UniqueUserId
                                                where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                                orderby aps.SlotZoneEndTime ascending
                                                select new UpcomingBookAppointmentDto
                                                {
                                                    PatientName = us.FullName,
                                                    DoctorName = apsn.FullName,
                                                    AppointmentDate = aps.AvailabilityDate,
                                                    StartTime = aps.AvailabilityStartTime,
                                                    AppointmentId = da.Id,
                                                    SlotId = da.AppointmentSlotId,
                                                    DoctorId = da.DoctorId,
                                                    TimeZone = aps.TimeZone,
                                                    SlotStartTime = aps.SlotZoneTime,
                                                    SlotEndTime = aps.SlotZoneEndTime,
                                                    meetingId = da.meetingId,
                                                    BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                    Title = da.Title,
                                                    PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                    IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                    IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                    RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType,
                                                }).ToList();
                                    upcomingBookAppointmentOutput.Count = list.Count;
                                    if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                    {
                                        list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                    }
                                    upcomingBookAppointmentOutput.Items = list;
                                    upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                    upcomingBookAppointmentOutput.StatusCode = 200;
                                }
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else
                        {
                            var appoinment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == input.Id && x.IsBooked == 1);
                            var doctorAppointment = (from da in appoinment.AsEnumerable()
                                                     join fa in _userConsentRepository.GetAll().AsEnumerable()
                                                     on da.Id equals fa.AppointmentId
                                                     where da.IsBooked == 1
                                                     select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                            orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                AppointmentDate = aps.AvailabilityDate,
                                                StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                TimeZone = aps.TimeZone,
                                                SlotStartTime = aps.SlotZoneTime,
                                                SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                    }
                    else
                    {
                        upcomingBookAppointmentOutput.Message = "No record found.";
                        upcomingBookAppointmentOutput.StatusCode = 401;
                    }
                    //}
                    //else
                    //{
                    //    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    //    upcomingBookAppointmentOutput.StatusCode = 401;
                    //}
                }
                else
                {
                    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingBookAppointmentOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Booked Appointment Error" + ex.StackTrace);

                upcomingBookAppointmentOutput.Message = "Error while fetching booking appointment";
                upcomingBookAppointmentOutput.StatusCode = 401;
            }

            return upcomingBookAppointmentOutput;
        }

        public async Task<UpcomingBookAppointmentOutput> GetBookedAppointmentByUserIdAndAppointmentId(BookAppointmentInput input)
        {
            UpcomingBookAppointmentOutput upcomingBookAppointmentOutput = new UpcomingBookAppointmentOutput();

            Logger.Info("Get Booked Appointment for input: " + input.Id);
            try
            {
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }
                if (AbpSession.UserId.HasValue)
                {
                    //if (AbpSession.UserId == Id)
                    //{
                    var family = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == input.Id).FirstOrDefault();
                    if (family != null)
                    {
                        var roleIds = family.Roles.Select(x => x.RoleId).ToArray();

                        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                        if (roles.Contains("FAMILYDOCTOR"))
                        {
                            var appoinment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == input.Id);
                            if (appoinment != null && appoinment.Count > 0)
                            {
                                var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                         join fa in appoinment.AsEnumerable()
                                                         on da.Id equals fa.AppointmentId
                                                         where da.Id == input.AppointmentId
                                                         select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                                if (doctorAppointment != null && doctorAppointment.Count > 0)
                                {
                                    var list = (from da in doctorAppointment.AsEnumerable()
                                                join us in _userRepository.GetAll().AsEnumerable()
                                                on da.UserId equals us.UniqueUserId
                                                join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                on da.AppointmentSlotId equals aps.Id
                                                join apsn in _userRepository.GetAll().AsEnumerable()
                                                on da.DoctorId equals apsn.UniqueUserId
                                                where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                                orderby aps.SlotZoneEndTime ascending
                                                select new UpcomingBookAppointmentDto
                                                {
                                                    PatientName = us.FullName,
                                                    DoctorName = apsn.FullName,
                                                    AppointmentDate = aps.AvailabilityDate,
                                                    StartTime = aps.AvailabilityStartTime,
                                                    AppointmentId = da.Id,
                                                    SlotId = da.AppointmentSlotId,
                                                    DoctorId = da.DoctorId,
                                                    TimeZone = aps.TimeZone,
                                                    SlotStartTime = aps.SlotZoneTime,
                                                    SlotEndTime = aps.SlotZoneEndTime,
                                                    meetingId = da.meetingId,
                                                    BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                    Title = da.Title,
                                                    PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                    IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                    IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                    RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType,
                                                }).ToList();
                                    upcomingBookAppointmentOutput.Count = list.Count;
                                    if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                    {
                                        list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                    }
                                    upcomingBookAppointmentOutput.Items = list;
                                    upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                    upcomingBookAppointmentOutput.StatusCode = 200;
                                }
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else
                        {
                            var appoinment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == input.Id);
                            var doctorAppointment = (from da in appoinment.AsEnumerable()
                                                     join fa in _userConsentRepository.GetAll().AsEnumerable()
                                                     on da.Id equals fa.AppointmentId
                                                     where da.Id == input.AppointmentId
                                                     select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                            orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                AppointmentDate = aps.AvailabilityDate,
                                                StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                TimeZone = aps.TimeZone,
                                                SlotStartTime = aps.SlotZoneTime,
                                                SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                    }
                    else
                    {
                        upcomingBookAppointmentOutput.Message = "No record found.";
                        upcomingBookAppointmentOutput.StatusCode = 401;
                    }
                    //}
                    //else
                    //{
                    //    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    //    upcomingBookAppointmentOutput.StatusCode = 401;
                    //}
                }
                else
                {
                    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingBookAppointmentOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Booked Appointment Error" + ex.StackTrace);

                upcomingBookAppointmentOutput.Message = "Error while fetching booking appointment";
                upcomingBookAppointmentOutput.StatusCode = 401;
            }

            return upcomingBookAppointmentOutput;
        }

        public async Task<UpcomingBookAppointmentOutput> GetBookLaterAppointment(BookAppointmentInput input)
        {
            UpcomingBookAppointmentOutput upcomingBookAppointmentOutput = new UpcomingBookAppointmentOutput();

            Logger.Info("Get Booked Appointment for input: " + input.Id);
            try
            {
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }
                if (AbpSession.UserId.HasValue)
                {
                    //if (AbpSession.UserId == Id)
                    //{
                    var family = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == input.Id).FirstOrDefault();
                    if (family != null)
                    {
                        var roleIds = family.Roles.Select(x => x.RoleId).ToArray();

                        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                        if (roles.Contains("FAMILYDOCTOR"))
                        {
                            var appoinment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == input.Id);
                            if (appoinment != null && appoinment.Count > 0)
                            {
                                var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                         join fa in appoinment.AsEnumerable()
                                                         on da.Id equals fa.AppointmentId
                                                         where da.IsBooked == 1 && da.Id == input.AppointmentId
                                                         select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                                if (doctorAppointment != null && doctorAppointment.Count > 0)
                                {
                                    var list = (from da in doctorAppointment.AsEnumerable()
                                                join us in _userRepository.GetAll().AsEnumerable()
                                                on da.UserId equals us.UniqueUserId
                                                //join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                //on da.AppointmentSlotId equals aps.Id
                                                join apsn in _userRepository.GetAll().AsEnumerable()
                                                on da.DoctorId equals apsn.UniqueUserId
                                                //where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                                //orderby aps.SlotZoneEndTime ascending
                                                select new UpcomingBookAppointmentDto
                                                {
                                                    PatientName = us.FullName,
                                                    DoctorName = apsn.FullName,
                                                    //AppointmentDate = aps.AvailabilityDate,
                                                    //StartTime = aps.AvailabilityStartTime,
                                                    AppointmentId = da.Id,
                                                    SlotId = da.AppointmentSlotId,
                                                    DoctorId = da.DoctorId,
                                                    //TimeZone = aps.TimeZone,
                                                    //SlotStartTime = aps.SlotZoneTime,
                                                    //SlotEndTime = aps.SlotZoneEndTime,
                                                    meetingId = da.meetingId,
                                                    BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                    Title = da.Title,
                                                    PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                    //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                    //IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                    RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType,
                                                }).ToList();
                                    upcomingBookAppointmentOutput.Count = list.Count;
                                    if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                    {
                                        list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                    }
                                    upcomingBookAppointmentOutput.Items = list;
                                    upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                    upcomingBookAppointmentOutput.StatusCode = 200;
                                }
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else
                        {
                            var appoinment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == input.Id && x.IsBooked == 1);
                            var doctorAppointment = (from da in appoinment.AsEnumerable()
                                                     join fa in _userConsentRepository.GetAll().AsEnumerable()
                                                     on da.Id equals fa.AppointmentId
                                                     where da.IsBooked == 1
                                                     select new { da.UserId, da.AppointmentSlotId, da.DoctorId, da.Id, da.meetingId, da.Title, fa.PatientId }).ToList();
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            //join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            //on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            //where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                            //orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                //AppointmentDate = aps.AvailabilityDate,
                                                //StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                //TimeZone = aps.TimeZone,
                                                //SlotStartTime = aps.SlotZoneTime,
                                                //SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UserType == "Patient" ? us.UniqueUserId : da.PatientId == null ? Guid.Empty : (Guid)da.PatientId,
                                                //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                //IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                                RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Booked Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                    }
                    else
                    {
                        upcomingBookAppointmentOutput.Message = "No record found.";
                        upcomingBookAppointmentOutput.StatusCode = 401;
                    }
                    //}
                    //else
                    //{
                    //    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    //    upcomingBookAppointmentOutput.StatusCode = 401;
                    //}
                }
                else
                {
                    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingBookAppointmentOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Booked Appointment Error" + ex.StackTrace);

                upcomingBookAppointmentOutput.Message = "Error while fetching booking appointment";
                upcomingBookAppointmentOutput.StatusCode = 401;
            }

            return upcomingBookAppointmentOutput;
        }

        public async Task<UpcomingBookAppointmentOutput> GetMissedAppointments(UpcomingBookAppointmentInput input)
        {
            UpcomingBookAppointmentOutput upcomingBookAppointmentOutput = new UpcomingBookAppointmentOutput();
            var outputItems = new List<UpcomingBookAppointmentDto>();
            Logger.Info("Get Missed Appointment for input: " + input.Id);
            try
            {
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }
                if (AbpSession.UserId.HasValue)
                {
                    var family = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == input.Id).FirstOrDefault();
                    if (family != null)
                    {
                        var roleIds = family.Roles.Select(x => x.RoleId).ToArray();

                        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                        if (roles.Contains("FAMILYDOCTOR"))
                        {
                            var appoinment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId != null && x.FamilyDoctorId == input.Id);
                            if (appoinment != null && appoinment.Count > 0)
                            {
                                var doctorAppointment = (from da in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                                         join fa in appoinment.AsEnumerable()
                                                         on da.Id equals fa.AppointmentId
                                                         where da.IsBooked == 1 && da.MissedAppointment == true
                                                         select da).ToList();
                                if (doctorAppointment != null && doctorAppointment.Count > 0)
                                {
                                    var list = (from da in doctorAppointment.AsEnumerable()
                                                join us in _userRepository.GetAll().AsEnumerable()
                                                on da.UserId equals us.UniqueUserId
                                                join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                                on da.AppointmentSlotId equals aps.Id
                                                join apsn in _userRepository.GetAll().AsEnumerable()
                                                on da.DoctorId equals apsn.UniqueUserId
                                                where Convert.ToDateTime(aps.SlotZoneEndTime) <= DateTime.UtcNow
                                                orderby aps.SlotZoneEndTime ascending
                                                select new UpcomingBookAppointmentDto
                                                {
                                                    PatientName = us.FullName,
                                                    DoctorName = apsn.FullName,
                                                    AppointmentDate = aps.AvailabilityDate,
                                                    StartTime = aps.AvailabilityStartTime,
                                                    AppointmentId = da.Id,
                                                    SlotId = da.AppointmentSlotId,
                                                    DoctorId = da.DoctorId,
                                                    TimeZone = aps.TimeZone,
                                                    SlotStartTime = aps.SlotZoneTime,
                                                    SlotEndTime = aps.SlotZoneEndTime,
                                                    meetingId = da.meetingId,
                                                    BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                    Title = da.Title,
                                                    PatientId = us.UniqueUserId,
                                                    //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                    IsBookingResechdule = true,
                                                    IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false
                                                }).ToList();
                                    upcomingBookAppointmentOutput.Count = list.Count;
                                    if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                    {
                                        list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                    }
                                    upcomingBookAppointmentOutput.Items = list;
                                    upcomingBookAppointmentOutput.Message = "Get Doctor Missed Appointment successfully.";
                                    upcomingBookAppointmentOutput.StatusCode = 200;
                                }
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Items = outputItems.ToList();
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else if (roles.Contains("PATIENT"))
                        {
                            var doctorAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == input.Id && x.IsBooked == 1 && x.MissedAppointment == true);
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            where Convert.ToDateTime(aps.SlotZoneEndTime) <= DateTime.UtcNow
                                            orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                AppointmentDate = aps.AvailabilityDate,
                                                StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                TimeZone = aps.TimeZone,
                                                SlotStartTime = aps.SlotZoneTime,
                                                SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UniqueUserId,
                                                //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                IsBookingResechdule = true,
                                                IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Missed Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Items = outputItems.ToList();
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else if (roles.Contains("INSURANCE") || roles.Contains("MEDICALLEGAL"))
                        {
                            var doctorAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == input.Id && x.IsBooked == 1 && x.MissedAppointment == true);
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            where Convert.ToDateTime(aps.SlotZoneEndTime) <= DateTime.UtcNow
                                            orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                AppointmentDate = aps.AvailabilityDate,
                                                StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                TimeZone = aps.TimeZone,
                                                SlotStartTime = aps.SlotZoneTime,
                                                SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UniqueUserId,
                                                //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                IsBookingResechdule = true,
                                                RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType,
                                                IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Missed Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Items = outputItems.ToList();
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                        else
                        {
                            var doctorAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == input.Id && x.IsBooked == 1 && x.MissedAppointment == true);
                            if (doctorAppointment != null && doctorAppointment.Count > 0)
                            {
                                var list = (from da in doctorAppointment.AsEnumerable()
                                            join us in _userRepository.GetAll().AsEnumerable()
                                            on da.UserId equals us.UniqueUserId
                                            join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                            on da.AppointmentSlotId equals aps.Id
                                            join apsn in _userRepository.GetAll().AsEnumerable()
                                            on da.DoctorId equals apsn.UniqueUserId
                                            where Convert.ToDateTime(aps.SlotZoneEndTime) <= DateTime.UtcNow
                                            orderby aps.SlotZoneEndTime ascending
                                            select new UpcomingBookAppointmentDto
                                            {
                                                PatientName = us.FullName,
                                                DoctorName = apsn.FullName,
                                                AppointmentDate = aps.AvailabilityDate,
                                                StartTime = aps.AvailabilityStartTime,
                                                AppointmentId = da.Id,
                                                SlotId = da.AppointmentSlotId,
                                                DoctorId = da.DoctorId,
                                                TimeZone = aps.TimeZone,
                                                SlotStartTime = aps.SlotZoneTime,
                                                SlotEndTime = aps.SlotZoneEndTime,
                                                meetingId = da.meetingId,
                                                BookedBy = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType == "Insurance" ? "Legal" : us.UserType == "MedicalLegal" ? "Legal" : "",
                                                Title = da.Title,
                                                PatientId = us.UniqueUserId,
                                                //IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                                IsBookingResechdule = true,
                                                RoleName = us.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(us.UserType) ? "" : us.UserType,
                                                IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false
                                            }).ToList();
                                upcomingBookAppointmentOutput.Count = list.Count;
                                if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && list.Count > 0)
                                {
                                    list = list.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                                }
                                upcomingBookAppointmentOutput.Items = list;
                                upcomingBookAppointmentOutput.Message = "Get Doctor Missed Appointment successfully.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                            else
                            {
                                upcomingBookAppointmentOutput.Items = outputItems.ToList();
                                upcomingBookAppointmentOutput.Message = "No record found.";
                                upcomingBookAppointmentOutput.StatusCode = 200;
                            }
                        }
                    }
                    else
                    {
                        upcomingBookAppointmentOutput.Message = "No record found.";
                        upcomingBookAppointmentOutput.StatusCode = 401;
                    }
                    //}
                    //else
                    //{
                    //    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    //    upcomingBookAppointmentOutput.StatusCode = 401;
                    //}
                }
                else
                {
                    upcomingBookAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingBookAppointmentOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Missed Appointment Error" + ex.StackTrace);

                upcomingBookAppointmentOutput.Message = "Error while fetching missing appointment";
                upcomingBookAppointmentOutput.StatusCode = 401;
            }

            return upcomingBookAppointmentOutput;
        }

        public async Task<UpcomingPatientAppointmentOutput> GetUserAppointment(Guid Id)
        {
            UpcomingPatientAppointmentOutput upcomingPatientAppointmentOutput = new UpcomingPatientAppointmentOutput();
            var outputItems = new List<UpcomingPatientAppointmentDto>();
            Logger.Info("Get upcoming Patient Appointment for input: " + Id);

            int? TenantId = null;

            if (AbpSession.TenantId.HasValue)
            {
                TenantId = AbpSession.TenantId.Value;
            }
            try
            {
                if (AbpSession.UserId.HasValue)
                {
                    var patientAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == Id && x.IsBooked == 1);
                    if (patientAppointment != null && patientAppointment.Count > 0)
                    {
                        var list = (from pa in patientAppointment.AsEnumerable()
                                    join us in _userRepository.GetAll().AsEnumerable()
                                    on pa.DoctorId equals us.UniqueUserId
                                    join ub in _userRepository.GetAll().AsEnumerable()
                                    on pa.UserId equals ub.UniqueUserId
                                    join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                    on pa.AppointmentSlotId equals aps.Id

                                    join uc in _userConsentRepository.GetAll().AsEnumerable()
                                    on pa.Id equals uc.AppointmentId

                                    join upi in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                    on uc.UserConsentPatientsDetailsId equals upi.Id

                                    where Convert.ToDateTime(aps.SlotZoneEndTime) >= DateTime.UtcNow
                                    orderby aps.SlotZoneEndTime ascending
                                    select new UpcomingPatientAppointmentDto
                                    {
                                        DoctorName = us.FullName,
                                        UserName = upi.FirstName + " " + upi.LastName,
                                        AppointmentDate = aps.AvailabilityDate,
                                        StartTime = aps.AvailabilityStartTime,
                                        AppointmentId = pa.Id,
                                        UserId = us.UniqueUserId,
                                        SlotId = pa.AppointmentSlotId,
                                        DoctorId = pa.DoctorId,
                                        TimeZone = aps.TimeZone,
                                        SlotStartTime = aps.SlotZoneTime,
                                        SlotEndTime = aps.SlotZoneEndTime,
                                        meetingId = pa.meetingId,
                                        Title = pa.Title,
                                        IsBookingResechdule = (aps.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                        IsJoinMeeting = (aps.SlotZoneTime.Date == DateTime.UtcNow.Date && (aps.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (aps.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && aps.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                        PatientId = ub.UserType == "Patient" ? us.UniqueUserId : uc.PatientId == null ? Guid.Empty : (Guid)uc.PatientId,
                                        RoleName = ub.UserType == "Patient" ? UserType.Patient.ToString() : ub.UserType
                                    }).ToList();

                        upcomingPatientAppointmentOutput.Items = list;
                        upcomingPatientAppointmentOutput.Message = "Get Patient Booked Appointment successfully.";
                        upcomingPatientAppointmentOutput.StatusCode = 200;
                    }
                    else
                    {
                        upcomingPatientAppointmentOutput.Items = outputItems.ToList();
                        upcomingPatientAppointmentOutput.Message = "No record found.";
                        upcomingPatientAppointmentOutput.StatusCode = 200;
                    }
                }
                else
                {
                    upcomingPatientAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingPatientAppointmentOutput.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Get upcoming Patient Appointment Error " + ex.StackTrace);

                upcomingPatientAppointmentOutput.Message = "Error while fetching booking appointment";
                upcomingPatientAppointmentOutput.StatusCode = 401;
            }

            return upcomingPatientAppointmentOutput;
        }

        public async Task<BookAppointmentOutput> Cancel(CancelBookedAppointmentInput input)
        {
            BookAppointmentOutput bookAppointmentOutput = new BookAppointmentOutput();
            try
            {
                Logger.Info("Cancel Appointment for input: " + input.Id + "," + input.UserId);
                var stopwatch = Stopwatch.StartNew();
                var appointment = await _doctorAppointmentRepository.GetAsync(input.Id);
                var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);
                TimeSpan t = slot.SlotZoneTime.Subtract(DateTime.UtcNow);
                if (t.TotalHours >= 48)
                {
                    appointment.IsBooked = 2;
                    appointment.MissedAppointment = false;
                    appointment.UpdatedOn = DateTime.UtcNow;
                    appointment.UpdatedBy = input.UserId;
                    appointment.Status = "Cancel";
                    appointment.Reason = input.Reason;
                    await _doctorAppointmentRepository.UpdateAsync(appointment);


                    slot.IsBooked = 0;
                    await _doctorAppointmentSlotRepository.UpdateAsync(slot);

                    var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                    var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    bookAppointmentOutput.Id = input.Id;
                    bookAppointmentOutput.Message = "Appointment Cancel successfully";
                    bookAppointmentOutput.StatusCode = 200;

                    stopwatch.Stop();

                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(user);
                    createEventsInput.Operation = user.Title + " " + user.FullName + " has been cancelled an appointment with doctor " + doctor.FullName + 
                        " on " + slot.AvailabilityDate + " at " + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + slot.TimeZone + ")";
                    createEventsInput.Component = "Appointment";
                    createEventsInput.Action = "Cancel";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    var consultantReport = await _consultReportRepository.FirstOrDefaultAsync(x => x.AppointmentId == appointment.Id);
                    if (consultantReport != null)
                    {
                        consultantReport.IsActive = false;
                        consultantReport.UpdatedOn = DateTime.UtcNow;
                        consultantReport.UpdatedBy = input.UserId;
                        await _consultReportRepository.UpdateAsync(consultantReport);
                    }

                    DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));
                    DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));

                    string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                    string message = "Your appointment has been cancelled." + " <br /> " + "Reason :-" + input.Reason + " <br /><br /><br /> " + "Appointee Name: " + user.FullName
                                    + " <br /><br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                                    + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + slot.TimeZone + ")";
                    string doctorMailBody = _templateAppService.GetTemplates(Name, message,"");

                    string userName = user.Name.ToPascalCase() + " " + user.Surname.ToPascalCase();
                    string usermessage = "Your appointment with Doctor has been cancelled." + " <br /> " + "Reason :-" + input.Reason + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                                   + " <br /><br /><br /> " + "Appointment Date and Time: " + patientStartTime.ToString("MM/dd/yyyy") + " "
                                   + patientStartTime.ToString("hh:mm tt") + " to " + patientEndTime.ToString("hh:mm tt") + " (" + user.Timezone + ")";
                    string userMailBody = _templateAppService.GetTemplates(userName, usermessage,"");

                    //string doctorMailBody = " Hello " + "<b>" + doctor.Name + " " + doctor.Surname + ",</b> <br /><br /> " + "Your appointment has been cancelled." + " <br /> " + "Reason :-" + input.Reason + " <br /><br /><br /> " + "Appointee Name: " + user.FullName
                    //                + " <br /><br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                    //                + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                    //                + " <br /><br /><br /> " + "Regards," + " <br />" + "Emro Team";

                    //string userMailBody = " Hello " + "<b>" + user.Name + " " + user.Surname + ",</b> <br /><br /> " + "Your appointment with Doctor has been cancelled." + " <br /> " + "Reason :-" + input.Reason + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                    //               + " <br /><br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                    //               + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                    //               + " <br /><br /><br /> " + "Regards," + " <br />" + "Emro Team";

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Cancelled", doctorMailBody, adminmail));

                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, "Appointment Cancelled", userMailBody, adminmail));
                }
                else
                {
                    bookAppointmentOutput.Message = "You are not allowed to cancel your appointment within 48 hours of slot time.";
                    bookAppointmentOutput.StatusCode = 200;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Cancel Appointment Error" + ex.StackTrace);

                bookAppointmentOutput.Message = "Error while Cancel appointment";
                bookAppointmentOutput.StatusCode = 401;
            }
            return bookAppointmentOutput;
        }

        public async Task<GetAppoinmentDetailsOutput> GetAppoinmentDetailsAsync(GetBookAppointmentInput input)
        {
            GetAppoinmentDetailsOutput output = new GetAppoinmentDetailsOutput();
            try
            {
                if (input.AppointmentId != Guid.Empty && input.UserId != Guid.Empty)
                {
                    var appoinmentconsent = await _userConsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppointmentId);
                    var appoinment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId && (x.IsBooked == 1 || x.IsBooked == 3));
                    if (appoinment != null && appoinmentconsent != null)
                    {
                        if (appoinment.UserId == Guid.Parse(_session.UniqueUserId) || appoinment.DoctorId == Guid.Parse(_session.UniqueUserId) || appoinmentconsent.FamilyDoctorId == Guid.Parse(_session.UniqueUserId))
                        {
                            var consult = await _consultReportRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppointmentId && x.IsActive == true);
                            if (consult != null)
                            {
                                output.ConsultId = consult.Id;
                                output.IsArchived = consult.IsCompleted;
                            }
                            output.AppointmentId = appoinment.Id;
                            output.meetingId = appoinment.meetingId;
                            output.IsPayment = appoinment.IsBooked == 1 ? true : false;
                            var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinment.UserId);
                            var userMetaDetails = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == appoinment.UserId);
                            var patientConsentDetail = await _userConsentPatientsDetailsRepository.FirstOrDefaultAsync(x => x.Id == appoinmentconsent.UserConsentPatientsDetailsId);
                            if (patient != null && patient.UserType == "Patient")
                            {
                                output.FirstName = patient.Name;
                                output.LastName = patient.Surname;
                                output.EmailID = patient.EmailAddress;
                                output.Gender = patient.Gender == "0" ? "" : patient.Gender;
                                output.TelePhone = patient.PhoneNumber;
                                output.DateOfBirth = patient.DateOfBirth;
                                if (!string.IsNullOrEmpty(patient.Address))
                                {
                                    output.Address = patient.Address;
                                }
                                if (!string.IsNullOrEmpty(patient.City))
                                {
                                    output.Address = output.Address + " " + patient.City;
                                }
                                if (!string.IsNullOrEmpty(patient.State))
                                {
                                    output.Address = output.Address + " " + patient.State;
                                }

                                if (!string.IsNullOrEmpty(patient.PostalCode))
                                {
                                    output.Address = output.Address + " " + patient.PostalCode;
                                }

                                if (!string.IsNullOrEmpty(patient.Country))
                                {
                                    output.Address = output.Address + " " + patient.Country;
                                }
                                output.Id = Convert.ToString(patient.Id);
                                output.RoleName = patient.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(patient.UserType) ? "" : patient.UserType == "Insurance" ? "Legal" : patient.UserType == "MedicalLegal" ? "Legal" : "";
                                //output.City = patient.City;
                                //output.State = patient.State;
                                //output.Country = patient.Country;
                                //output.PostalCode = patient.PostalCode;
                                // output.PatientId = patient.UniqueUserId;
                                output.PatientId = patient.UserType == "Patient" ? patient.UniqueUserId : appoinmentconsent.PatientId == null ? Guid.Empty : (Guid)appoinmentconsent.PatientId;
                                // output.DucoumentUploadedId = appoinmentconsent.PatientId == null ? patient.UniqueUserId : (Guid)appoinmentconsent.PatientId;
                                if (!string.IsNullOrEmpty(patient.UploadProfilePicture))
                                {
                                    if (patient.IsBlobStorage)
                                    {
                                        output.ProfileUrl = _blobContainer.Download(patient.UploadProfilePicture);
                                    }
                                    else
                                    {
                                        output.ProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + patient.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                    }
                                }
                                //output.ProfileUrl = patient.UploadProfilePicture;
                            }
                            else if (patient.UserType == "Insurance" || patient.UserType == "MedicalLegal")
                            {
                                output.FirstName = patientConsentDetail.FirstName;
                                output.Id = patientConsentDetail.CaseId;
                                output.LastName = patientConsentDetail.LastName;
                                output.Gender = patientConsentDetail.Gender == "0" ? "" : patientConsentDetail.Gender;
                                output.DateOfBirth = patientConsentDetail.DateOfBirth != null ? Convert.ToDateTime(patientConsentDetail.DateOfBirth).ToShortDateString() : "";
                                if (!string.IsNullOrEmpty(patientConsentDetail.Address))
                                {
                                    output.Address = patientConsentDetail.Address;
                                }

                                if (!string.IsNullOrEmpty(patientConsentDetail.City))
                                {
                                    output.Address = output.Address + " " + patientConsentDetail.City;
                                }
                                if (!string.IsNullOrEmpty(patientConsentDetail.State))
                                {
                                    output.Address = output.Address + " " + patientConsentDetail.State;
                                }

                                if (!string.IsNullOrEmpty(patientConsentDetail.PostalCode))
                                {
                                    output.Address = output.Address + " " + patientConsentDetail.PostalCode;
                                }

                                if (!string.IsNullOrEmpty(patientConsentDetail.Country))
                                {
                                    output.Address = output.Address + " " + patientConsentDetail.Country;
                                }

                                output.LegalId = patient.UniqueUserId;
                                if (!string.IsNullOrEmpty(patient.UploadProfilePicture))
                                {
                                    if (patient.IsBlobStorage)
                                    {
                                        output.ProfileUrl = _blobContainer.Download(patient.UploadProfilePicture);
                                    }
                                    else
                                    {
                                        output.ProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + patient.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                    }
                                }
                                output.LegalName = patient.FullName;
                                output.LegalCompany = userMetaDetails.Company;
                            }
                            var family = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.UniqueUserId == input.UserId).FirstOrDefault();
                            if (family != null)
                            {
                                var roleIds = family.Roles.Select(x => x.RoleId).ToArray();

                                var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                                if (roles.Contains("CONSULTANT"))
                                {
                                    if (appoinmentconsent.FamilyDoctorId != Guid.Empty && appoinmentconsent.FamilyDoctorId != null)
                                    {
                                        var familyDoctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinmentconsent.FamilyDoctorId);
                                        if (familyDoctor != null)
                                        {
                                            output.DoctorFirstName = familyDoctor.Name;
                                            output.DoctorLastName = familyDoctor.Surname;
                                            output.DoctorTitle = familyDoctor.Title;
                                            output.DoctorId = familyDoctor.UniqueUserId;
                                            if (!string.IsNullOrEmpty(familyDoctor.UploadProfilePicture))
                                            {
                                                if (familyDoctor.IsBlobStorage)
                                                {
                                                    output.DoctorProfileUrl = _blobContainer.Download(familyDoctor.UploadProfilePicture);
                                                }
                                                else
                                                {
                                                    output.DoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + familyDoctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                                }
                                            }
                                            var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == familyDoctor.UniqueUserId).FirstOrDefault();
                                            if (metadetails != null)
                                            {
                                                output.DoctorSpecialty = metadetails.OncologySpecialty;
                                                output.HospitalName = metadetails.HospitalAffiliation;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        var usermetadetails = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == appoinment.UserId);
                                        if (usermetadetails != null)
                                        {
                                            if (usermetadetails.FamilyDoctorId != null && usermetadetails.FamilyDoctorId != Guid.Empty)
                                            {
                                                var familyDoctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == usermetadetails.FamilyDoctorId);
                                                if (familyDoctor != null)
                                                {
                                                    output.DoctorFirstName = familyDoctor.Name;
                                                    output.DoctorLastName = familyDoctor.Surname;
                                                    output.DoctorTitle = familyDoctor.Title;
                                                    output.DoctorId = familyDoctor.UniqueUserId;
                                                    if (!string.IsNullOrEmpty(familyDoctor.UploadProfilePicture))
                                                    {
                                                        if (familyDoctor.IsBlobStorage)
                                                        {
                                                            output.DoctorProfileUrl = _blobContainer.Download(familyDoctor.UploadProfilePicture);
                                                        }
                                                        else
                                                        {
                                                            output.DoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + familyDoctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                                        }
                                                    }
                                                    var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == familyDoctor.UniqueUserId).FirstOrDefault();
                                                    if (metadetails != null)
                                                    {
                                                        output.DoctorSpecialty = metadetails.OncologySpecialty;
                                                        output.HospitalName = metadetails.HospitalAffiliation;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (appoinment.DoctorId != null && appoinment.DoctorId != Guid.Empty)
                                    {
                                        var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinment.DoctorId);
                                        if (doctor != null)
                                        {
                                            output.DoctorFirstName = doctor.Name;
                                            output.DoctorLastName = doctor.Surname;
                                            output.DoctorTitle = doctor.Title;
                                            output.DoctorId = doctor.UniqueUserId;
                                            //output.DoctorProfileUrl = doctor.UploadProfilePicture;
                                            if (!string.IsNullOrEmpty(doctor.UploadProfilePicture))
                                            {
                                                if (doctor.IsBlobStorage)
                                                {
                                                    output.DoctorProfileUrl = _blobContainer.Download(doctor.UploadProfilePicture);
                                                }
                                                else
                                                {
                                                    output.DoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + doctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                                }
                                            }
                                            var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == doctor.UniqueUserId).FirstOrDefault();
                                            if (metadetails != null)
                                            {
                                                output.DoctorSpecialty = metadetails.OncologySpecialty;
                                                output.HospitalName = metadetails.HospitalAffiliation;
                                            }

                                        }
                                    }
                                }
                            }
                            output.Message = "Appoinment details get successfully.";
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
                Logger.Error("Get Appoinment DetailsError" + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        public async Task<GetAppoinmentDetailsOutput> GetInsuranceAppointmentDetails(GetBookAppointmentInput input)
        {
            GetAppoinmentDetailsOutput output = new GetAppoinmentDetailsOutput();
            try
            {
                if (input.AppointmentId != Guid.Empty)
                {
                    var appoinmentconsent = await _userConsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == input.AppointmentId);
                    var appoinment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId && (x.IsBooked == 1 || x.IsBooked == 3));
                    if (appoinment != null && appoinmentconsent != null)
                    {
                        output.AppointmentId = appoinment.Id;
                        output.meetingId = appoinment.meetingId;
                        var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinmentconsent.PatientId);
                        var patientConsentDetail = await _userConsentPatientsDetailsRepository.FirstOrDefaultAsync(x => x.Id == appoinmentconsent.UserConsentPatientsDetailsId);
                        if (patient != null)
                        {
                            output.FirstName = patient.Name;
                            output.Id = Convert.ToString(patient.Id);
                            output.LastName = patient.Surname;
                            output.EmailID = patient.EmailAddress;
                            output.Gender = patient.Gender == "0" ? "" : patient.Gender;
                            output.TelePhone = patient.PhoneNumber;
                            output.DateOfBirth = patient.DateOfBirth != null ? Convert.ToDateTime(patient.DateOfBirth).ToShortDateString() : "";
                            if (!string.IsNullOrEmpty(patient.Address))
                            {
                                output.Address = patient.Address;
                            }

                            if (!string.IsNullOrEmpty(patient.City))
                            {
                                output.Address = output.Address + " " + patient.City;
                            }
                            if (!string.IsNullOrEmpty(patient.State))
                            {
                                output.Address = output.Address + " " + patient.State;
                            }

                            if (!string.IsNullOrEmpty(patient.PostalCode))
                            {
                                output.Address = output.Address + " " + patient.PostalCode;
                            }

                            if (!string.IsNullOrEmpty(patient.Country))
                            {
                                output.Address = output.Address + " " + patient.Country;
                            }
                            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinment.UserId);
                            if (user != null)
                            {
                                output.RoleName = user.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(user.UserType) ? "" : user.UserType == "Insurance" ? "Legal" : user.UserType == "MedicalLegal" ? "Legal" : "";
                            }

                            //output.PatientId = appoinmentconsent.PatientId == null ? Guid.Empty : (Guid)appoinmentconsent.PatientId;
                            // output.DucoumentUploadedId = appoinmentconsent.PatientId == null ? Guid.Empty : (Guid)appoinmentconsent.PatientId;
                            output.PatientId = patient.UserType == "Patient" ? patient.UniqueUserId : appoinmentconsent.PatientId == null ? Guid.Empty : (Guid)appoinmentconsent.PatientId;
                            if (!string.IsNullOrEmpty(patient.UploadProfilePicture))
                            {
                                if (patient.IsBlobStorage)
                                {
                                    output.ProfileUrl = _blobContainer.Download(patient.UploadProfilePicture);
                                }
                                else
                                {
                                    output.ProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + patient.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                }
                            }
                        }
                        else
                        {
                            output.FirstName = patientConsentDetail.FirstName;
                            output.Id = patientConsentDetail.CaseId;
                            output.LastName = patientConsentDetail.LastName;
                            output.Gender = patientConsentDetail.Gender == "0" ? "" : patientConsentDetail.Gender;
                            output.DateOfBirth = patientConsentDetail.DateOfBirth != null ? Convert.ToDateTime(patientConsentDetail.DateOfBirth).ToShortDateString() : "";
                            if (!string.IsNullOrEmpty(patientConsentDetail.Address))
                            {
                                output.Address = patientConsentDetail.Address;
                            }

                            if (!string.IsNullOrEmpty(patientConsentDetail.City))
                            {
                                output.Address = output.Address + " " + patientConsentDetail.City;
                            }
                            if (!string.IsNullOrEmpty(patientConsentDetail.State))
                            {
                                output.Address = output.Address + " " + patientConsentDetail.State;
                            }

                            if (!string.IsNullOrEmpty(patientConsentDetail.PostalCode))
                            {
                                output.Address = output.Address + " " + patientConsentDetail.PostalCode;
                            }

                            if (!string.IsNullOrEmpty(patientConsentDetail.Country))
                            {
                                output.Address = output.Address + " " + patientConsentDetail.Country;
                            }
                        }
                        if (appoinment.DoctorId != null)
                        {
                            var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appoinment.DoctorId);
                            if (doctor != null)
                            {
                                output.DoctorFirstName = doctor.Name;
                                output.DoctorLastName = doctor.Surname;
                                output.DoctorTitle = doctor.Title;
                                output.DoctorId = doctor.UniqueUserId;
                                if (!string.IsNullOrEmpty(doctor.UploadProfilePicture))
                                {
                                    if (doctor.IsBlobStorage)
                                    {
                                        output.DoctorProfileUrl = _blobContainer.Download(doctor.UploadProfilePicture);
                                    }
                                    else
                                    {
                                        output.DoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + doctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                    }
                                }
                                var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == doctor.UniqueUserId).FirstOrDefault();
                                if (metadetails != null)
                                {
                                    output.DoctorSpecialty = metadetails.OncologySpecialty;
                                    output.HospitalName = metadetails.HospitalAffiliation;
                                }
                            }
                        }
                        output.Message = "Appoinment details get successfully.";
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
                Logger.Error("Get Appoinment DetailsError" + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        public async Task<GetPatientAppoinmentDetailsOutput> GetPatientAppointmentDetails(Guid UserId)
        {
            GetPatientAppoinmentDetailsOutput output = new GetPatientAppoinmentDetailsOutput();
            try
            {
                output.IsBookingResechdule = true;
                //if (input.AppointmentId > 0 && input.UserId > 0)
                //var result = (from da in appoinment
                //              orderby da.UpdatedOn descending
                //              select (da.Id, da.AppointmentSlotId, da.DoctorId, da.meetingId, da.UserId)).ToList();

                //Guid appointmentId = result.FirstOrDefault().Id;
                //Guid slotId = (Guid)result.FirstOrDefault().AppointmentSlotId;
                //Guid doctorId = result.FirstOrDefault().DoctorId;
                //string meetingID = result.FirstOrDefault().meetingId;

                if (UserId != Guid.Empty)
                {
                    var appoinment = _doctorAppointmentRepository.GetAll().Where(x => x.UserId == UserId && x.IsBooked == 1).OrderBy(x => x.UpdatedOn).ToList();

                    var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();

                    var query = (from a in appoinment.AsEnumerable()
                                 join slot in slots.AsEnumerable()
                                 on a.AppointmentSlotId equals slot.Id
                                 where slot.SlotZoneEndTime >= DateTime.UtcNow
                                 orderby slot.SlotZoneTime
                                 select new
                                 {
                                     a = a,
                                     slot = slot
                                 }).FirstOrDefault();

                    if (appoinment != null)
                    {
                        var list = await _userConsentRepository.FirstOrDefaultAsync(x => x.AppointmentId == query.a.Id);
                        var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == UserId);
                        var appSlot = await _doctorAppointmentSlotRepository.FirstOrDefaultAsync(x => x.Id == query.a.AppointmentSlotId);

                        if (list != null && appSlot.SlotZoneEndTime >= DateTime.UtcNow)
                        {
                            output.AppointmentId = query.a.Id;
                            output.meetingId = query.a.meetingId;
                            output.PatientId = query.a.UserId;
                            if (query.a.DoctorId != null)
                            {
                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == query.a.DoctorId);
                                if (doctor != null)
                                {

                                    output.SlotId = (Guid)query.a.AppointmentSlotId;
                                    output.DoctorId = query.a.DoctorId;
                                    output.UserName = user.FullName;
                                    output.StartTime = appSlot.AvailabilityStartTime;
                                    output.TimeZone = appSlot.TimeZone;
                                    output.AppointmentDate = appSlot.AvailabilityDate;
                                    output.SlotStartTime = appSlot.SlotZoneTime;
                                    output.SlotEndTime = appSlot.SlotZoneEndTime;
                                    output.DoctorFirstName = doctor.Name;
                                    output.DoctorLastName = doctor.Surname;
                                    output.DoctorTitle = doctor.Title;
                                    output.RoleName = user.UserType;

                                    //output.DoctorProfileUrl = doctor.UploadProfilePicture;
                                    if (!string.IsNullOrEmpty(doctor.UploadProfilePicture))
                                    {
                                        if (doctor.IsBlobStorage)
                                        {
                                            output.DoctorProfileUrl = _blobContainer.Download(doctor.UploadProfilePicture);
                                        }
                                        else
                                        {
                                            output.DoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + doctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                        }
                                    }
                                    var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == doctor.UniqueUserId).FirstOrDefault();
                                    if (metadetails != null)
                                    {
                                        output.DoctorSpecialty = metadetails.OncologySpecialty;
                                        output.HospitalName = metadetails.HospitalAffiliation;
                                    }
                                    // 2021-03-26 21:30:00
                                    TimeSpan t = appSlot.SlotZoneTime.Subtract(DateTime.UtcNow);
                                    if (t.TotalHours <= 48)
                                    {
                                        output.IsBookingResechdule = false;
                                    }
                                    if (t.Hours <= 1 && t.TotalMinutes <= 10 && appSlot.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow && appSlot.SlotZoneEndTime.Date == DateTime.UtcNow.Date)
                                    {
                                        output.IsJoinMeeting = true;
                                    }
                                    else
                                    {
                                        output.IsJoinMeeting = false;
                                    }

                                }
                            }
                            if (list.FamilyDoctorId != null && list.FamilyDoctorId != null)
                            {
                                var familyDoctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == list.FamilyDoctorId);
                                if (familyDoctor != null)
                                {
                                    output.FamilyDoctorFirstName = familyDoctor.Name;
                                    output.FamilyDoctorLastName = familyDoctor.Surname;
                                    output.FamilyDoctorTitle = familyDoctor.Title;
                                    output.FamilyDoctorId = familyDoctor.UniqueUserId;
                                    //output.FamilyDoctorProfileUrl = familyDoctor.UploadProfilePicture;
                                    if (!string.IsNullOrEmpty(familyDoctor.UploadProfilePicture))
                                    {
                                        if (familyDoctor.IsBlobStorage)
                                        {
                                            output.FamilyDoctorProfileUrl = _blobContainer.Download(familyDoctor.UploadProfilePicture);
                                        }
                                        else
                                        {
                                            output.FamilyDoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + familyDoctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                        }
                                    }
                                    var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == familyDoctor.UniqueUserId).FirstOrDefault();
                                    if (metadetails != null)
                                    {
                                        output.FamilyDoctorSpecialty = metadetails.OncologySpecialty;
                                        output.FamilyHospitalName = metadetails.HospitalAffiliation;
                                    }

                                }
                            }
                            else
                            {
                                var usermetadetails = await _userMetaDetailsRepository.FirstOrDefaultAsync(x => x.UserId == user.UniqueUserId);
                                if (usermetadetails != null)
                                {
                                    if (usermetadetails.FamilyDoctorId != null && usermetadetails.FamilyDoctorId != Guid.Empty)
                                    {
                                        var familyDoctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == usermetadetails.FamilyDoctorId);
                                        if (familyDoctor != null)
                                        {
                                            output.FamilyDoctorFirstName = familyDoctor.Name;
                                            output.FamilyDoctorLastName = familyDoctor.Surname;
                                            output.FamilyDoctorTitle = familyDoctor.Title;
                                            output.FamilyDoctorId = familyDoctor.UniqueUserId;
                                            //output.FamilyDoctorProfileUrl = familyDoctor.UploadProfilePicture;
                                            if (!string.IsNullOrEmpty(familyDoctor.UploadProfilePicture))
                                            {
                                                if (familyDoctor.IsBlobStorage)
                                                {
                                                    output.FamilyDoctorProfileUrl = _blobContainer.Download(familyDoctor.UploadProfilePicture);
                                                }
                                                else
                                                {
                                                    output.FamilyDoctorProfileUrl = Convert.ToBase64String(Utility.DecryptFile(_env.ContentRootPath + familyDoctor.UploadProfilePicture, _configuration[Utility.ENCRYPTION_KEY]));

                                                }
                                            }
                                            var metadetails = _userMetaDetailsRepository.GetAll().Where(x => x.UserId == familyDoctor.UniqueUserId).FirstOrDefault();
                                            if (metadetails != null)
                                            {
                                                output.FamilyDoctorSpecialty = metadetails.OncologySpecialty;
                                                output.FamilyHospitalName = metadetails.HospitalAffiliation;
                                            }
                                        }
                                    }
                                }
                            }

                            output.Message = "Appoinment details get successfully.";
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
                Logger.Error("Get Appoinment DetailsError" + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        public async Task<DashBoardOutbutDto> GetConsultantStats(Guid UserId)
        {
            DashBoardOutbutDto dashBoard = new DashBoardOutbutDto();
            try
            {
                if (UserId != Guid.Empty)
                {

                    //var patientAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == UserId && (x.IsBooked == 1 || x.IsBooked == 3));
                    var patientAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.DoctorId == UserId && (x.IsBooked == 1)); //TBC -- need to confirm with team
                    long newPatient = (from da in patientAppointment.AsEnumerable()
                                       join us in _userRepository.GetAll().AsEnumerable()
                                       on da.UserId equals us.UniqueUserId
                                       join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                       on da.AppointmentSlotId equals aps.Id
                                       join cr in _consultReportRepository.GetAll().AsEnumerable()
                                       on da.Id equals cr.AppointmentId
                                       where aps.SlotZoneTime >= DateTime.UtcNow
                                       && aps.SlotZoneEndTime >= DateTime.UtcNow
                                       && us.UserType == "Patient" && cr.IsActive == true
                                       select da.Id).GroupBy(x => x).Select(g => g.Key).Count();

                    long reportDue = (from c in patientAppointment.AsEnumerable()
                                      join uc in _consultReportRepository.GetAll().AsEnumerable()
                                      on c.Id equals uc.AppointmentId
                                      join u in _userRepository.GetAll().AsEnumerable()
                                      on c.UserId equals u.UniqueUserId
                                      join slot in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                       on c.AppointmentSlotId equals slot.Id
                                       into ps
                                      from p in ps.DefaultIfEmpty()
                                      where uc.IsActive == true && uc.IsCompleted == false
                                      orderby c.CreatedOn descending
                                      select new
                                      {
                                          PatientName = u.Name + " " + u.Surname,
                                          AppointmentDate = p == null ? "" : p.SlotZoneTime == null ? "" : Convert.ToDateTime(p.SlotZoneTime).ToString(),
                                          ConsultId = uc.Id,
                                          BookedBy = u.UserType == "EmroAdmin" ? "" : string.IsNullOrEmpty(u.UserType) ? "" : u.UserType == "Insurance" ? "Legal" : u.UserType == "MedicalLegal" ? "Legal" : "",
                                          AppointmentId = c.Id,
                                          IsBookLater = c.AppointmentSlotId == Guid.Empty ? true : false,
                                          DoctorId = c.DoctorId,
                                          IsPayment = c.IsBooked == 1 ? true : false
                                      }).GroupBy(x => x.AppointmentId).Select(x => x.FirstOrDefault()).Count();

                    long totalPatient = (from da in patientAppointment.AsEnumerable()
                                         join us in _userRepository.GetAll().AsEnumerable()
                                         on da.UserId equals us.UniqueUserId
                                         join cr in _consultReportRepository.GetAll().AsEnumerable()
                                         on da.Id equals cr.AppointmentId
                                         where
                                          us.UserType == "Patient"
                                          && cr.IsActive == true
                                         select da.UserId).GroupBy(x => x).Select(g => g.Key).Count();

                    dashBoard.NewPatient = newPatient;
                    dashBoard.ReportDue = reportDue;
                    dashBoard.TotalPatient = totalPatient;
                    dashBoard.Message = "Get record.";
                    dashBoard.StatusCode = 200;
                }
                else
                {
                    dashBoard.Message = "No record found.";
                    dashBoard.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Consultant Stats" + ex.StackTrace);
                dashBoard.Message = "Something went wrong, please try again.";
                dashBoard.StatusCode = 500;
            }
            return dashBoard;
        }

        public async Task<LegalDashBoardOutbutDto> GetLegalStats(Guid UserId)
        {
            LegalDashBoardOutbutDto dashBoard = new LegalDashBoardOutbutDto();
            try
            {
                if (UserId != Guid.Empty)
                {

                    var patientAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == UserId && (x.IsBooked == 1));
                    //long newPatient = (from da in patientAppointment.AsEnumerable()
                    //                   join us in _userRepository.GetAll().AsEnumerable()
                    //                   on da.UserId equals us.UniqueUserId
                    //                   join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                    //                   on da.AppointmentSlotId equals aps.Id
                    //                   where aps.SlotZoneTime >= DateTime.UtcNow
                    //                   && aps.SlotZoneEndTime >= DateTime.UtcNow
                    //                   && us.UserType == "Patient"
                    //                   select da).Count();

                    long newCases = (from da in patientAppointment.AsEnumerable()
                                      join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                      on da.AppointmentSlotId equals aps.Id
                                      join cr in _consultReportRepository.GetAll().AsEnumerable()
                                      on da.Id equals cr.AppointmentId
                                      where cr.IsCompleted == false && cr.IsActive == true && (aps.SlotZoneEndTime > DateTime.UtcNow || da.AppointmentSlotId == Guid.Empty)
                                     select da).Count();

                    long reportDue = (from da in patientAppointment.AsEnumerable()
                                      join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                      on da.AppointmentSlotId equals aps.Id
                                      join cr in _consultReportRepository.GetAll().AsEnumerable()
                                      on da.Id equals cr.AppointmentId
                                      where cr.IsCompleted == false && cr.IsActive == true
                                      select da).Count();

                    long totalPatient = (from da in patientAppointment.AsEnumerable()
                                         join us in _userRepository.GetAll().AsEnumerable()
                                         on da.UserId equals us.UniqueUserId
                                         join cr in _consultReportRepository.GetAll().AsEnumerable()
                                         on da.Id equals cr.AppointmentId
                                         where cr.IsActive == true
                                         select da).Count();

                    dashBoard.NewCases = newCases;
                    dashBoard.ReportDue = reportDue;
                    dashBoard.TotalCases = totalPatient;
                    dashBoard.Message = "Get record.";
                    dashBoard.StatusCode = 200;
                }
                else
                {
                    dashBoard.Message = "No record found.";
                    dashBoard.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Consultant Stats" + ex.StackTrace);
                dashBoard.Message = "Something went wrong, please try again.";
                dashBoard.StatusCode = 500;
            }
            return dashBoard;
        }

        public async Task<CreateBookAppointmentOutput> Update(UpdateBookInput input)
        {
            CreateBookAppointmentOutput output = new CreateBookAppointmentOutput();
            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            DoctorAppointmentSlot soltslist = new DoctorAppointmentSlot();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (input.AppointmentId != Guid.Empty && input.SlotId != Guid.Empty)
                {
                    var appointment = await _doctorAppointmentRepository.GetAsync(input.AppointmentId);
                    if (appointment != null)
                    {
                        //Create Audit log for before update
                        createEventsInput.BeforeValue = Utility.Serialize(appointment);

                        appointment.AppointmentSlotId = input.SlotId;
                        appointment.UpdatedOn = DateTime.UtcNow;
                        appointment.CreatedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            appointment.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _doctorAppointmentRepository.UpdateAsync(appointment);

                        if(appointment.AppointmentSlotId != Guid.Empty)
                        {
                            soltslist = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);
                            soltslist.IsBooked = 1;
                            soltslist.UpdatedOn = DateTime.UtcNow;
                            await _doctorAppointmentSlotRepository.UpdateAsync(soltslist);
                        }

                        var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                        var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);

                        var patientUserType = patientuser.UserType == "MedicalLegal" ? "Medical Legal" : patientuser.UserType;

                        string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                        var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);

                        DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));
                        DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));

                        stopwatch.Stop();
                        //Log Audit Events
                        JObject updatedAppointmentJsonObj = JObject.Parse(Utility.Serialize(appointment));
                        JObject updatedSlotDetailsJsonObj = JObject.Parse(Utility.Serialize(soltslist));

                        updatedAppointmentJsonObj.Merge(updatedSlotDetailsJsonObj, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Union
                        });
                        string AfterValueAuditDetails = updatedAppointmentJsonObj.ToString();

                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = AfterValueAuditDetails;
                        createEventsInput.Operation = patientuser.FullName + " has update an appoinment with doctor " + doctor.FullName +
                             " on " + slot.AvailabilityDate + " at " + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + slot.TimeZone + ")"; 
                        createEventsInput.Component = "Appointment";
                        createEventsInput.Action = "Update";
                        //createEventsInput.BeforeValue = "";
                        createEventsInput.AfterValue = AfterValueAuditDetails;
                        await _auditReportAppService.CreateAuditEvents(createEventsInput);

                        string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                        string message = "An appointment with a " + patientUserType.ToLower() + " was successfully booked. Please review and note the appointment details below: " + " <br /><br /><br /> " + patientUserType + "'s Name: " + patientuser.FullName
                                        + " <br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                                        + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + slot.TimeZone + ")" + " <br /><br />"
                                        + "Reminder messages will be sent 24-hour, 6-hour, 1-hour and 10 minutes before the meeting time."
                                        + " <br /><br /> Please click <a style='color:#fff;' href='" + _configuration["PORTAL_URL"] + "'>here</a> to login and see specific details.";
                        string doctorMailBody = _templateAppService.GetTemplates(Name, message,"");

                        string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                        string usermessage = "Your appointment has been booked successfully and here are the details -" + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                                      + " <br /><br /> " + "Appointment Date and Time: " + patientStartTime.ToString("MM/dd/yyyy") + " "
                                      + patientStartTime.ToString("hh:mm tt") + " to " + patientEndTime.ToString("hh:mm tt") + " (" + patientuser.Timezone + ")" + " <br /><br />"
                                      + "Link to join the meeting will be send 10 minutes before the meeting time.";
                        string patientMailBody = _templateAppService.GetTemplates(userName, usermessage,"");


                        //string doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "There is a patient wanted to book an appointment with you. Below are the appointment details -" + " <br /><br /><br /> " + "Patient Name: " + patientuser.FullName
                        //                + " <br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                        //                + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                        //                + "Link to join the meeting will be send 10 minutes before the meeting time."
                        //                //+ "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //                + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Your appointment has been booked successfully and here are the details -" + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                        //               + " <br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                        //               + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                        //               + "Link to join the meeting will be send 10 minutes before the meeting time."
                        //               //+ "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Confirmation", doctorMailBody, adminmail));

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Confirmation", patientMailBody, adminmail));

                        //Send Notification to patient
                        string Title = "Appointment Confirmation";
                        string userMessage = "Your appointment has been booked successfully with Dr "
                             + doctor.FullName + " on " + patientStartTime.ToString("MM/dd/yyyy") + " at " + patientStartTime.ToString("hh:mm tt")
                             + " (" + patientuser.Timezone + ")";
                        _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);

                        //Send Notification to Doctor
                        string drMessage = patientuser.FullName + " booked an appointment with you for "
                                             + slot.AvailabilityDate + " at " + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt")
                                             + " (" + slot.TimeZone + ")";
                        _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                        output.Message = "your appointment has been confirmed.";
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
                    output.Message = "Slot Id and Appointment Id is required";
                    output.StatusCode = 401;
                }
            }
            catch (Exception)
            {

                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        public async Task<UpcomingPatientAppointmentOutput> GetAllAppointment(Guid Id, Guid AppointmentId)
        {
            UpcomingPatientAppointmentOutput upcomingPatientAppointmentOutput = new UpcomingPatientAppointmentOutput();
            Logger.Info("Get upcoming Patient Appointment for input: " + Id);

            int? TenantId = null;

            if (AbpSession.TenantId.HasValue)
            {
                TenantId = AbpSession.TenantId.Value;
            }
            try
            {
                if (AbpSession.UserId.HasValue)
                {
                    var patientAppointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.UserId == Id && x.Id == AppointmentId && (x.IsBooked == 1 || x.IsBooked == 3));
                    if (patientAppointment != null && patientAppointment.Count > 0)
                    {
                        var list = (from pa in patientAppointment.AsEnumerable()
                                    join us in _userRepository.GetAll().AsEnumerable()
                                    on pa.DoctorId equals us.UniqueUserId
                                    join uc in _userConsentRepository.GetAll().AsEnumerable()
                                    on pa.Id equals uc.AppointmentId
                                    join upi in _userConsentPatientsDetailsRepository.GetAll().AsEnumerable()
                                    on uc.UserConsentPatientsDetailsId equals upi.Id
                                    join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                    on pa.AppointmentSlotId equals aps.Id
                                      into ps
                                    from p in ps.DefaultIfEmpty()
                                        //orderby aps.SlotZoneEndTime ascending
                                    select new UpcomingPatientAppointmentDto
                                    {
                                        DoctorName = us.FullName,
                                        UserName = upi.FirstName + " " + upi.LastName,
                                        AppointmentDate = p == null ? "" : p.AvailabilityDate == null ? "" : p.AvailabilityDate,
                                        StartTime = p == null ? "" : p.AvailabilityDate == null ? "" : p.AvailabilityStartTime,
                                        AppointmentId = pa.Id,
                                        UserId = us.UniqueUserId,
                                        SlotId = pa.AppointmentSlotId,
                                        DoctorId = pa.DoctorId,
                                        TimeZone = p == null ? "" : p.TimeZone == null ? "" : p.TimeZone,
                                        SlotStartTime = p?.SlotZoneTime,
                                        SlotEndTime = p?.SlotZoneEndTime,
                                        meetingId = pa.meetingId,
                                        Title = pa.Title,
                                        IsBookingResechdule = p == null ? true : p.SlotZoneTime == null ? true : (p.SlotZoneTime - DateTime.UtcNow).TotalHours <= 48 ? false : true,
                                        IsJoinMeeting = p == null ? true : p.SlotZoneTime == null ? false : (p.SlotZoneTime.Date == DateTime.UtcNow.Date && (p.SlotZoneTime - DateTime.UtcNow).Hours <= 1 && (p.SlotZoneTime - DateTime.UtcNow).TotalMinutes <= 10 && p.SlotZoneEndTime.AddMinutes(10) > DateTime.UtcNow) ? true : false,
                                        PatientId = us.UserType == "Patient" ? us.UniqueUserId : uc.PatientId == null ? Guid.Empty : (Guid)uc.PatientId,
                                    }).ToList();

                        upcomingPatientAppointmentOutput.Items = list;
                        upcomingPatientAppointmentOutput.Message = "Get Patient Booked Appointment successfully.";
                        upcomingPatientAppointmentOutput.StatusCode = 200;
                    }
                }
                else
                {
                    upcomingPatientAppointmentOutput.Message = "Please log in before attemping to fetch booking appointment";
                    upcomingPatientAppointmentOutput.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Get upcoming Patient Appointment Error " + ex.StackTrace);

                upcomingPatientAppointmentOutput.Message = "Error while fetching booking appointment";
                upcomingPatientAppointmentOutput.StatusCode = 401;
            }

            return upcomingPatientAppointmentOutput;
        }

        /// <summary>
        /// Get family doctor stats like (Total patient referred,Total due case and total case )
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<DashBoardOutbutDto> GetFamilyDoctorStats(Guid UserId)
        {
            DashBoardOutbutDto dashBoard = new DashBoardOutbutDto();
            try
            {
                if (UserId != Guid.Empty)
                {
                    var inviteUser = await _inviteRepository.GetAllListAsync(x => x.CreatedBy == UserId && x.UserType == "Patient");
                    var patientAppointment = await _userConsentRepository.GetAllListAsync(x => x.FamilyDoctorId == UserId);

                    //long newPatient = (from da in patientAppointment.AsEnumerable()
                    //                   join us in _userRepository.GetAll().AsEnumerable()
                    //                   on da.UserId equals us.UniqueUserId
                    //                   join aps in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                    //                   on da.AppointmentSlotId equals aps.Id
                    //                   join cr in _consultReportRepository.GetAll().AsEnumerable()
                    //                   on da.Id equals cr.AppointmentId
                    //                   where aps.SlotZoneTime >= DateTime.UtcNow
                    //                   && aps.SlotZoneEndTime >= DateTime.UtcNow
                    //                   && us.UserType == "Patient" && cr.IsActive == true
                    //                   select da.Id).GroupBy(x => x).Select(g => g.Key).Count();

                    long reportDue = (from pa in patientAppointment.AsEnumerable()
                                      join us in _userRepository.GetAll().AsEnumerable()
                                      on pa.UserId equals us.UniqueUserId
                                      join cr in _consultReportRepository.GetAll().AsEnumerable()
                                      on pa.AppointmentId equals cr.AppointmentId
                                      join da in _doctorAppointmentRepository.GetAll()
                                      on pa.AppointmentId equals da.Id
                                      //where cr.IsCompleted == false && cr.IsActive == true && (da.IsBooked == 1 || da.IsBooked == 3)
                                      where cr.IsCompleted == false && cr.IsActive == true && (da.IsBooked == 1) && us.UserType == "Patient" //TBC -- need to confirm with team
                                      select da.Id).GroupBy(x => x).Select(g => g.Key).Count();

                    long totalPatient = (from pa in patientAppointment.AsEnumerable()
                                         join us in _userRepository.GetAll().AsEnumerable()
                                         on pa.UserId equals us.UniqueUserId
                                         join cr in _consultReportRepository.GetAll().AsEnumerable()
                                         on pa.AppointmentId equals cr.AppointmentId
                                         join da in _doctorAppointmentRepository.GetAll()
                                         on pa.AppointmentId equals da.Id
                                         //where
                                         // us.UserType == "Patient"
                                         // && cr.IsActive == true && (da.IsBooked == 1 || da.IsBooked == 3)
                                         where
                                          us.UserType == "Patient"
                                          && cr.IsActive == true && da.IsBooked == 1  //TBC -- need to confirm with team
                                         select da.UserId).GroupBy(x => x).Select(g => g.Key).Count();

                    dashBoard.NewPatient = inviteUser.Count();
                    dashBoard.ReportDue = reportDue;
                    dashBoard.TotalPatient = totalPatient + dashBoard.NewPatient;
                    dashBoard.Message = "Get record.";
                    dashBoard.StatusCode = 200;
                }
                else
                {
                    dashBoard.Message = "No record found.";
                    dashBoard.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Consultant Stats" + ex.StackTrace);
                dashBoard.Message = "Something went wrong, please try again.";
                dashBoard.StatusCode = 500;
            }
            return dashBoard;
        }

    }
}
