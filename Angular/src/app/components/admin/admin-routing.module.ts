import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AdminChildAppComponent } from './admin-child-app/admin-child-app.component';
import { ConsultantsComponent } from './consultants/consultants.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DiagnosticsComponent } from './diagnostics/diagnostics.component';
import { EmroAdminsComponent } from './emro-admins/emro-admins.component';
import { FamilyDoctorsComponent } from './family-doctors/family-doctors.component';
import { InsurancesComponent } from './insurances/insurances.component';
import { MedicalLegalsComponent } from './medical-legals/medical-legals.component';
import { PatientsComponent } from './patients/patients.component';
import { ProfileComponent } from './profile/profile.component';
import { SystemMonitoringComponent } from './system-monitoring/system-monitoring.component';
import { InboxComponent } from '../mail/inbox/inbox.component';
import { ComposeComponent } from '../mail/compose/compose.component';
import { ConsentFormsComponent } from './consent-forms/consent-forms.component';
import { RequestDoctorsComponent } from './request-doctors/request-doctors.component';
import { CostConfigurationComponent } from './cost-configuration/cost-configuration.component';
import { AddNotificationComponent } from '../add-notification/add-notification.component';
import { SurveyFeedbacksComponent } from './survey-feedbacks/survey-feedbacks.component';

const routes: Routes = [{
  path: "",
  redirectTo: "manage",
  pathMatch: "full"
},
{
  path: "manage",
  component: AdminChildAppComponent,
  children: [
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
      path: "consultants",
      component: ConsultantsComponent,
    },
    {
      path: "patients",
      component: PatientsComponent,
    },
    {
      path: "family-doctors",
      component: FamilyDoctorsComponent,
    },
    {
      path: "diagnostics",
      component: DiagnosticsComponent,
    },
    {
      path: "insurances",
      component: InsurancesComponent,
    },
    {
      path: "medical-legals",
      component: MedicalLegalsComponent,
    },
    {
      path: "etelehealth-admins",
      component: EmroAdminsComponent,
    },
    {
      path: "system-monitoring",
      component: SystemMonitoringComponent,
    },
    {
      path: "consent-forms",
      component: ConsentFormsComponent,
    },
    {
      path: "request-doctors",
      component: RequestDoctorsComponent,
    },
    {
      path: "cost-configuration",
      component: CostConfigurationComponent,
    },
    {
      path: "add-notification",
      component: AddNotificationComponent,
    },
    {
      path: "survey-feedbacks",
      component: SurveyFeedbacksComponent,
    }
  ]
}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
