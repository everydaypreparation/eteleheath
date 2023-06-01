import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ProfileComponent } from './profile/profile.component';
import { DiagnosticDashboardComponent } from '../diagnostic-dashboard/diagnostic-dashboard.component';
import { PatientDetailsComponent } from './patient-details/patient-details.component';


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
  }
  ,
  {
    path: "diagnostic-dashboard",
    component: DiagnosticDashboardComponent,
  },
  {
    path: "patient-details/:patientId/:patientNumericId/:appointmentId",
    component: PatientDetailsComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DiagnosticRoutingModule { }
