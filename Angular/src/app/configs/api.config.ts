import { Injectable } from "@angular/core";

@Injectable()
export class ApiConfig {

    readonly signin = "v1/TokenAuth/Authenticate/";
    readonly signupPatient = "services/app/User/SignUp/";
    readonly forgotPassword = "v1/TokenAuth/ForgotPassword";
    readonly resetPassword = "v1/TokenAuth/ResetPassword";

    //User
    readonly getUser = "services/app/User/GetById?Id=";
    readonly createUser = "services/app/User/Create";
    readonly getUserByRoles = "services/app/User/GetUserByRoles?RoleName=";
    readonly getUserDetailsById = "services/app/User/GetUserDetailsId?UserId=";
    readonly deleteUserDetailsById = "services/app/User/DeleteUser?UserId=";

    //consultant
    readonly consultantCreate = "services/app/User/CreateConsultant";
    readonly updateConsultant = "services/app/User/UpdateConsultant";

    //patient
    readonly createPatient = "services/app/User/CreatePatient";
    readonly updatePatient = "services/app/User/UpdatePatient";

    //Family doctor
    readonly createFamilyDoctor = "services/app/User/CreateFamilyDoctor";
    readonly updateFamilyDoctor = "services/app/User/UpdateFamilyDoctor";

    //diagnosis
    readonly createDiagnostic = "services/app/User/CreateDiagnostic";
    readonly updateDiagnostic = "services/app/User/UpdateDiagnostic";
    readonly activePatient = "services/app/Diagnostics/ActivePatient";
    readonly archivePatient = "services/app/Diagnostics/ArchivePatient";
    readonly searchPatient = "services/app/Diagnostics/Search";
    readonly archivePatientRecord = "services/app/Diagnostics/Update";
    readonly deletePatientRecord = "services/app/Diagnostics/Delete?CaseId=";

    //medicalLegal
    readonly createMedicalLegal = "services/app/User/CreateMedicalLegal";
    readonly updateMedicalLegal = "services/app/User/UpdateMedicalLegal";

    //Insurance
    readonly createInsurance = "services/app/User/CreateInsurance";
    readonly updateInsurance = "services/app/User/UpdateInsurance";

    //AppointmentSlot
    readonly createAvailabilitySlot = "services/app/AppointmentSlot/Create";
    readonly updateAvailabilitySlot = "services/app/AppointmentSlot/Update?Id=";
    readonly getAvailabilitySlotById = "services/app/AppointmentSlot/GetAppointmentSlotbyId?Id=";
    readonly getAllAppointmentSlotbyDoctorId = "services/app/AppointmentSlot/GetAllAppointmentSlotbyDoctorId?UserId=";
    readonly deleteAppointmentSlotbyId = "services/app/AppointmentSlot/Delete?Id=";
    readonly getAllUnbookedAppointmentSlotbyDoctorId = "services/app/AppointmentSlot/GetAllUnbookedAppointmentSlotbyDoctorId?UserId=";
    readonly getAvailabilitySlot = "services/app/Appointment/Get?UserId=";
    readonly IsSlotAvailable = "services/app/AppointmentSlot/IsSlotAvailable";

    //Appointment
    readonly appointmentBook = "services/app/Appointment/Create";
    readonly rescheduleAppointment = "services/app/Appointment/Reschedule";
    readonly cancelAppointment = "services/app/Appointment/Cancel";
    readonly getAppoinmentDetailsById = "services/app/Appointment/GetAppoinmentDetails?AppointmentId=";
    readonly GetInsuranceAppointmentDetailsById = "services/app/Appointment/GetInsuranceAppointmentDetails?AppointmentId=";
    readonly searchConsultant = "services/app/Patient/Search";
    readonly submitPatientConsentForm = "services/app/Patient/Create";
    readonly getAllUpcomingAppointmentById = "services/app/Appointment/GetBookedAppointment?Id=";
    readonly getNextAppointmentById = "services/app/Appointment/GetUserAppointment?Id=";
    readonly getAllAppointmentById = "services/app/Appointment/GetAllAppointment?Id=";
    readonly getPatientAppointmentDetails = "services/app/Appointment/GetPatientAppointmentDetails?UserId=";
    readonly getFamilyDoctorInfoByName = "services/app/Patient/GetByName";
    readonly getMissedAppointments = "services/app/Appointment/GetMissedAppointments?Id=";
    readonly updateAppointments = "services/app/Appointment/Update";
    readonly getBookLaterAppointment = "services/app/Appointment/GetBookLaterAppointment?Id=";
    readonly getBookedAppointmentByUserIdAndAppointmentId = "services/app/Appointment/GetBookedAppointmentByUserIdAndAppointmentId?Id=";

    //Payment
    readonly appointmentPayment = "services/app/Payment/AppointmentPayment";
    readonly getPaymentDetails = "services/app/Payment/PaymentDetails";
    readonly getCouponCodes = "services/app/Coupon/Get";
    readonly validateCoupon = "services/app/Coupon/ValidateCoupon";

    //messaging
    readonly getAllUserEmailsByUserId = "services/app/UserMessage/Indox";
    readonly getUserEmails = "services/app/User/GetUserEmails?Id=";
    readonly saveDraftMail = "services/app/UserMessage/Draft";
    readonly deleteAttachment = "services/app/UserMessage/DeleteAttachment?AttachmentId=";
    readonly sendMessages = "services/app/UserMessage/Send";
    readonly draftMessages = "services/app/UserMessage/Draft";
    readonly uploadAttachment = "services/app/UserMessage/UploadAttachment";
    readonly getAllSenderEmailsByUserId = "services/app/UserMessage/Sent";
    readonly getAllTrashEmailsByUserId = "services/app/UserMessage/Thrash";
    readonly getEmailById = "services/app/UserMessage/GetById?UserId=";
    readonly downloadAttachment = "services/app/UserMessage/GetDownloadAttachment?AttachmentId=";
    readonly readBy = "services/app/UserMessage/ReadBy";
    readonly deleteMails = "services/app/UserMessage/Delete?Id=";
    readonly restore = "services/app/UserMessage/Restore";
    readonly getUserMessages = "services/app/UserMessage/GetUserMessages?FromUserId=";

    //Documents
    readonly getDocumentListByUserId = "services/app/Patient/GetDocumentList?UserId=";
    readonly getDocumentListByAppointmentId = "services/app/Patient/GetDocumentList?AppoinmentId=";
    readonly editDocumentById = "services/app/Document/ReUpload";
    readonly uploadDocument = "services/app/Document/Upload";
    readonly deleteDocumentById = "services/app/Document/Delete?DocumentId=";
    readonly downloadDocumentById = "services/app/Document/Get?DocumentId=";

    //Master data
    readonly getSpecialties = "services/app/Specialty/GetSpecialties";
    readonly getSubSpecialties = "services/app/SubSpecialty/GetSubSpecialties";
    readonly getConsentFormsMaster = "services/app/ConsentFormsMaster/GetAll";
    readonly getTimeZoneMaster = "services/app/Master/GetTimeZone";

    //notes
    readonly getAllNotesByUserId = "services/app/UserNotes/GetAll?UserId=";
    readonly saveNote = "services/app/UserNotes/Create";
    readonly deleteNoteById = "services/app/UserNotes/Delete?NotesId=";
    readonly getNoteById = "services/app/UserNotes/Get?NotesId=";
    readonly updateNote = "services/app/UserNotes/Update";
    readonly getNoteByUserIdAndAppointmentId = "services/app/UserNotes/GetNotesForSamvaad?UserId=";
    readonly saveOrUpdateNote = "services/app/UserNotes/CreateUpdateNotesForSamvaad";

    //Consent forms
    readonly getAllConsentForms = "services/app/ConsentFormsMaster/GetAll";
    readonly createConsentForm = "services/app/ConsentFormsMaster/Create";
    readonly getConsentFormById = "services/app/ConsentFormsMaster/GetById?Id=";
    readonly updateConsentFormById = "services/app/ConsentFormsMaster/Update";
    readonly deleteConsentFormById = "services/app/ConsentFormsMaster/Delete?Id=";

    // Consultant reports
    readonly getConsultantReport = "services/app/ConsultReport/Get?ConsultId=";
    readonly createConsultantReport = "services/app/ConsultReport/Create";
    readonly completeConsultantReport = "services/app/ConsultReport/Completed";
    readonly deleteConsultantReport = "services​/app​/ConsultReport​/Delete?ConsultId=";
    readonly getActiveCases = "services/app/ConsultReport/ActiveCase";
    readonly getArchiveCases = "services/app/ConsultReport/ArchiveCase";
    readonly getConsultantReportsByAppointmentIdAndRoleName = "services/app/ConsultReport/UserConsultantReport";
    readonly getPatientConsultantReportByPatientId = "services/app/ConsultReport/PatientConsultantReport";
    readonly getIntakeConsultantReportsByPatientId = "services/app/Patient/GetIntakeConsultantReportsByPatientId?UserId=";
    readonly downloadReportByConsultId = "services/app/ConsultReport/GetReport?ConsultId=";
    readonly getConsultantStats = "services/app/Appointment/GetConsultantStats?UserId=";
    readonly getConsultantReportForAllRole = "services/app/ConsultReport/GetConsultantReportForAllRole?UserId=";


    //samvaad
    readonly JoinSamvaadMeeting = "services/app/JoinMeeting/JoinSamvaadMeeting";

    //Consultant

    readonly getRequestDoctors = "services/app/Consultant/GetAll";
    readonly deleteRequestDoctorDetail = "services/app/Consultant/Delete?RequestId=";
    readonly getDetailsByRequestId = "services/app/Consultant/Get?Id=";
    readonly createRequestDoctor = "services/app/Consultant/Create";
    readonly updateUserOnboardRequest = "services/app/Consultant/Update";

    //payment
    readonly paymentByPaypal = "services/app/Payment/PaymentByPaypal";
    readonly getPaymentDetailsByPaypal = "services/app/Payment/PayPalDetail";
    readonly payByDepositeAmount = "services/app/Payment/PayByDepositeAmount";

    //Cost
    readonly createCost = "services/app/ConfigurationCosts/Create";
    readonly getAllCost = "services/app/ConfigurationCosts/GetAll";
    readonly getCost = "services/app/ConfigurationCosts/Get?CostId=";
    readonly deleteCost = "services/app/ConfigurationCosts/Delete?CostId=";
    readonly updateCost = "services/app/ConfigurationCosts/Update";

    //save test request
    readonly requestForTest = "services/app/Consultant/RequestForTest";

    //changePassword
    readonly changePassword = "services/app/User/ChangePassword";

    //notification
    readonly deleteNotification = "services/app/AdminNotification/DeleteNotifications?notificationId=";
    readonly getNotificationsById = "services/app/AdminNotification/GetNotificationsById?notificationId=";
    readonly getAllNotification = "services/app/AdminNotification/GetNotifications";
    readonly sendNotification = "services/app/AdminNotification/SendNotifications";
    readonly updateNotification = "services/app/AdminNotification/UpdateNotifications";
    readonly getRolesNotifications = "services/app/AdminNotification/GetRolesNotifications?RoleName=";

    //Roles
    readonly getAllRoles = "services/app/User/GetRoles";
    readonly getLegalStats = "services/app/Appointment/GetLegalStats?UserId=";
    readonly getDashBoardCount = "services/app/User/DashBoard?UserId=";
    readonly getFamilyDoctorStatsCount = "services/app/Appointment/GetFamilyDoctorStats?UserId=";
    readonly getDiagnosticsCount = "services/app/Diagnostics/Dashboard?UserId=";

    //Meeting statistics
    readonly createMeetingLog = "services/app/MeetingLog/Create";
    //getDetails
    readonly getDetails = "services/app/Patient/GetDetails";
    readonly getPatientInfoByName = "services/app/Patient/GetByName";
    readonly getPatientInfoById = "services/app/Patient/GetById?Id=";
    readonly matchesUrl = "https://";

    //Impersonation
    readonly ImpersonateUser = "v1/Impersonation/ImpersonateUser?userId=";
    readonly StopImpersonation = "v1/Impersonation/StopImpersonation?rememberClient=";
    readonly getAuditReports = "services/app/AuditReport/getAuditReport";

    //Custom Notifications
    readonly getCustomNotifications = "services/app/CustomNotification/GetNotifications";
    readonly setCustomNotifications = "services/app/CustomNotification/SetNotifications";
    readonly sendCustomNotifications = "services/app/CustomNotification/SendNotifications";
    readonly deleteCustomNotifications = "services/app/CustomNotification/DeleteNotifications";
    readonly countCustomNotifications = "services/app/CustomNotification/GetNotificationsCount";

    //Survey Form
    readonly getSurveyForm = "services/app/SurveyForm/GetAll";
    readonly submitSurveyForm = "services/app/SurveyForm/SurveyResponse";
    readonly getSurveys = "services/app/SurveyForm/GetSurveyResponse";
    readonly deleteSurvey = "services/app/SurveyForm/Delete?UserId=";
    readonly surveyAuthenticateUser = "services/app/SurveyForm/SurveyAuthenticateUser";

    readonly SendSamvaadEmail = "services/app/UserMessage/SendSamvaadEmail?samvaadLink=";
}
