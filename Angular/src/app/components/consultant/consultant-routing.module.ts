import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ProfileComponent } from './profile/profile.component';
import { PatientDetailsComponent } from './patient-details/patient-details.component';
import { ConsultantReportComponent } from './consultant-report/consultant-report.component';
import { AuthGuard } from 'src/app/guards/auth.guard';

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
    path: "patient-details/:appointmentId",
    component: PatientDetailsComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'consultant-report/:consultId',
    component: ConsultantReportComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'consultant-report/:consultId/:appointmentId',
    component: ConsultantReportComponent,
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ConsultantRoutingModule { }
