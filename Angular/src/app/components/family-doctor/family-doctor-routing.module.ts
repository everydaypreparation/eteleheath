import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PatientDetailsComponent } from './patient-details/patient-details.component';
import { ProfileComponent } from './profile/profile.component';
import { FamilyDoctorDashboardComponent } from '../family-doctor-dashboard/family-doctor-dashboard.component';


const routes: Routes = [
  {
    path: "",
    redirectTo: "dashboard",
    pathMatch: "full"
  },
  {
    path: "dashboard",
    component: DashboardComponent,
  },
  {
    path: "profile",
    component: ProfileComponent,
  },
  {
    path: "family-doctor-dashboard",
    component: FamilyDoctorDashboardComponent,
  },
  {
    path: "patient-details/:appointmentId",
    component: PatientDetailsComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FamilyDoctorRoutingModule { }
