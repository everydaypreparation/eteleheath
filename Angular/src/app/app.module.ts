import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ApiConfig } from './configs/api.config';
import { RouteConfig } from './configs/route.config';
import { PropConfig } from './configs/prop.config';
import { ApiService } from './services/api.service';
import { UserService } from './services/user.service';
import { ValidationService } from './services/validation.service';
import { HelperService } from './services/helper.service';
import { AuthGuard } from './guards/auth.guard';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { AppComponent } from './app.component';
import { SpinnerComponent } from "./shared/spinner.component";
import { CommonComponent } from "./shared/common.component";
import { FirstCharPipe } from './pipes/first-char.pipe';
import { SigninComponent } from './components/signin/signin.component';
import { SignupComponent } from './components/signup/signup.component';
import { ChildAppComponent } from './components/child-app/child-app.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { ChildNotFoundComponent } from './components/child-not-found/child-not-found.component';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { ConsultantRegistrationComponent } from './components/consultant-registration/consultant-registration.component';
import { DiagnosticDashboardComponent } from './components/diagnostic-dashboard/diagnostic-dashboard.component';
import { DiagnosticRegistrationComponent } from './components/diagnostic-registration/diagnostic-registration.component';
import { FamilyDoctorDashboardComponent } from './components/family-doctor-dashboard/family-doctor-dashboard.component';
import { FamilyDoctorRegistrationComponent } from './components/family-doctor-registration/family-doctor-registration.component';
import { FindingConsultantComponent } from './components/finding-consultant/finding-consultant.component';
import { InsuranceDashboardComponent } from './components/insurance-dashboard/insurance-dashboard.component';
import { InsuranceRegistrationComponent } from './components/insurance-registration/insurance-registration.component';
import { MedicalLegalDashboardComponent } from './components/medical-legal-dashboard/medical-legal-dashboard.component';
import { MedicalLegalRegistrationComponent } from './components/medical-legal-registration/medical-legal-registration.component';
import { PatientRegistrationComponent } from './components/patient-registration/patient-registration.component';
import { StickyBarService } from './services/sticky.bar.service';
import { ConfirmModalComponent } from './components/confirm-modal/confirm-modal.component';
import { ViewUserComponent } from './components/view-user/view-user.component';
import { AppointmentSlotModelComponent } from './components/appointment-slot-model/appointment-slot-model.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AppointmentBookModelComponent } from './components/appointment-book-model/appointment-book-model.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { InboxComponent } from './components/mail/inbox/inbox.component';
import { SentComponent } from './components/mail/sent/sent.component';
import { TrashComponent } from './components/mail/trash/trash.component';
import { ViewMailComponent } from './components/mail/view-mail/view-mail.component';
import { ComposeComponent } from './components/mail/compose/compose.component';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';
import { RescheduleAppointmentModalComponent } from './components/reschedule-appointment-modal/reschedule-appointment-modal.component';
import { UploadDocumentModalComponent } from './components/upload-document-modal/upload-document-modal.component';
import { NotesComponent } from './components/notes/notes.component';
import { NotesModelComponent } from './components/notes-model/notes-model.component';
import { ViewNoteModelComponent } from './components/view-note-model/view-note-model.component';
import { RequestTestModalComponent } from './request-test-modal/request-test-modal.component';
import { UpdateConsentFormComponent } from './components/update-consent-form/update-consent-form.component';
import { ConsultantReportComponent } from './components/consultant/consultant-report/consultant-report.component';
import { SignaturePadModule } from 'angular2-signaturepad';
import { InputDigitsOnlyDirective } from './shared/input-digits-only.directive';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { NgxMaskModule } from 'ngx-mask';
import { RequestDocModalComponent } from './components/request-doc-modal/request-doc-modal.component';
import { ViewRequestDoctorModelComponent } from './components/view-request-doctor-model/view-request-doctor-model.component';
import { NgxPayPalModule } from 'ngx-paypal';
import { SamvaadMeetingsComponent } from './components/samvaad-meetings/samvaad-meetings.component';
import { SamvaadLogoutRedirectComponent } from './components/samvaad-logout-redirect/samvaad-logout-redirect.component';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { AddPatientModalComponent } from './components/add-patient-modal/add-patient-modal.component';
import { UploadPaymentDocumentModalComponent } from './components/upload-payment-document-modal/upload-payment-document-modal.component';
import { CancelAppointmentModalComponent } from './components/cancel-appointment-modal/cancel-appointment-modal.component';
import { AddCostModalComponent } from './components/add-cost-modal/add-cost-modal.component';
import { RequestUsersModalComponent } from './components/request-users-modal/request-users-modal.component';
import { ChangePasswordComponent } from './components/change-password/change-password.component';
import { AddNotificationComponent } from './components/add-notification/add-notification.component';
import { AddNotificationModalComponent } from './components/add-notification-modal/add-notification-modal.component';
import { CarouselModule } from 'ngx-owl-carousel-o';
import { AppointmentBookLaterModalComponent } from './components/appointment-book-later-modal/appointment-book-later-modal.component';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { ViewDocModalComponent } from './components/view-doc-modal/view-doc-modal.component';
import { DirectiveSharedModule } from './directives/directive-shared.module';
import { AuditReportComponent } from './components/audit-report/audit-report.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SurveyFormComponent } from './components/survey-form/survey-form.component';
import { ViewConsultantReportModalComponent } from './components/view-consultant-report-modal/view-consultant-report-modal.component';
import { ViewSurveyFeedbackModelComponent } from './components/view-survey-feedback-model/view-survey-feedback-model.component';

@NgModule({
  declarations: [
    AppComponent,
    SpinnerComponent,
    CommonComponent,
    FirstCharPipe,
    SigninComponent,
    SignupComponent,
    ChildAppComponent,
    NotFoundComponent,
    ChildNotFoundComponent,
    HeaderComponent,
    FooterComponent,
    ForgotPasswordComponent,
    ConsultantRegistrationComponent,
    DiagnosticDashboardComponent,
    DiagnosticRegistrationComponent,
    FamilyDoctorDashboardComponent,
    FamilyDoctorRegistrationComponent,
    FindingConsultantComponent,
    InsuranceDashboardComponent,
    InsuranceRegistrationComponent,
    MedicalLegalDashboardComponent,
    MedicalLegalRegistrationComponent,
    PatientRegistrationComponent,
    ConfirmModalComponent,
    ViewUserComponent,
    AppointmentSlotModelComponent,
    AppointmentBookModelComponent,
    ResetPasswordComponent,
    RescheduleAppointmentModalComponent,
    InboxComponent,
    SentComponent,
    TrashComponent,
    ViewMailComponent,
    ComposeComponent,
    UploadDocumentModalComponent,
    NotesComponent,
    NotesModelComponent,
    ViewNoteModelComponent,
    ConsultantReportComponent,
    RequestTestModalComponent,
    RequestDocModalComponent,
    UpdateConsentFormComponent,
    InputDigitsOnlyDirective,
    ViewRequestDoctorModelComponent,
    SamvaadMeetingsComponent,
    SamvaadLogoutRedirectComponent,
    AddPatientModalComponent,
    UploadPaymentDocumentModalComponent,
    CancelAppointmentModalComponent,
    AddCostModalComponent,
    RequestUsersModalComponent,
    ChangePasswordComponent,
    AddNotificationComponent,
    AddNotificationModalComponent,
    AppointmentBookLaterModalComponent,
    ViewDocModalComponent,
    AuditReportComponent,
    SurveyFormComponent,
    ViewConsultantReportModalComponent,
    ViewSurveyFeedbackModelComponent

  ],
  imports: [
    BrowserModule,
    CarouselModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    CommonModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatMenuModule,
    MatTabsModule,
    MatSelectModule,
    MatInputModule,
    MatRadioModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    NgbModule,
    NgMultiSelectDropDownModule,
    SignaturePadModule,
    MatAutocompleteModule,
    NgxPayPalModule,
    CKEditorModule,
    NgxDocViewerModule,
    DragDropModule,
    NgxMaskModule.forRoot(),
    DirectiveSharedModule
  ],
  providers: [ApiConfig, RouteConfig, PropConfig, ApiService, UserService, ValidationService, HelperService, StickyBarService, AuthGuard, DecimalPipe],
  bootstrap: [AppComponent]
})
export class AppModule { }
