import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddInfoFormComponent } from './add-info-form/add-info-form.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DoctorDetailsComponent } from './doctor-details/doctor-details.component';
import { DoctorsListComponent } from './doctors-list/doctors-list.component';
import { PaymentDetailsComponent } from './payment-details/payment-details.component';
import { PaymentComponent } from './payment/payment.component';
import { ProfileComponent } from './profile/profile.component';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { SurveyFormComponent } from '../survey-form/survey-form.component';


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
    path: "add-info-form/:appointmentId/:doctorId",
    component: AddInfoFormComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "doctor-details/:userId",
    component: DoctorDetailsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: "doctors-list",
    component: DoctorsListComponent,
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
export class PatientRoutingModule { }
