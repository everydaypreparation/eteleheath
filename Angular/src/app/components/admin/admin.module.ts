import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { AdminRoutingModule } from './admin-routing.module';
import { AdminChildAppComponent } from './admin-child-app/admin-child-app.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ConsultantsComponent } from './consultants/consultants.component';
import { DiagnosticsComponent } from './diagnostics/diagnostics.component';
import { EmroAdminsComponent } from './emro-admins/emro-admins.component';
import { FamilyDoctorsComponent } from './family-doctors/family-doctors.component';
import { InsurancesComponent } from './insurances/insurances.component';
import { MedicalLegalsComponent } from './medical-legals/medical-legals.component';
import { PatientsComponent } from './patients/patients.component';
import { SystemMonitoringComponent } from './system-monitoring/system-monitoring.component';
import { ProfileComponent } from './profile/profile.component';
import { FormsModule } from '@angular/forms';
import { ConsentFormsComponent } from './consent-forms/consent-forms.component';
import { NgxMaskModule } from 'ngx-mask';
import { RequestDoctorsComponent } from './request-doctors/request-doctors.component';
import { CostConfigurationComponent } from './cost-configuration/cost-configuration.component';
import { CarouselModule } from 'ngx-owl-carousel-o';
import { DirectiveSharedModule } from 'src/app/directives/directive-shared.module';
import { SurveyFeedbacksComponent } from './survey-feedbacks/survey-feedbacks.component';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';

@NgModule({
  declarations: [AdminChildAppComponent, DashboardComponent, ConsultantsComponent, DiagnosticsComponent, EmroAdminsComponent, FamilyDoctorsComponent, InsurancesComponent, MedicalLegalsComponent, PatientsComponent, SystemMonitoringComponent, ProfileComponent, ConsentFormsComponent, RequestDoctorsComponent, CostConfigurationComponent, SurveyFeedbacksComponent],
  imports: [
    NgMultiSelectDropDownModule,
    CommonModule,
    CarouselModule,
    AdminRoutingModule,
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
    FormsModule,
    NgxMaskModule.forRoot(),
    DirectiveSharedModule
  ]
})
export class AdminModule { }
