using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.CustomNotification;
using EMRO.Common.Templates;
using EMRO.Coupon;
using EMRO.Email;
using EMRO.Payment.Dto;
using EMRO.PayPal;
using EMRO.UserCoupon;
using EMRO.UsersMetaInfo;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;

namespace EMRO.Payment
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class PaymentAppService : ApplicationService, IPaymentAppService
    {
        private readonly StripeSetting _stripeSetting;
        private readonly IRepository<UserAppointmentPayment, Guid> _userAppointmentPayment;
        private readonly IRepository<UserCouponTransaction, Guid> _userCouponTransaction;
        private readonly IRepository<CouponMaster, Guid> _couponMasterRepository;
        private readonly IRepository<DoctorAppointment, Guid> _doctorappointmentRepository;
        private readonly IRepository<UserAppointmentPayPal, Guid> _userAppointmentPayPalRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IMailer _mailer;
        private IConfiguration _configuration;
        private readonly IRepository<UserMetaDetails, Guid> _userMetaDetailsRepository;
        private readonly TemplateAppService _templateAppService;
        //private readonly INotificationPublisher _notificationPublisher;
        private readonly ICustomNotificationAppService _customNotificationAppService;
        private readonly IAuditReportAppService _auditReportAppService;
        public PaymentAppService(IOptions<StripeSetting> stripeSetting,
            IRepository<UserAppointmentPayment, Guid> userAppointmentPayment,
            IRepository<UserCouponTransaction, Guid> userCouponTransaction,
            IRepository<CouponMaster, Guid> couponMasterRepository,
            IRepository<DoctorAppointment, Guid> doctorappointmentRepository,
            IRepository<UserAppointmentPayPal, Guid> userAppointmentPayPalRepository,
            IRepository<User, long> userRepository,
             IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IMailer mailer, IConfiguration configuration,
            IRepository<UserMetaDetails, Guid> userMetaDetailsRepository
            , TemplateAppService templateAppService,
            IAuditReportAppService auditReportAppService
            //, INotificationPublisher notificationPublisher
            ,ICustomNotificationAppService customNotificationAppService)
        {
            _stripeSetting = stripeSetting.Value;
            _userAppointmentPayment = userAppointmentPayment;
            _userCouponTransaction = userCouponTransaction;
            _couponMasterRepository = couponMasterRepository;
            _doctorappointmentRepository = doctorappointmentRepository;
            _userAppointmentPayPalRepository = userAppointmentPayPalRepository;
            _userRepository = userRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _mailer = mailer;
            _configuration = configuration;
            _userMetaDetailsRepository = userMetaDetailsRepository;
            _templateAppService = templateAppService;
            //_notificationPublisher = notificationPublisher;
            _customNotificationAppService = customNotificationAppService;
            _auditReportAppService = auditReportAppService;
        }

        public async Task<PaymentResultOutput> AppointmentPayment(StripeCardInput input)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.Info("Stripe Card Payment for AppointmentId: " + input.AppointmentId);
            PaymentResultOutput paymentResultOutput = new PaymentResultOutput();
            Guid CId = Guid.Empty;
            try
            {

                var appointment = await _doctorappointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId && (x.IsBooked == 0 || x.IsBooked == 3));
                if (appointment != null)
                {
                    // coupon check start
                    if (!string.IsNullOrEmpty(input.CouponId))
                    {
                        var result = await _couponMasterRepository.FirstOrDefaultAsync(x => x.IsActive == true && x.DiscountCode == input.CouponId);
                        if (result != null)
                        {
                            CId = result.Id;
                            var isCouponUsed = await _userCouponTransaction.GetAllListAsync(x => x.CouponId == result.Id && x.UserId == input.UserId);
                            if (isCouponUsed.Count > 0)
                            {
                                paymentResultOutput.Message = "Coupon already used";
                                paymentResultOutput.StatusCode = 402;
                                return paymentResultOutput;
                            }
                        }
                    }
                    bool isInternet = Utility.CheckInternetConnection();
                    if (isInternet)
                    {
                        if (input.PayAmount == 0)
                        {
                            PayByDepositeInput payinput = new PayByDepositeInput();
                            payinput.OriginalPayAmount = input.OriginalPayAmount;
                            payinput.PayAmount = input.PayAmount;
                            payinput.CouponId = input.CouponId;
                            payinput.AppointmentId = input.AppointmentId;
                            payinput.UserId = input.UserId;
                            payinput.Description = input.Description;
                            payinput.IsPatient = input.IsPatient;
                            var result = await PayByDepositeAmount(payinput);
                            paymentResultOutput.Message = result.Message;
                            paymentResultOutput.StatusCode = result.StatusCode;
                            paymentResultOutput.PaymentId = result.PaymentId;
                        }
                        else
                        {
                            ValidaToken validaToken = await ValidateCardDetails(input.NameOnCard, input.CardNumber, input.CVC, input.ExpirationMonth, input.ExpirationYear, input.PayAmount, input.Description);
                            if (validaToken.StatusCode == 200)
                            {
                                if (validaToken.charge.Paid)
                                {
                                    var paymentSuccess = new UserAppointmentPayment
                                    {
                                        AppointmentId = input.AppointmentId,
                                        //CardNumber = input.CardNumber.Substring(input.CardNumber.Length - 4),
                                        CardNumber = validaToken.stripeToken.Card.Last4,
                                        CardType = validaToken.stripeToken.Card.Brand,
                                        CardOrigin = validaToken.stripeToken.Card.Country,
                                        CreatedBy = input.UserId,
                                        CreatedOn = DateTime.UtcNow,
                                        Description = input.Description,

                                        ExpirationMonth = input.ExpirationMonth,
                                        ExpirationYear = input.ExpirationYear,
                                        //Fee = input.Fee,
                                        OriginalPayAmount = input.OriginalPayAmount,
                                        NameOnCard = input.NameOnCard,
                                        PayAmount = Convert.ToInt64(input.PayAmount),
                                        PaymentMessage = "Payment Successful",
                                        Status = "Paid",
                                        UpdatedBy = input.UserId,
                                        UpdatedOn = DateTime.UtcNow,
                                        UserId = input.UserId,
                                        PaymentMethod = 1 //Stripe
                                    };
                                    if (CId != Guid.Empty)
                                    {
                                        paymentSuccess.CouponId = CId;
                                    }
                                    var paymentId = await _userAppointmentPayment.InsertAndGetIdAsync(paymentSuccess);

                                    var userCoupon = new UserCouponTransaction
                                    {
                                        UserId = input.UserId,
                                        AppointmentId = input.AppointmentId,
                                        //CouponId = input.CouponId,
                                        CreatedBy = input.UserId,
                                        CreatedOn = DateTime.UtcNow,
                                        UpdatedBy = input.UserId,
                                        UpdatedOn = DateTime.UtcNow
                                    };
                                    if (CId != Guid.Empty)
                                    {
                                        userCoupon.CouponId = CId;
                                    }
                                    await _userCouponTransaction.InsertAsync(userCoupon);

                                    paymentResultOutput.Message = "Payment Successfull";
                                    paymentResultOutput.StatusCode = 200;
                                    paymentResultOutput.PaymentId = paymentId;
                                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                                    appointment.Status = "Confirm";
                                    appointment.IsBooked = 1;
                                    appointment.MissedAppointment = true;
                                    appointment.UpdatedOn = DateTime.UtcNow;
                                    await _doctorappointmentRepository.UpdateAsync(appointment);

                                   


                                    var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                                    var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);

                                    var patientUserType = patientuser.UserType == "MedicalLegal" ? "Medical Legal" : patientuser.UserType;

                                    if (appointment.AppointmentSlotId != Guid.Empty)
                                    {
                                        var soltslist = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);
                                        soltslist.IsBooked = 1;
                                        soltslist.UpdatedOn = DateTime.UtcNow;
                                        await _doctorAppointmentSlotRepository.UpdateAsync(soltslist);

                                        var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);

                                        DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));
                                        DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));

                                        string Name = doctor.Name.ToPascalCase() + " " + doctor.Surname.ToPascalCase();
                                        string message = "An appointment with a "+ patientUserType.ToLower() +" was successfully booked. Please review and note the appointment details below: " + " <br /><br /><br /> " + patientUserType +"'s Name: " + patientuser.FullName
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

                                        stopwatch.Stop();

                                        //Log Audit Events
                                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                                        createEventsInput.Parameters = Utility.Serialize(appointment);
                                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with stripe.";
                                        createEventsInput.Component = "Payment";
                                        createEventsInput.Action = "Stripe Payment";

                                        await _auditReportAppService.CreateAuditEvents(createEventsInput);

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
                                    }
                                    else if(appointment.AppointmentSlotId == Guid.Empty)
                                    {
                                        stopwatch.Stop();
                                        //Log Audit Events
                                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                                        createEventsInput.Parameters = Utility.Serialize(appointment);
                                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment(book later) with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with stripe.";
                                        createEventsInput.Component = "Payment";
                                        createEventsInput.Action = "Stripe Payment";
                                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                                    }
                                }
                                else
                                {
                                    var paymentFail = new UserAppointmentPayment
                                    {
                                        AppointmentId = input.AppointmentId,
                                        CardNumber = validaToken.stripeToken.Card.Last4,
                                        CardType = validaToken.stripeToken.Card.Brand,
                                        CardOrigin = validaToken.stripeToken.Card.Country,
                                        CreatedBy = input.UserId,
                                        CreatedOn = DateTime.UtcNow,
                                        Description = input.Description,
                                        //CouponId = input.CouponId,
                                        ExpirationMonth = input.ExpirationMonth,
                                        ExpirationYear = input.ExpirationYear,
                                        //Fee = input.Fee,
                                        OriginalPayAmount = input.OriginalPayAmount,
                                        NameOnCard = input.NameOnCard,
                                        PayAmount = Convert.ToInt64(input.PayAmount),
                                        PaymentMessage = validaToken.charge.FailureMessage,
                                        Status = "Payment Fail",
                                        UpdatedBy = input.UserId,
                                        UpdatedOn = DateTime.UtcNow,
                                        UserId = input.UserId,
                                        PaymentMethod = 1 //Stripe
                                    };
                                    if (CId != Guid.Empty)
                                    {
                                        paymentFail.CouponId = CId;
                                    }
                                    var paymentId = await _userAppointmentPayment.InsertAndGetIdAsync(paymentFail);
                                    paymentResultOutput.Message = "Payment Failure";
                                    paymentResultOutput.StatusCode = 401;
                                    paymentResultOutput.PaymentId = paymentId;
                                }
                            }
                            else
                            {
                                paymentResultOutput.Message = validaToken.Message;
                                paymentResultOutput.StatusCode = 401;
                            }

                        }
                    }
                    else
                    {
                        paymentResultOutput.Message = "Please check your internet connection or try again later.";
                        paymentResultOutput.StatusCode = 401;
                    }
                }
                else
                {
                    paymentResultOutput.Message = "Payment already done.";
                    paymentResultOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Stripe Card Payment Error" + ex.StackTrace);
                paymentResultOutput.Message = "Something went wrong, please try again.";
                paymentResultOutput.StatusCode = 500;
            }
            return paymentResultOutput;
        }

        public async Task<PaymentDetailOutput> PaymentDetails(PaymentDetails input)
        {
            Logger.Info("Get Payment details for paymentId: " + input.PaymentId);
            PaymentDetailOutput paymentDetailOutput = new PaymentDetailOutput();
            try
            {
                var paymentDetails = await _userAppointmentPayment.GetAsync(input.PaymentId);

                if (paymentDetails != null)
                {
                    paymentDetailOutput.AppointmentId = paymentDetails.AppointmentId;
                    paymentDetailOutput.CardNumber = paymentDetails.CardNumber;
                    paymentDetailOutput.CardOrigin = paymentDetails.CardOrigin;
                    paymentDetailOutput.CardType = paymentDetails.CardType;
                    paymentDetailOutput.CouponId = paymentDetails.CouponId;
                    paymentDetailOutput.CreatedOn = paymentDetails.CreatedOn;
                    paymentDetailOutput.Description = paymentDetails.Description;
                    paymentDetailOutput.ExpirationMonth = paymentDetails.ExpirationMonth;
                    paymentDetailOutput.ExpirationYear = paymentDetails.ExpirationYear;
                    paymentDetailOutput.NameOnCard = paymentDetails.NameOnCard;
                    paymentDetailOutput.OriginalPayAmount = paymentDetails.OriginalPayAmount;
                    paymentDetailOutput.PayAmount = paymentDetails.PayAmount;
                    paymentDetailOutput.Status = paymentDetails.Status;
                    paymentDetailOutput.UserId = paymentDetails.UserId;
                    paymentDetailOutput.StatusCode = 200;
                    paymentDetailOutput.Message = "Get payment details";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error for getting Payment details for paymentId " + input.PaymentId + " - " + ex.StackTrace);
            }
            return paymentDetailOutput;
        }

        public async Task<PaymentResultOutput> PaymentByPaypal(PayPalInput input)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.Info("PayPal Payment for AppointmentId: " + input.AppointmentId);
            PaymentResultOutput paymentResultOutput = new PaymentResultOutput();
            Guid CId = Guid.Empty;
            try
            {
                var appointment = await _doctorappointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId && (x.IsBooked == 0 || x.IsBooked == 3));
                if (appointment != null)
                {
                    // coupon check start
                    if (!string.IsNullOrEmpty(input.CouponId))
                    {
                        var result = await _couponMasterRepository.FirstOrDefaultAsync(x => x.IsActive == true && x.DiscountCode == input.CouponId);
                        if (result != null)
                        {
                            CId = result.Id;
                            var isCouponUsed = await _userCouponTransaction.GetAllListAsync(x => x.CouponId == result.Id && x.UserId == input.UserId);
                            if (isCouponUsed.Count > 0)
                            {
                                paymentResultOutput.Message = "Coupon already used";
                                paymentResultOutput.StatusCode = 402;
                                return paymentResultOutput;
                            }
                        }
                    }

                    var paymentSuccess = new UserAppointmentPayment
                    {
                        AppointmentId = input.AppointmentId,
                        CreatedBy = input.UserId,
                        CreatedOn = DateTime.UtcNow,
                        OriginalPayAmount = input.OriginalPayAmount,
                        PayAmount = input.OriginalPayAmount,
                        PayeeEmailAddress = input.PayeeEmailAddress,
                        PayeeMerchantId = input.PayeeMerchantId,
                        PayerAddressLine = input.PayerAddressLine,
                        PayerAdminArea1 = input.PayerAdminArea1,
                        PayerAdminArea2 = input.PayerAdminArea2,
                        PayerCountryCode = input.PayerCountryCode,
                        PayerEmailAddress = input.PayerEmailAddress,
                        PayerFullName = input.PayerFullName,
                        PayerId = input.PayerId,
                        PayerPostalCode = input.PayerPostalCode,
                        PaymentCreateTime = input.PaymentCreateTime,
                        PaymentCurrencyCode = input.PaymentCurrencyCode,
                        PaymentOrderId = input.PaymentOrderId,
                        PaymentStatus = input.PaymentStatus,
                        PaymentUpdateTime = input.PaymentUpdateTime,
                        UserId = input.UserId,
                        PaymentMethod = 2 //paypal
                    };
                    if (CId != Guid.Empty)
                    {
                        paymentSuccess.CouponId = CId;
                    }
                    var paymentId = await _userAppointmentPayment.InsertAndGetIdAsync(paymentSuccess);

                    var userCoupon = new UserCouponTransaction
                    {
                        UserId = input.UserId,
                        AppointmentId = input.AppointmentId,
                        //CouponId = input.CouponId,
                        CreatedBy = input.UserId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = input.UserId,
                        UpdatedOn = DateTime.UtcNow
                    };
                    if (CId != Guid.Empty)
                    {
                        userCoupon.CouponId = CId;
                    }
                    await _userCouponTransaction.InsertAsync(userCoupon);


                    paymentResultOutput.StatusCode = 200;
                    paymentResultOutput.PaymentId = paymentId;

                    //if (input.IsPatient)
                    //{
                    //    appointment.IsBooked = 3;
                    //    paymentResultOutput.Message = "Payment Successfull";
                    //}
                    //else
                    //{
                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    appointment.Status = "Confirm";
                    appointment.IsBooked = 1;
                    appointment.MissedAppointment = true;
                    appointment.UpdatedOn = DateTime.UtcNow;
                    await _doctorappointmentRepository.UpdateAsync(appointment);
                   

                    //var doctor = _userRepository.GetAll().Where(x => x.Id == doctorAppointment.DoctorId).FirstOrDefault();
                    //var patient = _userRepository.GetAll().Where(x => x.Id == doctorAppointment.PatientId).FirstOrDefault();
                    //var slot = _doctorAppointmentSlotRepository.GetAll().Where(x => x.Id == doctorAppointment.AppointmentSlotId).FirstOrDefault();

                    var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);
                    var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);

                    var patientUserType = patientuser.UserType == "MedicalLegal" ? "Medical Legal" : patientuser.UserType;

                    if (appointment.AppointmentSlotId != Guid.Empty)
                    {
                        var soltslist = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);
                        soltslist.IsBooked = 1;
                        soltslist.UpdatedOn = DateTime.UtcNow;
                        await _doctorAppointmentSlotRepository.UpdateAsync(soltslist);
                        var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);

                        DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));
                        DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));

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
                        //                // + "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //                + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Your appointment has been booked successfully and here are the details -" + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                        //               + " <br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                        //               + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                        //               + "Link to join the meeting will be send 10 minutes before the meeting time."
                        //               // + "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Confirmation", doctorMailBody, adminmail));

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Confirmation", patientMailBody, adminmail));

                        string Title = "Appointment Confirmation";
                        string userMessage = "Your appointment has been booked successfully with Dr "
                             + doctor.FullName + " on " + patientStartTime.ToString("MM/dd/yyyy") + " at " + patientStartTime.ToString("hh:mm tt")
                             + " (" + patientuser.Timezone + ")";
                        _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);

                        string drMessage = patientuser.FullName + " booked an appointment with you for "
                             + slot.AvailabilityDate + " at " + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt")
                             + " (" + slot.TimeZone + ")";
                        _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                        stopwatch.Stop();

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(appointment);
                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with paypal.";
                        createEventsInput.Component = "Payment";
                        createEventsInput.Action = "Paypal Payment";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    }
                    else if(appointment.AppointmentSlotId == Guid.Empty)
                    {
                        stopwatch.Stop();
                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(appointment);
                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment(book later) with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with paypal.";
                        createEventsInput.Component = "Payment";
                        createEventsInput.Action = "Paypal Payment";
                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                    }

                    paymentResultOutput.Message = "Your payment and appointment has been done successfull.";
                    //}


                }
                else
                {
                    paymentResultOutput.Message = "Payment already done.";
                    paymentResultOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("PayPal Payment Error" + ex.StackTrace);
                paymentResultOutput.Message = "Payment Error";
                paymentResultOutput.StatusCode = 500;
            }

            return paymentResultOutput;
        }

        public async Task<PayPalDetailOutput> PayPalDetail(PaymentDetails input)
        {
            Logger.Info("Get Paypal Payment details for paymentId: " + input.PaymentId);
            PayPalDetailOutput paymentDetailOutput = new PayPalDetailOutput();
            try
            {
                var paymentDetails = await _userAppointmentPayment.GetAsync(input.PaymentId);

                if (paymentDetails != null)
                {
                    paymentDetailOutput.AppointmentId = paymentDetails.AppointmentId;
                    paymentDetailOutput.CouponId = paymentDetails.CouponId;
                    paymentDetailOutput.PayeeEmailAddress = paymentDetails.PayeeEmailAddress;
                    paymentDetailOutput.PayAmount = paymentDetails.PayAmount;
                    paymentDetailOutput.PaymentOrderId = paymentDetails.PaymentOrderId;
                    paymentDetailOutput.PaymentStatus = paymentDetails.PaymentStatus;
                    paymentDetailOutput.UserId = paymentDetails.UserId;
                    paymentDetailOutput.PaymentCurrencyCode = paymentDetails.PaymentCurrencyCode;
                    paymentDetailOutput.StatusCode = 200;
                    paymentDetailOutput.Message = "Get Paypal payment details";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error for getting Paypal Payment details for paymentId " + input.PaymentId + " - " + ex.StackTrace);
            }
            return paymentDetailOutput;
        }

        public async Task<PaymentResultOutput> PayByDepositeAmount(PayByDepositeInput input)
        {

            var stopwatch = Stopwatch.StartNew();

            Logger.Info("Pay By DepositeAmount for AppointmentId: " + input.AppointmentId);
            PaymentResultOutput paymentResultOutput = new PaymentResultOutput();
            Guid CId = Guid.Empty;
            try
            {
                var appointment = await _doctorappointmentRepository.FirstOrDefaultAsync(x => x.Id == input.AppointmentId && (x.IsBooked == 0 || x.IsBooked == 3));
                if (appointment != null)
                {
                    var paymentSuccess = new UserAppointmentPayment
                    {
                        AppointmentId = input.AppointmentId,
                        CreatedBy = input.UserId,
                        CreatedOn = DateTime.UtcNow,
                        Description = input.Description,
                        OriginalPayAmount = input.OriginalPayAmount,
                        PayAmount = input.PayAmount,
                        PaymentMessage = "Payment Successful",
                        Status = "Paid",
                        UpdatedBy = input.UserId,
                        UpdatedOn = DateTime.UtcNow,
                        UserId = input.UserId,
                        PaymentMethod = 3 //depositeAmount
                    };
                    if (CId != Guid.Empty)
                    {
                        paymentSuccess.CouponId = CId;
                    }
                    var paymentId = await _userAppointmentPayment.InsertAndGetIdAsync(paymentSuccess);

                    var userCoupon = new UserCouponTransaction
                    {
                        UserId = input.UserId,
                        AppointmentId = input.AppointmentId,
                        //CouponId = input.CouponId,
                        CreatedBy = input.UserId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = input.UserId,
                        UpdatedOn = DateTime.UtcNow
                    };
                    if (CId != Guid.Empty)
                    {
                        userCoupon.CouponId = CId;
                    }
                    await _userCouponTransaction.InsertAsync(userCoupon);

                    paymentResultOutput.Message = "Payment Successfull";
                    paymentResultOutput.StatusCode = 200;
                    paymentResultOutput.PaymentId = paymentId;

                    string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    appointment.Status = "Confirm";
                    appointment.IsBooked = 1;
                    appointment.MissedAppointment = true;
                    appointment.UpdatedOn = DateTime.UtcNow;
                    await _doctorappointmentRepository.UpdateAsync(appointment);

                   
                    var patientuser = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.UserId);
                    if (patientuser != null && input.IsPatient == false)
                    {
                        var userMetaDetails = _userMetaDetailsRepository.GetAllList().Where(x => x.UserId == input.UserId).FirstOrDefault();
                        if (userMetaDetails != null)
                        {
                            userMetaDetails.AmountDeposit = userMetaDetails.AmountDeposit - input.PayAmount;
                            await _userMetaDetailsRepository.UpdateAsync(userMetaDetails);
                        }
                    }

                    var doctor = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == appointment.DoctorId);

                    var patientUserType = patientuser.UserType == "MedicalLegal" ? "Medical Legal" : patientuser.UserType;

                    if (appointment.AppointmentSlotId != Guid.Empty)
                    {
                        var soltslist = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);
                        soltslist.IsBooked = 1;
                        soltslist.UpdatedOn = DateTime.UtcNow;
                        await _doctorAppointmentSlotRepository.UpdateAsync(soltslist);

                        var slot = await _doctorAppointmentSlotRepository.GetAsync((Guid)appointment.AppointmentSlotId);

                        DateTime patientStartTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));
                        DateTime patientEndTime = TimeZoneInfo.ConvertTimeFromUtc(slot.SlotZoneEndTime, TimeZoneInfo.FindSystemTimeZoneById(patientuser.Timezone));

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
                        //               + "Link to join the meeting will be send 10 minutes before the meeting time."
                        //                // + "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //                + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        //string patientMailBody = " Hello " + "<b>" + patientuser.Name.ToPascalCase() + " " + patientuser.Surname.ToPascalCase() + ",</b> <br /><br /> " + "Your appointment has been booked successfully and here are the details -" + " <br /><br /><br /> " + "Doctor Name: " + doctor.FullName
                        //               + " <br /><br /> " + "Appointment Date and Time: " + slot.AvailabilityDate + " "
                        //               + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt") + " to " + DateTime.Parse(slot.AvailabilityEndTime).ToString("hh:mm tt") + "(" + slot.TimeZone + ")" + " <br /><br />"
                        //               + "Link to join the meeting will be send 10 minutes before the meeting time."
                        //               // + "Please join here, <a href='" + _configuration["PORTAL_URL"] + "/#/emro/meeting/join/" + appointment.Title + "/" + appointment.Id + "/" + appointment.meetingId + "/" + appointment.UserId + "/" + patientuser.UserType + "'>Click to Join</a> <br /><br />"
                        //               + " <br /><br /><br /> " + "Regards," + " <br />" + "EMRO Team";

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(doctor.EmailAddress, "Appointment Confirmation", doctorMailBody, adminmail));

                        BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patientuser.EmailAddress, "Appointment Confirmation", patientMailBody, adminmail));

                        string Title = "Appointment Confirmation";
                        string userMessage = "Your appointment has been booked successfully with Dr "
                             + doctor.FullName + " on " + patientStartTime.ToString("MM/dd/yyyy") + " at " + patientStartTime.ToString("hh:mm tt")
                             + " (" + patientuser.Timezone + ")";
                        _customNotificationAppService.NotificationPublish(patientuser.Id, Title, userMessage);

                        string drMessage = patientuser.FullName + " booked an appointment with you for "
                             + slot.AvailabilityDate + " at " + DateTime.Parse(slot.AvailabilityStartTime).ToString("hh:mm tt")
                             + " (" + slot.TimeZone + ")";
                        _customNotificationAppService.NotificationPublish(doctor.Id, Title, drMessage);

                        stopwatch.Stop();

                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(appointment);
                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with deposit amount.";
                        createEventsInput.Component = "Payment";
                        createEventsInput.Action = "Deposit Amount Payment";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);

                    }
                    else if(appointment.AppointmentSlotId == Guid.Empty)
                    {
                        stopwatch.Stop();
                        //Log Audit Events
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(appointment);
                        createEventsInput.Operation = patientuser.FullName + " has booked and confirmed an appointment(book later) with consultant " + doctor.Title + " " + doctor.FullName + ", payment done with deposit amount.";
                        createEventsInput.Component = "Payment";
                        createEventsInput.Action = "Deposit Amount Payment";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                    }

                }
                else
                {
                    paymentResultOutput.Message = "Payment already done.";
                    paymentResultOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Stripe Card Payment Error" + ex.StackTrace);
                paymentResultOutput.Message = "Payment Error";
                paymentResultOutput.StatusCode = 500;
            }
            return paymentResultOutput;
        }

        protected async Task<ValidaToken> ValidateCardDetails(string NameOnCard, string CardNumber, string CVC, long? ExpirationMonth, long? ExpirationYear, double PayAmount, string Description)
        {
            ValidaToken validaToken = new ValidaToken();
            long amount = Convert.ToInt64(PayAmount) * 100;
            try
            {
                StripeConfiguration.ApiKey = _stripeSetting.SecretKey;
                var optionstoken = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Name = NameOnCard,
                        Number = CardNumber,
                        Cvc = CVC,
                        ExpMonth = ExpirationMonth,
                        ExpYear = ExpirationYear,
                    }
                };

                var servicetoken = new TokenService();
                Token stripeToken = await servicetoken.CreateAsync(optionstoken);
                var options = new ChargeCreateOptions
                {
                    Currency = "usd", // required 
                    Amount = amount,
                    //Currency = stripeToken.Card.Currency,
                    Description = Description,
                    Source = stripeToken.Id
                };

                var chargeService = new ChargeService();
                Charge charge = await chargeService.CreateAsync(options);
                validaToken.charge = charge;
                validaToken.options = options;
                validaToken.stripeToken = stripeToken;
                validaToken.StatusCode = 200;
                validaToken.Message = "Token Generated";
            }
            catch (Exception ex)
            {
                validaToken.StatusCode = 500;
                validaToken.Message = ex.Message;
                Logger.Error("Validate Card DetailsPayment Error" + ex.StackTrace);
            }
            return validaToken;
        }
    }
}
