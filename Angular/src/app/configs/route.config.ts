import { Injectable } from "@angular/core";

@Injectable()
export class RouteConfig {
    readonly signInPath = "signin";
    readonly signUpPath = "signup";
    readonly forgotPasswordPath = "forgot-password";
    readonly findingConsultantPath = "finding-consultant";
    readonly consultantRegistrationPath = "consultant-registration";
    readonly patientRegistrationPath = "patient-registration";
    readonly familyDoctorRegistrationPath = "family-doctor-registration";
    readonly insuranceRegistrationPath = "insurance-registration";
    readonly medicalRegistrationPath = "medical-legal-registration";
    readonly diagnosticRegistrationPath = "diagnostic-registration";
    readonly familyDoctorEmptyDashboardPath = "family-doctor-dashboard";
    // readonly insuranceEmptyDashboardPath = "insurance-dashboard";
    // readonly medicalLegalEmptyDashboardPath = "medical-legal-dashboard";
    readonly diagnosticEmptyDashboardPath = "diagnostic-dashboard";
    readonly updateConsentFormPath = "update-consent-form";

    readonly childPath = "etelehealth/";

    readonly adminPath = "admin/";
    readonly adminChildPath = "manage/";
    readonly adminDashboardPath = this.childPath + this.adminPath + this.adminChildPath + "dashboard";
    readonly adminProfilePath = this.childPath + this.adminPath + this.adminChildPath + "profile";
    readonly adminConsultantsPath = this.childPath + this.adminPath + this.adminChildPath + "consultants";
    readonly adminPatientsPath = this.childPath + this.adminPath + this.adminChildPath + "patients";
    readonly adminFamilyDoctorsPath = this.childPath + this.adminPath + this.adminChildPath + "family-doctors";
    readonly adminDiagnosticsPath = this.childPath + this.adminPath + this.adminChildPath + "diagnostics";
    readonly adminInsurancesPath = this.childPath + this.adminPath + this.adminChildPath + "insurances";
    readonly adminMedicalLegalsPath = this.childPath + this.adminPath + this.adminChildPath + "medical-legals";
    readonly adminEmroAdminsPath = this.childPath + this.adminPath + this.adminChildPath + "etelehealth-admins";
    readonly adminSystemMonitoringPath = this.childPath + this.adminPath + this.adminChildPath + "system-monitoring";
    readonly adminConsentFormsPath = this.childPath + this.adminPath + this.adminChildPath + "consent-forms";
    readonly adminRequestDoctorsPath = this.childPath + this.adminPath + this.adminChildPath + "request-doctors";
    readonly costConfigurationPath = this.childPath + this.adminPath + this.adminChildPath + "cost-configuration";
    readonly addNotificationPath = this.childPath + this.adminPath + this.adminChildPath + "add-notification";
    readonly surveyFeedbacksPath = this.childPath + this.adminPath + this.adminChildPath + "survey-feedbacks";
    readonly adminViewUserPath = this.childPath + "view-user";

    readonly messagingInboxPath = this.childPath+"mail/inbox";
    readonly messagingComposePath = this.childPath + "mail/compose";
    readonly messagingSentPath = this.childPath+"mail/sent";
    readonly messagingTrashPath = this.childPath+"mail/trash";
    readonly messagingViewMailPath = this.childPath+"mail/view-mail";
    readonly notesPath = this.childPath + "notes";
    readonly surveyFormPath = this.childPath + "survey-form";
    readonly viewNotesPath = this.childPath + "view-note";
    readonly samvaad = this.childPath + "meeting/join";
    readonly changePasswordPath = this.childPath + "change-password";

    readonly consultantPath = "consultant/";
    readonly consultantDashboardPath = this.childPath + this.consultantPath + "dashboard";
    readonly consultantProfilePath = this.childPath + this.consultantPath + "profile";
    readonly consultantPatientDetailsPath = this.childPath + this.consultantPath + "patient-details";

    readonly consultantReportPath = this.childPath + this.consultantPath+"consultant-report";
    readonly patientPath = "patient/";
    readonly patientDashboardPath = this.childPath + this.patientPath + "dashboard";
    readonly patientProfilePath = this.childPath + this.patientPath + "profile";
    readonly patientAddInfoFormPath = this.childPath + this.patientPath + "add-info-form";
    readonly patientDoctorDetailsPath = this.childPath + this.patientPath + "doctor-details";
    readonly patientDoctorListPath = this.childPath + this.patientPath + "doctors-list";
    readonly patientPaymentPath = this.childPath + this.patientPath + "payment";
    readonly patientPaymentDetailsPath = this.childPath + this.patientPath + "payment-details";

    readonly familyDoctorPath = "family-doctor/";
    readonly familyDoctorDashboardPath = this.childPath + this.familyDoctorPath + "dashboard";
    readonly familyDoctorsEmptyDashboardPath = this.childPath + this.familyDoctorPath + "family-doctor-dashboard";
    readonly familyDoctorProfilePath = this.childPath + this.familyDoctorPath + "profile";
    readonly familyDoctorPatientDetailsPath = this.childPath + this.familyDoctorPath + "patient-details";

    readonly insurancePath = "insurance/";
    readonly insuranceDashboardPath = this.childPath + this.insurancePath + "dashboard";
    readonly insuranceProfilePath = this.childPath + this.insurancePath + "profile";
    readonly insurancePatientInfoFormPath = this.childPath + this.insurancePath + "patient-info-form";
    readonly insurancePatientDetailsPath = this.childPath + this.insurancePath + "patient-details";
    readonly insuranceConsultantDetailsPath = this.childPath + this.insurancePath + "consultant-details";
    readonly insuranceConsultantsListPath = this.childPath + this.insurancePath + "consultants-list";
    readonly insurancePaymentPath = this.childPath + this.insurancePath + "payment";
    readonly insurancePaymentDetailsPath = this.childPath + this.insurancePath + "payment-details";
    readonly insuranceEmptyDashboardPath = this.childPath + this.insurancePath + "insurance-dashboard";

    readonly medicalLegalPath = "medical-legal/";
    readonly medicalLegalDashboardPath = this.childPath + this.medicalLegalPath + "dashboard";
    readonly medicalLegalProfilePath = this.childPath + this.medicalLegalPath + "profile";
    readonly medicalLegalPatientInfoFormPath = this.childPath + this.medicalLegalPath + "patient-info-form";
    readonly medicalLegalPatientDetailsPath = this.childPath + this.medicalLegalPath + "patient-details";
    readonly medicalLegalConsultantDetailsPath = this.childPath + this.medicalLegalPath + "consultant-details";
    readonly medicalLegalConsultantsListPath = this.childPath + this.medicalLegalPath + "consultants-list";
    readonly medicalLegalPaymentPath = this.childPath + this.medicalLegalPath + "payment";
    readonly medicalLegalPaymentDetailsPath = this.childPath + this.medicalLegalPath + "payment-details";
    readonly medicalLegalEmptyDashboardPath = this.childPath + this.medicalLegalPath + "medical-legal-dashboard";

    readonly diagnosticPath = "diagnostic/";
    readonly diagnosticDashboardPath = this.childPath + this.diagnosticPath + "dashboard";
    readonly diagnosticDashboardDetailsPath = this.childPath + this.diagnosticPath + "diagnostic-dashboard";
    readonly diagnosticProfilePath = this.childPath + this.diagnosticPath + "profile";
    readonly diagnosticPatientDetailsPath = this.childPath + this.diagnosticPath + "patient-details";

    readonly notFoundPath = "not-found";

    readonly auditReportPath = this.childPath + "audit-report";
}
