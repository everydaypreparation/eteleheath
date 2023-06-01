import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ConsultantDetailsComponent } from './consultant-details/consultant-details.component';
import { ConsultantsListComponent } from './consultants-list/consultants-list.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PatientDetailsComponent } from './patient-details/patient-details.component';
import { PatientInfoFormComponent } from './patient-info-form/patient-info-form.component';
import { PaymentDetailsComponent } from './payment-details/payment-details.component';
import { PaymentComponent } from './payment/payment.component';
import { ProfileComponent } from './profile/profile.component';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { MedicalLegalDashboardComponent } from '../medical-legal-dashboard/medical-legal-dashboard.component';

const routes: Routes = [
  {
    path: "",
    redirectTo: "dashboard",
    pathMatch: "full",
    canActivate: [AuthGuard]
  },
  {
    path: "dashboard",
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "profile",
    component: ProfileComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "patient-info-form/:appointmentId/:doctorId",
    component: PatientInfoFormComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "patient-details/:appointmentId",
    component: PatientDetailsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "consultant-details/:userId",
    component: ConsultantDetailsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "consultants-list",
    component: ConsultantsListComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "medical-legal-dashboard",
    component: MedicalLegalDashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "payment/:appointmentId/:doctorId",
    component: PaymentComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "payment-details/:appointmentId/:paymentId/:doctorId/:verify",
    component: PaymentDetailsComponent,
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MedicalLegalRoutingModule { }
