using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using EMRO.Appointment;
using EMRO.AppointmentSlot.Dto;
using EMRO.Authorization.Users;
using EMRO.Common.Templates;
using EMRO.Email;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeZoneNames;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common;

namespace EMRO.AppointmentSlot
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class AppointmentSlotAppService : ApplicationService, IAppointmentSlotAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IMailer _mailer;
        private IConfiguration _configuration;
        private readonly TemplateAppService _templateAppService;
        private readonly IAuditReportAppService _auditReportAppService;

        public AppointmentSlotAppService(IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IRepository<User, long> userRepository
            , IDoctorAppointmentRepository doctorAppointmentRepository,
            IAuditReportAppService auditReportAppService,
            IMailer mailer, IConfiguration configuration, TemplateAppService templateAppService)
        {
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _userRepository = userRepository;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _mailer = mailer;
            _configuration = configuration;
            _templateAppService = templateAppService;
            _auditReportAppService = auditReportAppService;
        }

        public async Task<AppointmentSlotOutput> Create(List<CreateAppointmentSlotInput> input)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input[0].UserId);

            AppointmentSlotOutput appointmentSlotOutput = new AppointmentSlotOutput();
            int duplicateCount = 0;
            int createdslots = 0;
            try
            {
                if (input.Count > 0)
                {
                    bool overlap = input.Any(r => input.Where(q => q != r).Any(
                        q => Convert.ToDateTime(q.AvailabilityDate + " " + q.AvailabilityEndTime) > Convert.ToDateTime(r.AvailabilityDate + " " + r.AvailabilityStartTime) 
                        && Convert.ToDateTime(q.AvailabilityDate + " " + q.AvailabilityStartTime) < Convert.ToDateTime(r.AvailabilityDate + " " + r.AvailabilityEndTime)));
                    if (overlap == false)
                    {
                        Logger.Info("Creating Appointment slot for input: " + input);
                        int? TenantId = null;

                        if (AbpSession.TenantId.HasValue)
                        {
                            TenantId = AbpSession.TenantId.Value;
                        }
                        for (int i = 0; i < input.Count; i++)
                        {

                            var stopwatch = Stopwatch.StartNew();

                            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(input[i].TimeZone);
                            DateTime newdate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityStartTime), est);
                            // check if slot already created start
                            var isSlotExist = await _doctorAppointmentSlotRepository.
                                GetAllListAsync(DoctorAppointmentSlot => DoctorAppointmentSlot.UserId == input[i].UserId
                                && DoctorAppointmentSlot.AvailabilityDate == input[i].AvailabilityDate
                                && DoctorAppointmentSlot.SlotZoneTime >= DateTime.UtcNow
                                //&& DoctorAppointmentSlot.AvailabilityStartTime == input[i].AvailabilityStartTime
                                //&& DoctorAppointmentSlot.AvailabilityEndTime == input[i].AvailabilityEndTime
                                //&& DoctorAppointmentSlot.TimeZone == input[i].TimeZone
                                );

                            isSlotExist = isSlotExist.Where(x => x.SlotZoneTime.Date == newdate.Date && Math.Abs(newdate.Subtract(x.SlotZoneTime).Hours) < 1 && Math.Abs(newdate.Subtract(x.SlotZoneTime).Minutes) < 60).ToList();
                            if (isSlotExist.Count > 0)
                            {
                                duplicateCount += 1;
                                if (input.Count == 1)
                                {
                                    appointmentSlotOutput.Message += "- " + Convert.ToDateTime(input[i].AvailabilityDate).ToString("MM/dd/yyyy") + " From :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityStartTime).ToString("HH:mm") + " To :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityEndTime).ToString("HH:mm") + " " + TZNames.GetAbbreviationsForTimeZone(input[i].TimeZone.ToString(), "en-US").Standard;
                                }
                                else
                                {
                                    appointmentSlotOutput.Message += duplicateCount == input.Count ? "- " + Convert.ToDateTime(input[i].AvailabilityDate).ToString("MM/dd/yyyy") + " From :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityStartTime).ToString("HH:mm") + " To :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityEndTime).ToString("HH:mm") + " " + TZNames.GetAbbreviationsForTimeZone(input[i].TimeZone, "en-US").Standard : "- " + Convert.ToDateTime(input[i].AvailabilityDate).ToString("MM/dd/yyyy") + " From :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityStartTime).ToString("HH:mm") + " To :-" + Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityEndTime).ToString("HH:mm") + " " + TZNames.GetAbbreviationsForTimeZone(input[i].TimeZone, "en-US").Standard + ", <br/>";
                                }
                            }
                            // check if slot already created end
                            else
                            {
                                //TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(input[i].TimeZone);
                                var doctorAppointmentSlot = new DoctorAppointmentSlot
                                {
                                    AvailabilityDate = input[i].AvailabilityDate,
                                    AvailabilityEndTime = input[i].AvailabilityEndTime,
                                    AvailabilityStartTime = input[i].AvailabilityStartTime,
                                    CreatedBy = input[i].CreatedBy,
                                    CreatedOn = DateTime.UtcNow,
                                    Flag = 1,
                                    TenantId = TenantId,
                                    TimeZone = input[i].TimeZone,
                                    UpdatedBy = input[i].UpdatedBy,
                                    UpdatedOn = DateTime.UtcNow,
                                    UserId = input[i].UserId, // Doctor ID
                                    IsBooked = 0,
                                    SlotZoneTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityStartTime), est),
                                    SlotZoneEndTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input[i].AvailabilityDate + " " + input[i].AvailabilityEndTime), est)
                                };
                                await _doctorAppointmentSlotRepository.InsertAndGetIdAsync(doctorAppointmentSlot);
                                createdslots += 1;

                                stopwatch.Stop();

                                //Log Audit Events
                                CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                                createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                                createEventsInput.Parameters = Utility.Serialize(doctorAppointmentSlot);
                                createEventsInput.Operation = user.Title + " " + user.FullName + " consultant has created slot from " + doctorAppointmentSlot.AvailabilityDate + " at " + DateTime.Parse(doctorAppointmentSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(doctorAppointmentSlot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + doctorAppointmentSlot.TimeZone + ")";
                                createEventsInput.Component = "Appointment Slot";
                                createEventsInput.Action = "Create";

                                await _auditReportAppService.CreateAuditEvents(createEventsInput);

                            }
                        }
                    }
                    else
                    {
                        appointmentSlotOutput.Message = "Please select slot without overlapping.";
                        appointmentSlotOutput.StatusCode = 401;
                    }
                }
                else
                {
                    appointmentSlotOutput.Message = "Bad request";
                    appointmentSlotOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                appointmentSlotOutput.Message = "Something went wrong, please try again.";
                appointmentSlotOutput.StatusCode = 401;
                Logger.Error("Create appointment slot" + ex.StackTrace);
            }
            if (createdslots == input.Count)
            {
                appointmentSlotOutput.Message = "Appointment slot created successfully.";
                appointmentSlotOutput.StatusCode = 200;
            }
            else if (duplicateCount == input.Count)
            {
                if (duplicateCount == 1)
                {
                    //appointmentSlotOutput.Message = "Below slot are overlapping with other slot, please select another time slot " + appointmentSlotOutput.Message.TrimEnd(',') + ".";
                    appointmentSlotOutput.Message = "Below slot(s) are overlapping with other slot(s), please select another time slot <br/>" + appointmentSlotOutput.Message.TrimEnd(',') + ".";
                }
                else
                {
                    appointmentSlotOutput.Message = "Below slot(s) are overlapping with other slot(s), please select another time slot(s) <br/>" + appointmentSlotOutput.Message.TrimEnd(',') + ".";
                }
                appointmentSlotOutput.StatusCode = 401;
            }
            else if (duplicateCount >= 1)
            {
                if (duplicateCount == 1)
                {
                    //appointmentSlotOutput.Message = "Below slot are overlapping with other slot, please select another time slot " + appointmentSlotOutput.Message.TrimEnd(',') + ".Rest other slot(s) are created successfully.";
                    appointmentSlotOutput.Message = "Below slot(s) are overlapping with other slot(s), please select another time slot(s) <br/>" + appointmentSlotOutput.Message.TrimEnd(',') + "Rest other slot(s) have been created successfully!";
                }
                else
                {
                    //appointmentSlotOutput.Message = "Below slot(s) are overlapping with other slot(s), please select another time slot(s) " + appointmentSlotOutput.Message.TrimEnd(',') + ".Rest other slot(s) are created successfully.";
                    appointmentSlotOutput.Message = "Below slot(s) are overlapping with other slot(s), please select another time slot(s) <br/>" + appointmentSlotOutput.Message.TrimEnd(',') + "Rest other slot(s) have been created successfully!";
                }

                appointmentSlotOutput.StatusCode = 206;
            }


            return appointmentSlotOutput;
        }

        public async Task<AppointmentSlotOutput> Update(UpdateAppointmentSlotInput input)
        {
            var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == input.UserId);
            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            var stopwatch = Stopwatch.StartNew();

            AppointmentSlotOutput appointmentSlotOutput = new AppointmentSlotOutput();
            try
            {
                int? TenantId = null;
                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }

                Logger.Info("Updating an appointment slot for input: " + input);

                TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(input.TimeZone);
                DateTime newdate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input.AvailabilityDate + " " + input.AvailabilityStartTime), est);

                // check if slot already created start
                var isSlotExist = await _doctorAppointmentSlotRepository.
                    GetAllListAsync(DoctorAppointmentSlot => DoctorAppointmentSlot.UserId == input.UserId
                    && DoctorAppointmentSlot.AvailabilityDate == input.AvailabilityDate
                    && DoctorAppointmentSlot.SlotZoneTime >= DateTime.UtcNow
                    //&& DoctorAppointmentSlot.AvailabilityStartTime == input.AvailabilityStartTime
                    //&& DoctorAppointmentSlot.AvailabilityEndTime == input.AvailabilityEndTime
                    //&& DoctorAppointmentSlot.TimeZone == input.TimeZone
                    && DoctorAppointmentSlot.Id != input.Id
                    );
                isSlotExist = isSlotExist.Where(x => x.SlotZoneTime.Date == newdate.Date && Math.Abs(newdate.Subtract(x.SlotZoneTime).Hours) < 1 && Math.Abs(newdate.Subtract(x.SlotZoneTime).Minutes) < 60).ToList();
                if (isSlotExist.Count > 0)
                {
                    appointmentSlotOutput.Message = "Appointment slot already exists";
                    appointmentSlotOutput.StatusCode = 401;
                }
                // check if slot already created end
                else
                {

                    var appointmentSlot = await _doctorAppointmentSlotRepository.GetAsync(input.Id);
                    createEventsInput.BeforeValue = Utility.Serialize(appointmentSlot);
                    //TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(input.TimeZone);
                    appointmentSlot.TimeZone = input.TimeZone;
                    appointmentSlot.UserId = input.UserId;
                    appointmentSlot.AvailabilityDate = input.AvailabilityDate;
                    appointmentSlot.AvailabilityEndTime = input.AvailabilityEndTime;
                    appointmentSlot.AvailabilityStartTime = input.AvailabilityStartTime;
                    appointmentSlot.TenantId = TenantId;
                    appointmentSlot.UpdatedBy = input.UpdatedBy;
                    appointmentSlot.UpdatedOn = DateTime.UtcNow;
                    appointmentSlot.SlotZoneTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input.AvailabilityDate + " " + input.AvailabilityStartTime), est);
                    appointmentSlot.SlotZoneEndTime = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(input.AvailabilityDate + " " + input.AvailabilityEndTime), est);

                    await _doctorAppointmentSlotRepository.UpdateAsync(appointmentSlot);

                    stopwatch.Stop();

                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(appointmentSlot);
                    createEventsInput.Operation = doctor.Title + " " + doctor.FullName + " consultant has updated slot, from " + appointmentSlot.AvailabilityDate + " at " + DateTime.Parse(appointmentSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(appointmentSlot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + appointmentSlot.TimeZone + ")";
                    createEventsInput.Component = "Appointment Slot";
                    createEventsInput.Action = "Update";
                    createEventsInput.AfterValue = Utility.Serialize(appointmentSlot);
                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.AppointmentSlotId == input.Id && x.IsBooked == 1);
                    if (appointment != null)
                    {
                        if (appointment.DoctorId == input.UserId)
                        {
                            string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                            // var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                            var user = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);

                            DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(appointmentSlot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));
                            DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(appointmentSlot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(user.Timezone));

                            string Name = user.Name.ToPascalCase() + " " + user.Surname.ToPascalCase();
                            string message = "Your appointment with Doctor has been rescheduled." + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                                   + " <br /><br /><br /> " + "Appointment Date and Time: " + patientStartTime.ToString("MM/dd/yyyy") + " "
                                   + patientStartTime.ToString("hh:mm tt") + " to " + patientEndTime.ToString("hh:mm tt") + " (" + user.Timezone + ")";
                            string userMailBody = _templateAppService.GetTemplates(Name, message,"");

                            //string userMailBody = " Hello " + "<b>" + user.Name + " " + user.Surname + ",</b> <br /><br /> " + "Your appointment with Doctor has been rescheduled." + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                            //       + " <br /><br /><br /> " + "Appointment Date and Time: " + appointmentSlot.AvailabilityDate + " "
                            //       + DateTime.Parse(appointmentSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(appointmentSlot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + appointmentSlot.TimeZone + ")" + " <br /><br />"
                            //       //+ "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + input.Id + "/" + appointment.meetingId + "/" + input.UserId + "/" + user.UserType + "'>Click to Join</a> <br /><br />"
                            //       + " <br /><br /><br /> " + "Regards," + " <br />" + "Emro Team";

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(user.EmailAddress, "ETeleHealth User Appointment", userMailBody, adminmail));
                        }
                    }
                    appointmentSlotOutput.Id = input.Id;
                    appointmentSlotOutput.Message = "Appointment slot updated successfully";
                    appointmentSlotOutput.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                appointmentSlotOutput.Message = "Error while updating appointment slot";
                appointmentSlotOutput.StatusCode = 401;
                Logger.Error("Update Appointment Error" + ex.StackTrace);
            }
            return appointmentSlotOutput;
        }

        public async Task<AppointmentSlotOutput> Delete(DeleteAppointmentSlotInput input)
        {

            var stopwatch = Stopwatch.StartNew();

            AppointmentSlotOutput appointmentSlotOutput = new AppointmentSlotOutput();
            try
            {
                var appointmentSlot = await _doctorAppointmentSlotRepository.GetAsync(input.Id);
                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointmentSlot.UserId);
                if (appointmentSlot != null)
                {

                    stopwatch.Stop();

                    //Log Audit Events
                    CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                    createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                    createEventsInput.Parameters = Utility.Serialize(appointmentSlot);
                    createEventsInput.Operation = doctor.Title + " " + doctor.FullName + " consultant has deleted slot, from " + appointmentSlot.AvailabilityDate + " at " + DateTime.Parse(appointmentSlot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(appointmentSlot.AvailabilityEndTime).ToString("hh:mm tt") + " (" + appointmentSlot.TimeZone + ")";
                    createEventsInput.Component = "Appointment Slot";
                    createEventsInput.Action = "Delete";

                    await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    await _doctorAppointmentSlotRepository.DeleteAsync(appointmentSlot);

                    appointmentSlotOutput.Message = "Appointment slot deleted successfully";
                    appointmentSlotOutput.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                appointmentSlotOutput.Message = "Error while deleting appointment slot";
                appointmentSlotOutput.StatusCode = 500;
                Logger.Error("Delete Appointment slot Error" + ex.StackTrace);
            }
            return appointmentSlotOutput;
        }

        public async Task<GetDoctorAppointmentSlot> GetAppointmentSlotbyId(Guid Id)
        {
            Logger.Info("Get Appointment slot for input: " + Id);
            int? TenantId = null;

            if (AbpSession.TenantId.HasValue)
            {
                TenantId = AbpSession.TenantId.Value;
            }

            GetDoctorAppointmentSlot getDoctorAppointmentSlot = new GetDoctorAppointmentSlot();
            try
            {
                var list = await _doctorAppointmentSlotRepository.GetAsync(Id);
                if (list != null)
                {
                    getDoctorAppointmentSlot.TimeZone = list.TimeZone;
                    getDoctorAppointmentSlot.AvailabilityDate = list.AvailabilityDate;
                    getDoctorAppointmentSlot.AvailabilityEndTime = list.AvailabilityEndTime;
                    getDoctorAppointmentSlot.AvailabilityStartTime = list.AvailabilityStartTime;
                    getDoctorAppointmentSlot.Id = list.Id;
                    getDoctorAppointmentSlot.Message = "Get Appointment slot successfully.";
                    getDoctorAppointmentSlot.StatusCode = 200;
                }
                else
                {
                    getDoctorAppointmentSlot.Message = "No record found.";
                    getDoctorAppointmentSlot.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                getDoctorAppointmentSlot.Message = "Error while getting appointment slot";
                getDoctorAppointmentSlot.StatusCode = 401;
                Logger.Error("Get Appointment slot Error" + ex.StackTrace);
            }
            return getDoctorAppointmentSlot;
        }

        public async Task<DoctorAppointmentSlotOutput> GetAllAppointmentSlotbyDoctorId(DoctorAppointmentSlotInputDto doctorAppointmentSlotInputDto)
        {
            Logger.Info("Get Doctor Appointment slot for input: " + doctorAppointmentSlotInputDto.UserId);

            DoctorAppointmentSlotOutput getDoctorAppointmentSlot = new DoctorAppointmentSlotOutput();
            try
            {
                var list = await _doctorAppointmentSlotRepository.GetAllListAsync(DoctorAppointmentSlot => DoctorAppointmentSlot.UserId == doctorAppointmentSlotInputDto.UserId && DoctorAppointmentSlot.SlotZoneTime >= DateTime.UtcNow);
                if (list != null && list.Count > 0)
                {
                    getDoctorAppointmentSlot.Items = (from l in list.AsEnumerable()
                                                      where Convert.ToDateTime(l.AvailabilityDate).Date >= DateTime.UtcNow.Date
                                                      orderby l.SlotZoneTime ascending
                                                      select new AppointmentSlotDto
                                                      {
                                                          TimeZone = l.TimeZone,
                                                          //AvailabilityDate = l.AvailabilityDate.ToString("MM/dd/yyyy"),
                                                          AvailabilityDate = l.AvailabilityDate,
                                                          AvailabilityStartTime = l.AvailabilityStartTime,
                                                          AvailabilityEndTime = l.AvailabilityEndTime,
                                                          UserId = l.UserId,
                                                          Id = l.Id,
                                                          IsBooked = l.IsBooked,
                                                          SlotStartTime = l.SlotZoneTime,
                                                          SlotEndTime = l.SlotZoneEndTime
                                                      }).ToList();

                    getDoctorAppointmentSlot.Count = getDoctorAppointmentSlot.Items.Count;
                    if (Convert.ToInt32(doctorAppointmentSlotInputDto.limit) > 0 && Convert.ToInt32(doctorAppointmentSlotInputDto.page) > 0 && getDoctorAppointmentSlot.Items.Count > 0)
                    {
                        getDoctorAppointmentSlot.Items = getDoctorAppointmentSlot.Items.Skip((Convert.ToInt32(doctorAppointmentSlotInputDto.page) - 1) * Convert.ToInt32(doctorAppointmentSlotInputDto.limit)).Take(Convert.ToInt32(doctorAppointmentSlotInputDto.limit)).ToList();
                    }

                    getDoctorAppointmentSlot.Message = "Get Doctor Appointment slot successfully.";
                    getDoctorAppointmentSlot.StatusCode = 200;
                }
                else
                {
                    getDoctorAppointmentSlot.Message = "No record found.";
                    getDoctorAppointmentSlot.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                getDoctorAppointmentSlot.Message = "Error while getting appointment slot";
                getDoctorAppointmentSlot.StatusCode = 401;
                Logger.Error("Get Doctor Appointment slot Error" + ex.StackTrace);
            }
            return getDoctorAppointmentSlot;
        }

        public async Task<DoctorAppointmentSlotOutput> GetAllUnbookedAppointmentSlotbyDoctorId(DoctorAppointmentSlotInputDto doctorAppointmentSlotInputDto)
        {
            Logger.Info("Get Doctor unbooked Appointment slot for input: " + doctorAppointmentSlotInputDto.UserId);

            DoctorAppointmentSlotOutput getDoctorAppointmentSlot = new DoctorAppointmentSlotOutput();
            try
            {
                var list = await _doctorAppointmentSlotRepository.GetAllListAsync(DoctorAppointmentSlot => DoctorAppointmentSlot.UserId == doctorAppointmentSlotInputDto.UserId && DoctorAppointmentSlot.IsBooked == 0 && DoctorAppointmentSlot.SlotZoneTime >= DateTime.UtcNow);
                if (list != null && list.Count > 0)
                {
                    getDoctorAppointmentSlot.Items = (from l in list.AsEnumerable()
                                                      where Convert.ToDateTime(l.AvailabilityDate).Date >= DateTime.UtcNow.Date
                                                      orderby l.SlotZoneTime ascending
                                                      select new AppointmentSlotDto
                                                      {
                                                          TimeZone = l.TimeZone,
                                                          //AvailabilityDate = l.AvailabilityDate.ToString("MM/dd/yyyy"),
                                                          AvailabilityDate = l.AvailabilityDate,
                                                          AvailabilityStartTime = l.AvailabilityStartTime,
                                                          AvailabilityEndTime = l.AvailabilityEndTime,
                                                          UserId = l.UserId,
                                                          Id = l.Id,
                                                          IsBooked = l.IsBooked,
                                                          SlotStartTime = l.SlotZoneTime,
                                                          SlotEndTime = l.SlotZoneEndTime
                                                      }).ToList();

                    getDoctorAppointmentSlot.Count = getDoctorAppointmentSlot.Items.Count;
                    if (Convert.ToInt32(doctorAppointmentSlotInputDto.limit) > 0 && Convert.ToInt32(doctorAppointmentSlotInputDto.page) > 0 && getDoctorAppointmentSlot.Items.Count > 0)
                    {
                        getDoctorAppointmentSlot.Items = getDoctorAppointmentSlot.Items.Skip((Convert.ToInt32(doctorAppointmentSlotInputDto.page) - 1) * Convert.ToInt32(doctorAppointmentSlotInputDto.limit)).Take(Convert.ToInt32(doctorAppointmentSlotInputDto.limit)).ToList();
                    }

                    getDoctorAppointmentSlot.Message = "Get Doctor Unbooked Appointment slot successfully.";
                    getDoctorAppointmentSlot.StatusCode = 200;
                }
                else
                {
                    getDoctorAppointmentSlot.Message = "No record found.";
                    getDoctorAppointmentSlot.StatusCode = 402;
                }
            }
            catch (Exception ex)
            {
                getDoctorAppointmentSlot.Message = "Error while getting unbooked appointment slot";
                getDoctorAppointmentSlot.StatusCode = 401;
                Logger.Error("Get Doctor unbooked Appointment slot Error" + ex.StackTrace);
            }
            return getDoctorAppointmentSlot;
        }

        public async Task<AppointmentSlotAvailableOutput> IsSlotAvailable(AvailableAppointmentSlotInput input)
        {
            AppointmentSlotAvailableOutput output = new AppointmentSlotAvailableOutput();
            try
            {
                if (input.AppointmentId != Guid.Empty)
                {
                    var appointment = await _doctorAppointmentRepository.GetAsync(input.AppointmentId);
                    if (appointment != null)
                    {

                        var slot = await _doctorAppointmentRepository.GetAllListAsync(x => x.AppointmentSlotId == appointment.AppointmentSlotId && x.IsBooked == 1);
                        if (appointment.IsBooked == 1)
                        {
                            output.IsAvailable = false;
                            output.Message = "slot is not available";
                            output.StatusCode = 200;
                        }
                        else if (slot.Count() != 0)
                        {
                            output.IsAvailable = false;
                            output.Message = "slot is not available";
                            output.StatusCode = 200;
                        }
                        else
                        {
                            var slottime = await _doctorAppointmentSlotRepository.FirstOrDefaultAsync(x => x.Id == appointment.AppointmentSlotId);
                            if (slottime != null)
                            {
                                if (slottime.SlotZoneTime >= DateTime.UtcNow)
                                {
                                    output.IsAvailable = true;
                                    output.Message = "This slot is available.";
                                    output.StatusCode = 200;
                                }
                                else
                                {
                                    output.IsAvailable = false;
                                    output.Message = "slot is not available";
                                    output.StatusCode = 200;
                                }
                            }
                            else
                            {
                                output.IsAvailable = false;
                                output.Message = "slot is not available";
                                output.StatusCode = 200;
                            }

                        }
                    }
                    else
                    {
                        output.IsAvailable = true;
                        output.Message = "This slot is available";
                        output.StatusCode = 200;
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
                output.Message = "Error while getting Available slot.";
                output.StatusCode = 500;
                Logger.Error("Is Slot Available Error" + ex.StackTrace);
            }
            return output;
        }
    }
}
