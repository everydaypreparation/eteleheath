using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Users;
using EMRO.Common.CronJob.Dto;
using EMRO.Common.CustomNotification;
using EMRO.Common.Templates;
using EMRO.Email;
using EMRO.Master;
using EMRO.OncologyConsultReports;
using EMRO.UserConsents;
using EMRO.Users.Dto;
using EMRO.UsersMetaInfo;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.CronJob
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class CronJobAppService : ApplicationService, ICronJobAppService
    {
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DoctorAppointment, Guid> _doctorAppointmentRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IRepository<CronHistory, Guid> _cronHistoryRepository;
        private readonly IRepository<OncologyConsultReport, Guid> _consultReportRepository;
        private readonly IMailer _mailer;
        private IConfiguration _configuration;
        private readonly IRepository<UserConsent, Guid> _userconsentRepository;
        private readonly TemplateAppService _templateAppService;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        public CronJobAppService(
           UserManager userManager,
           IRepository<UserMetaDetails, Guid> userMetaDetailsRepository,
        IRepository<User, long> userRepository,
           IRepository<DoctorAppointment, Guid> doctorAppointmentRepository,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IRepository<CronHistory, Guid> cronHistoryRepository,
            IRepository<OncologyConsultReport, Guid> consultReportRepository,
            IMailer mailer,
            IConfiguration configuration
            , IRepository<UserConsent, Guid> userconsentRepository
            , TemplateAppService templateAppService
            //, INotificationPublisher notificationPublisher
            , ICustomNotificationAppService customNotificationAppService)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _cronHistoryRepository = cronHistoryRepository;
            _consultReportRepository = consultReportRepository;
            _mailer = mailer;
            _configuration = configuration;
            _userconsentRepository = userconsentRepository;
            _templateAppService = templateAppService;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
            _userMetaDetailsRepository = userMetaDetailsRepository;
        }

        /// <summary>
        /// This function is use to send the appointment notification  to the patient and Doctor
        /// </summary>
        /// <returns></returns>
        public async Task AppointmentNotification()
        {
            try
            {
                //bool IsSent24 = await HourNotification(24);
                //if (IsSent24)
                //{
                //    bool IsSent6 = await HourSixNotification(6);
                //    if (IsSent6)
                //    {
                //        bool IsSent1 = await HourOneNotification(1);
                //        if (IsSent1)
                //        {
                await MinuteNotification(10);
                //}
                //}
                //}

            }
            catch (Exception ex)
            {
                Logger.Error("AppointmentNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// This function is use to send the users (After 90 days) for deactivation account in every days
        /// </summary>
        /// <returns></returns>
        public async Task UserAccountNotification()
        {
            try
            {


                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var users = await _userManager.GetUsersInRoleAsync("PATIENT");
                if (users.Count > 0)
                {
                    var list = (from da in users.AsEnumerable()
                                where
                               da.LastModificationTime != null && DateTime.Now.Subtract(Convert.ToDateTime(da.LastModificationTime)).Days == 90
                                select da).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {

                            var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.Id == item.Id);
                            string Name = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                            string message = "This is a friendly reminder for account activation in the ETeleHealth cloud. if you want to close your account please contact ETeleHealth admin.";
                            string patientMailBody = _templateAppService.GetTemplates(Name, message,"");
                            //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> "
                            //    + "This is a friendly reminder for account activation in the EMRO cloud. if you want to close your account please contact Emro admin."
                            //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Account Activaion ", patientMailBody, adminmail));

                            var user = await _userRepository.GetAsync(item.Id);
                            if (user != null)
                            {
                                user.LastModificationTime = DateTime.Now;
                                await _userRepository.UpdateAsync(user);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UserAccountNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Send appointment notification to the user before 24 hours 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task HourNotification(int value)
        {
            try
            {


                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();
                var consult = await _consultReportRepository.GetAllListAsync();
                if (appointment.Count > 0)
                {
                    var list = (from da in appointment.AsEnumerable()
                                join aps in slots.AsEnumerable()
                                on da.AppointmentSlotId equals aps.Id
                                join cr in consult.AsEnumerable()
                                on da.Id equals cr.AppointmentId
                                where cr.IsCompleted == false
                                && aps.SlotZoneTime.Subtract(DateTime.UtcNow).Hours <= value && aps.SlotZoneTime.Subtract(DateTime.UtcNow).Hours >= 23
                                select new { da.DoctorId, da.UserId, aps.AvailabilityDate, aps.AvailabilityStartTime, aps.AvailabilityEndTime, aps.TimeZone, da.Title, da.Id }).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var mailsent = await _cronHistoryRepository.FirstOrDefaultAsync(x => x.AppointmentId == item.Id && x.ScheduleTime == value);
                            if (mailsent == null)
                            {
                                var request = new CronHistory
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    AppointmentId = item.Id,
                                    ScheduleTime = value
                                };

                                await _cronHistoryRepository.InsertAsync(request);

                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.DoctorId);
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.UserId);

                                string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                string message = "Just a friendly reminder that you have an upcoming appointment with " + patientuser.FullName + " in 24 hours.";
                                string doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");

                                string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string usermessage = "Just a friendly reminder that you have an upcoming appointment with " + doctor.FullName + " in 24 hours.";
                                string patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                //string doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + patientuser.FullName + " after 24 hours. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //                      //+ "Patient Name: " + patientuser.FullName
                                //                      //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //                      //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //                      + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + doctor.FullName + " after 24 hours. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //               //+ "Doctor Name: " + doctor.FullName
                                //               //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //               //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //               + "Please try to arrive 5 minutes early."
                                //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";


                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Booking " + value + " hours remaining ", doctorMailBody, adminmail));

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Booking " + value + " hours remaining ", patientMailBody, adminmail));

                                //Send Notification to Doctor
                                string Title = "Appointment Reminder - 24 hours before";
                                string drMessage = "Reminder - You have an appointment with " + patientuser.FullName + " after 24 hours.";
                                _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                                //Send Notification to User/Patient
                                string userMessage = "Reminder - You have an appointment with Dr " + doctor.FullName + " after 24 hours.";
                                _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("HourNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace + "Interval time " + value);
            }
        }

        /// <summary>
        /// Send appointment notification to the user before 10 hours
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task HourTenNotification(int value)
        {
            try
            {
                int ScheduleTimeValue = 600;
                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();
                var consult = await _consultReportRepository.GetAllListAsync();
                if (appointment.Count > 0)
                {
                    var list = (from da in appointment.AsEnumerable()
                                join aps in slots.AsEnumerable()
                                on da.AppointmentSlotId equals aps.Id
                                join cr in consult.AsEnumerable()
                                on da.Id equals cr.AppointmentId
                                where cr.IsCompleted == false
                                && aps.SlotZoneTime.Date == DateTime.UtcNow.Date && aps.SlotZoneTime.AddHours(-value) <= DateTime.UtcNow && aps.SlotZoneTime.Subtract(DateTime.UtcNow).Hours >= 9
                                select new { da.DoctorId, da.UserId, aps.AvailabilityDate, aps.AvailabilityStartTime, aps.AvailabilityEndTime, aps.TimeZone, da.Title, da.Id }).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var mailsent = await _cronHistoryRepository.FirstOrDefaultAsync(x => x.AppointmentId == item.Id && x.ScheduleTime == ScheduleTimeValue);
                            if (mailsent == null)
                            {
                                var request = new CronHistory
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    AppointmentId = item.Id,
                                    ScheduleTime = ScheduleTimeValue
                                };

                                await _cronHistoryRepository.InsertAsync(request);

                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.DoctorId);
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.UserId);

                                string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                string message = "Just a friendly reminder that you have an upcoming appointment with " + patientuser.FullName + " in 10 hours.";
                                string doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");

                                string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string usermessage = "Just a friendly reminder that you have an upcoming appointment with " + doctor.FullName + " in 10 hours.";
                                string patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Booking " + value + " hours remaining ", doctorMailBody, adminmail));

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Booking " + value + " hours remaining ", patientMailBody, adminmail));

                                //Send Notification to Doctor
                                string Title = "Appointment Reminder - 10 hours before";
                                string drMessage = "Reminder - You have an appointment with " + patientuser.FullName + " after 10 hours.";
                                _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                                //Send Notification to User/Patient
                                string userMessage = "Reminder - You have an appointment with Dr " + doctor.FullName + " after 10 hours.";
                                _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("HourTenNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace + "Interval time " + value);
            }
        }

        /// <summary>
        /// Send appointment notification to the user before 6 hours 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task HourSixNotification(int value)
        {
            try
            {

                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();
                var consult = await _consultReportRepository.GetAllListAsync();
                if (appointment.Count > 0)
                {
                    var list = (from da in appointment.AsEnumerable()
                                join aps in slots.AsEnumerable()
                                on da.AppointmentSlotId equals aps.Id
                                join cr in consult.AsEnumerable()
                                on da.Id equals cr.AppointmentId
                                where cr.IsCompleted == false
                                && aps.SlotZoneTime.Date == DateTime.UtcNow.Date && aps.SlotZoneTime.AddHours(-value) <= DateTime.UtcNow && aps.SlotZoneTime.Subtract(DateTime.UtcNow).Hours >= 5
                                select new { da.DoctorId, da.UserId, aps.AvailabilityDate, aps.AvailabilityStartTime, aps.AvailabilityEndTime, aps.TimeZone, da.Title, da.Id }).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var mailsent = await _cronHistoryRepository.FirstOrDefaultAsync(x => x.AppointmentId == item.Id && x.ScheduleTime == value);
                            if (mailsent == null)
                            {
                                var request = new CronHistory
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    AppointmentId = item.Id,
                                    ScheduleTime = value
                                };

                                await _cronHistoryRepository.InsertAsync(request);

                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.DoctorId);
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.UserId);

                                string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                string message = "Just a friendly reminder that you have an upcoming appointment with " + patientuser.FullName + " in 6 hours.";
                                string doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");

                                string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string usermessage = "Just a friendly reminder that you have an upcoming appointment with " + doctor.FullName + " in 6 hours.";
                                string patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                //string doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + patientuser.FullName + " after 6 hours. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //                      //+ "Patient Name: " + patientuser.FullName
                                //                      //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //                      //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //                      + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + doctor.FullName + " after 6 hours. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //               //+ "Doctor Name: " + doctor.FullName
                                //               //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //               //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //               + "Please try to arrive 5 minutes early."
                                //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";


                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Booking " + value + " hours remaining ", doctorMailBody, adminmail));

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Booking " + value + " hours remaining ", patientMailBody, adminmail));

                                //Send Notification to Doctor
                                string Title = "Appointment Reminder - 6 hours before";
                                string drMessage = "Reminder - You have an appointment with " + patientuser.FullName + " after 6 hours.";
                                _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                                //Send Notification to User/Patient
                                string userMessage = "Reminder - You have an appointment with Dr " + doctor.FullName + " after 6 hours.";
                                _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("HourSixNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace + "Interval time " + value);
            }
        }

        /// <summary>
        /// Send appointment notification to the user before 1 hours 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task HourOneNotification(int value)
        {
            try
            {

                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();
                var consult = await _consultReportRepository.GetAllListAsync();
                if (appointment.Count > 0)
                {
                    var list = (from da in appointment.AsEnumerable()
                                join aps in slots.AsEnumerable()
                                on da.AppointmentSlotId equals aps.Id
                                join cr in consult.AsEnumerable()
                                on da.Id equals cr.AppointmentId
                                where cr.IsCompleted == false
                                && aps.SlotZoneTime.Date == DateTime.UtcNow.Date && aps.SlotZoneTime.AddHours(-value) <= DateTime.UtcNow && aps.SlotZoneTime.Subtract(DateTime.UtcNow).Minutes >= 30
                                select new { da.DoctorId, da.UserId, aps.AvailabilityDate, aps.AvailabilityStartTime, aps.AvailabilityEndTime, aps.TimeZone, da.Title, da.Id }).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var mailsent = await _cronHistoryRepository.FirstOrDefaultAsync(x => x.AppointmentId == item.Id && x.ScheduleTime == value);
                            if (mailsent == null)
                            {
                                var request = new CronHistory
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    AppointmentId = item.Id,
                                    ScheduleTime = value
                                };

                                await _cronHistoryRepository.InsertAsync(request);

                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.DoctorId);
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.UserId);

                                string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                string message = "Just a friendly reminder that you have an upcoming appointment with " + patientuser.FullName + " in 1 hour.";
                                string doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");

                                string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string usermessage = "Just a friendly reminder that you have an upcoming appointment with " + doctor.FullName + " in 1 hour.";
                                string patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                //string doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + patientuser.FullName + " after 1 hour. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //                     //+ "Patient Name: " + patientuser.FullName
                                //                     //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //                     //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //                     + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + doctor.FullName + " after 1 hour. So ensure your availability and join on time." + " <br /><br /><br /> "

                                //               //+ "Doctor Name: " + doctor.FullName
                                //               //+ " <br /><br /> " + "Appointment Date and Time: " + item.AvailabilityDate + " "
                                //               //+ DateTime.Parse(item.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(item.AvailabilityEndTime).ToString("hh:mm tt") + "(" + item.TimeZone + ")" + " <br /><br />"
                                //               + "Please try to arrive 5 minutes early."
                                //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";


                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Booking " + value + " hours remaining ", doctorMailBody, adminmail));

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Booking " + value + " hours remaining ", patientMailBody, adminmail));

                                //Send Notification to Doctor
                                string Title = "Appointment Reminder - 1 hour before";
                                string drMessage = "Reminder - You have an appointment with " + patientuser.FullName + " after 1 hour.";
                                _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                                //Send Notification to User/Patient
                                string userMessage = "Reminder - You have an appointment with Dr " + doctor.FullName + " after 1 hour.";
                                _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("HourOneNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace + "Interval time " + value);
            }
        }

        /// <summary>
        /// Send appointment notification to the user before 10 minute 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected async Task MinuteNotification(int value)
        {
            try
            {

                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var appointment = await _doctorAppointmentRepository.GetAllListAsync(x => x.IsBooked == 1);
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync();
                var consult = await _consultReportRepository.GetAllListAsync();
                var userConsent = await _userconsentRepository.GetAllListAsync();
                if (appointment.Count > 0)
                {
                    var list = (from da in appointment.AsEnumerable()
                                join aps in slots.AsEnumerable()
                                on da.AppointmentSlotId equals aps.Id
                                join cr in consult.AsEnumerable()
                                on da.Id equals cr.AppointmentId
                                join uc in userConsent.AsEnumerable()
                                on da.Id equals uc.AppointmentId
                                where cr.IsCompleted == false
                                && aps.SlotZoneTime.Date == DateTime.UtcNow.Date && aps.SlotZoneTime.Subtract(DateTime.UtcNow).TotalMinutes <= value && aps.SlotZoneTime.Subtract(DateTime.UtcNow).TotalMinutes >= 1 && da.AppointmentSlotId != Guid.Empty
                                select new { da.DoctorId, da.UserId, aps.AvailabilityDate, aps.AvailabilityStartTime, aps.AvailabilityEndTime, aps.TimeZone, da.Title, da.Id, da.meetingId, uc.PatientId }).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            var mailsent = await _cronHistoryRepository.FirstOrDefaultAsync(x => x.AppointmentId == item.Id && x.ScheduleTime == value);
                            if (mailsent == null)
                            {
                                string doctorMailBody = string.Empty;
                                string patientMailBody = string.Empty;
                                var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.DoctorId);
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.UserId);
                                string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                string userName = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string message = string.Empty;
                                string usermessage = string.Empty;
                                if (patientuser.UserType != "Patient")
                                {

                                    message = "You have an upcoming appointment with " + patientuser.FullName + " in 10 minutes." + " <br /><br /> "
                                                                                  + "Click here to join <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + item.PatientId + "/" + patientuser.UserType + "'>link</a>";
                                    doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");


                                    usermessage = "You have an upcoming appointment with " + doctor.FullName + " in 10 minutes." + " <br /><br /> "
                                                 + "Click here to join <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + item.PatientId + "/" + patientuser.UserType + "'>link</a>";
                                                
                                    patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                    //doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + patientuser.FullName + " after 10 minutes. So ensure your availability and join on time." + " <br /><br /> "
                                    //                                               + "Please join here, <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + item.PatientId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                                    //                                               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                    //patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + doctor.FullName + " after 10 minutes. So ensure your availability and join on time." + " <br /><br /> "
                                    //              + "Please join here, <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + item.PatientId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                                    //              + "Please try to arrive 5 minutes early."
                                    //              + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                                }
                                else
                                {
                                    message = "You have an upcoming appointment with " + patientuser.FullName + " in 10 minutes." + " <br /><br /> "
                                                                                    + "Click here to join <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + patientuser.UniqueUserId + "/" + patientuser.UserType + "'>link</a>";
                                    doctorMailBody = _templateAppService.GetTemplates(Name, message, "ETeleHealth Team");


                                    usermessage = "You have an upcoming appointment with " + doctor.FullName + " in 10 minutes." + " <br /><br /> "
                                                  + "Click here to join <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/etelehealth/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + patientuser.UniqueUserId + "/" + patientuser.UserType + "'>link</a>";             
                                    patientMailBody = _templateAppService.GetTemplates(userName, usermessage, "ETeleHealth Team");

                                    //doctorMailBody = " Hello " + "<b>" + doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + patientuser.FullName + " after 10 minutes. So ensure your availability and join on time." + " <br /><br /> "
                                    //                                                + "Please join here, <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + patientuser.UniqueUserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                                    //                                                + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                    //patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Just a friendly reminder that you will have an upcoming appointment with " + doctor.FullName + " after 10 minutes. So ensure your availability and join on time." + " <br /><br /> "
                                    //              + "Please join here, <a style='color: #fff;' href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + item.Title + "/" + item.Id + "/" + item.meetingId + "/" + patientuser.UniqueUserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                                    //              + "Please try to arrive 5 minutes early."
                                    //              + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                                }
                                await _mailer.SendEmailAsync(doctor.EmailAddress, "Appointment Booking " + value + " minutes remaining ", doctorMailBody, adminmail);

                                await _mailer.SendEmailAsync(patientuser.EmailAddress, "Appointment Booking " + value + " minutes remaining ", patientMailBody, adminmail);

                                //Send Notification to Doctor
                                string Title = "Appointment Reminder - 10 minutes before";
                                string drMessage = "Reminder - You have an appointment with " + patientuser.FullName + " after 10 minutes.";
                                _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                                //Send Notification to User/Patient
                                string userMessage = "Reminder - You have an appointment with Dr " + doctor.FullName + " after 10 minutes.";
                                _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);

                                var request = new CronHistory
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    AppointmentId = item.Id,
                                    ScheduleTime = value
                                };

                                await _cronHistoryRepository.InsertAsync(request);
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("MinuteNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace + "Interval time " + value);
            }

        }

        /// <summary>
        /// Send deactivation notification to the user every year
        /// </summary>
        /// <returns></returns>
        public async Task UserAccountNotificationEveryYear()
        {
            try
            {


                var ulist = await _userManager.GetUsersInRoleAsync("ADMIN");
                string adminmail = ulist.FirstOrDefault().EmailAddress;
                var users = await _userManager.GetUsersInRoleAsync("PATIENT");
                if (users.Count > 0)
                {
                    var list = (from da in users.AsEnumerable()
                                where
                               da.LastModificationTime != null && DateTime.Now.Subtract(Convert.ToDateTime(da.LastModificationTime)).Days == 365
                               && da.IsActive == false
                                select da).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var item in list)
                        {

                            var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.Id == item.Id);

                            string Name = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                            string message = "This is a friendly reminder for account activation in the ETeleHealth cloud. if you want to close your account please contact ETeleHealth admin.";
                            string patientMailBody = _templateAppService.GetTemplates(Name, message,"");
                            //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> "
                            //    + "This is a friendly reminder for account activation in the EMRO cloud. if you want to close your account please contact Emro admin."
                            //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";
                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Account Activaion ", patientMailBody, adminmail));

                            var user = await _userRepository.GetAsync(item.Id);
                            if (user != null)
                            {
                                user.LastModificationTime = DateTime.Now;
                                await _userRepository.UpdateAsync(user);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UserAccountNotification Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Send reminder email if Doctor(Consultant) has <= 4 slots remaining.
        /// </summary>
        /// <returns></returns>
        public async Task SlotReminder()
        {
            try
            {
                var consultantUserList = await _userRepository.GetAllListAsync(x => x.UserType == "Consultant");
                var slots = await _doctorAppointmentSlotRepository.GetAllListAsync(x => x.SlotZoneTime > DateTime.UtcNow && x.IsBooked == 0);

                var list = (from c in consultantUserList.AsEnumerable()
                            join s in slots.AsEnumerable()
                            on c.UniqueUserId equals s.UserId into cs
                            from x in cs.DefaultIfEmpty()
                            select new
                            {
                                EmailAddress = c.EmailAddress,
                                UniqueUserId = c.UniqueUserId,
                                Id = c.Id
                            } into cg
                            group cg by cg.UniqueUserId into cl
                            select new
                            {
                                Id = cl.FirstOrDefault().Id,
                                EmailAddress = cl.FirstOrDefault().EmailAddress,
                                userId = cl.Key,
                                numberOfSlots = cl.Count()
                            }).Where(a => a.numberOfSlots <= 4).ToList();

                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == item.userId);

                        string Name = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                        string message = "This is a friendly reminder to update and add your availability on the ETeleHealth Portal. Your current availabilities may be running low or absent.";
                        string patientMailBody = _templateAppService.GetTemplates(Name, message,"");

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Reminder to update availability", patientMailBody, ""));

                        _customNotificationAppService.NotificationPublish(item.Id, "Reminder 4 or less slots remaining", message);

                        if (patientuser != null)
                        {
                            patientuser.LastModificationTime = DateTime.Now;
                            await _userRepository.UpdateAsync(patientuser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SlotReminder Error, Message: " + ex.Message);
                Logger.Error("SlotReminder Error, StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// New/Unused patient accounts can be removed after 14 days. 
        /// Give him 3 reminder emails to start using system else they have to restart the sign-up process. 
        /// Reminder should be on 8th day 10th day and 13th day, else on 14th day remove it.
        /// </summary>
        /// <returns></returns>
        public async Task UnusedPatient()
        {
            try
            {
                var patientUserList = await _userRepository.GetAllListAsync(x => x.UserType == "Patient" && x.IsActive == true);
                var appointmentList = await _doctorAppointmentRepository.GetAllListAsync();

                var list = patientUserList.Where(x => !appointmentList.Any(y => x.UniqueUserId == y.UserId)).ToList();

                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        var numberOfDays = (DateTime.Now - item.CreationTime).Days;

                        if (numberOfDays == 8 || numberOfDays == 10 || numberOfDays == 13 || numberOfDays >= 14)
                        {
                            if (numberOfDays >= 14)
                            {
                                var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == item.UniqueUserId).FirstOrDefault();
                                if (userMetaDetails != null)
                                {
                                    item.IsActive = false;
                                    userMetaDetails.IsActive = false;
                                    userMetaDetails.DeletedOn = DateTime.UtcNow;
                                    userMetaDetails.DeletedBy = item.UniqueUserId;
                                    await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                                    await _userManager.DeleteAsync(item);
                                    Logger.Info("User deleted successfully.");
                                }
                                else
                                {
                                    item.IsActive = false;
                                    await _userManager.DeleteAsync(item);
                                    Logger.Info("User deleted successfully.");
                                }
                            }
                            else
                            {
                                var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.Id == item.Id);
                                string Name = patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase();
                                string message = "This is a friendly reminder you have crossed " + numberOfDays + " days without any activity. " +
                                    "We will remove your record from the system in 14 days if we identify no activity in the account. " +
                                    "If you want to close your account please contact ETeleHealth admin.";
                                string patientMailBody = _templateAppService.GetTemplates(Name, message,"");

                                BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Account deactivation notification", patientMailBody, ""));

                                _customNotificationAppService.NotificationPublish(item.Id, "Account deactivation notification", message);

                                if (patientuser != null)
                                {
                                    patientuser.LastModificationTime = DateTime.Now;
                                    await _userRepository.UpdateAsync(patientuser);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UnusedPatient Error, Message: " + ex.Message);
                Logger.Error("UnusedPatient Error, StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// For Consultant and FamilyDoctor - just send them reminders weekly unless they done with their profile completion after registration.
        /// </summary>
        /// <returns></returns>
        public async Task IsConsultantProfileComplete()
        {
            try
            {
                var list = (from user in await _userRepository.GetAllListAsync(x => (x.UserType == "Consultant" || x.UserType == "FamilyDoctor") && x.IsActive == true)
                            join userMetaDetails in await _userMetaDetailsRepository.GetAllListAsync()
                            on user.UniqueUserId equals userMetaDetails.UserId
                            select new IsProfileCompleteDto
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Surname = user.Surname,
                                EmailAddress = user.EmailAddress,
                                UserType = user.UserType,
                                DateOfBirth = user.DateOfBirth,
                                Address = user.Address,
                                Gender = user.Gender,
                                PhoneNumber = user.PhoneNumber,
                                ExperienceOrTraining = userMetaDetails.ExperienceOrTraining,
                                Credentials = userMetaDetails.Credentials,
                                CurrentAffiliation = userMetaDetails.CurrentAffiliation,
                                LicensingNumber = userMetaDetails.LicensingNumber,
                                Certificate = userMetaDetails.Certificate,
                                MedicalAssociationMembership = userMetaDetails.MedicalAssociationMembership,
                                Fellowship = userMetaDetails.Fellowship,
                                UndergraduateMedicalTraining = userMetaDetails.UndergraduateMedicalTraining,
                                ProfessionalBio = userMetaDetails.ProfessionalBio,
                                Residency1 = userMetaDetails.Residency1,
                                Residency2 = userMetaDetails.Residency2,
                                DateConfirmed = userMetaDetails.DateConfirmed
                            }
                        ).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        string recommendedParams = string.Empty;
                        foreach (PropertyInfo pi in item.GetType().GetProperties())
                        {
                            if (pi.GetValue(item) == null || pi.GetValue(item).ToString() == "")
                            {
                                var attr = pi.GetCustomAttribute(typeof(DisplayNameAttribute), true);
                                if (attr == null)
                                {
                                    recommendedParams += string.Join(",", " - " + pi.Name + "<br />");
                                }
                                else
                                {
                                    recommendedParams += string.Join(",", " - " + (attr as DisplayNameAttribute).DisplayName + "<br />");
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(recommendedParams))
                        {
                            string name = item.Name.ToPascalCase() + " " + item.Surname.ToPascalCase();
                            string message = "This is a gentle reminder to complete your profile."
                                + "<br />" + "Please provide values in below attributes -"
                                + "<br />" + recommendedParams;
                            string mailBody = _templateAppService.GetTemplates(name, message,"");

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(item.EmailAddress, "Reminder - Complete your profile", mailBody, ""));

                            _customNotificationAppService.NotificationPublish(item.Id, "Reminder - Complete your profile", "This is a gentle reminder to complete your profile.");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("IsConsultantProfileComplete Error, Message: " + ex.Message);
                Logger.Error("IsConsultantProfileComplete Error, StackTrace: " + ex.StackTrace);
            }

        }

        /// <summary>
        /// For MedicalLegal, Insurance and Diagnostic - just send them reminders weekly unless they done with their profile completion after registration.
        /// </summary>
        /// <returns></returns>
        public async Task IsLegalProfileComplete()
        {
            try
            {
                var list = (from user in await _userRepository.GetAllListAsync(x => (x.UserType == "MedicalLegal" || x.UserType == "Insurance" || x.UserType == "Diagnostic") && x.IsActive == true)
                            join userMetaDetails in await _userMetaDetailsRepository.GetAllListAsync()
                            on user.UniqueUserId equals userMetaDetails.UserId
                            select new LegalProfileCompleteDto
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Surname = user.Surname,
                                EmailAddress = user.EmailAddress,
                                UserType = user.UserType,
                                DateOfBirth = user.DateOfBirth,
                                Address = user.Address,
                                Gender = user.Gender,
                                PhoneNumber = user.PhoneNumber,
                                Company = userMetaDetails.Company,
                                RequestedOncologySubspecialty = userMetaDetails.RequestedOncologySubspecialty
                            }
                        ).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        string recommendedParams = string.Empty;
                        foreach (PropertyInfo pi in item.GetType().GetProperties())
                        {
                            if (pi.GetValue(item) == null || pi.GetValue(item).ToString() == "")
                            {
                                if ((item.UserType.ToLower() == "Diagnostic".ToLower() && pi.Name != "Company") || item.UserType.ToLower() == "MedicalLegal".ToLower() || item.UserType.ToLower() == "Insurance".ToLower())
                                {
                                    if ((item.UserType.ToLower() == "Diagnostic".ToLower() && pi.Name != "RequestedOncologySubspecialty") || item.UserType.ToLower() == "MedicalLegal".ToLower() || item.UserType.ToLower() == "Insurance".ToLower())
                                    {
                                        var attr = pi.GetCustomAttribute(typeof(DisplayNameAttribute), true);
                                        if (attr == null)
                                        {
                                            recommendedParams += string.Join(",", " - " + pi.Name + "<br />");
                                        }
                                        else
                                        {
                                            recommendedParams += string.Join(",", " - " + (attr as DisplayNameAttribute).DisplayName + "<br />");
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(recommendedParams))
                        {
                            string name = item.Name.ToPascalCase() + " " + item.Surname.ToPascalCase();
                            string message = "This is a gentle reminder to complete your profile."
                                + "<br />" + "Please provide values in below attributes -"
                                + "<br />" + recommendedParams;
                            string mailBody = _templateAppService.GetTemplates(name, message,"");

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(item.EmailAddress, "Reminder - Complete your profile", mailBody, ""));

                            _customNotificationAppService.NotificationPublish(item.Id, "Reminder - Complete your profile", "This is a gentle reminder to complete your profile.");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("IsLegalProfileComplete Error, Message: " + ex.Message);
                Logger.Error("IsLegalProfileComplete Error, StackTrace: " + ex.StackTrace);
            }

        }
    }
}
