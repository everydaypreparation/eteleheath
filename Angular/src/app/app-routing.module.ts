import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ChildAppComponent } from './components/child-app/child-app.component';
import { ChildNotFoundComponent } from './components/child-not-found/child-not-found.component';
import { ConsultantRegistrationComponent } from './components/consultant-registration/consultant-registration.component';
import { DiagnosticDashboardComponent } from './components/diagnostic-dashboard/diagnostic-dashboard.component';
import { DiagnosticRegistrationComponent } from './components/diagnostic-registration/diagnostic-registration.component';
import { FamilyDoctorDashboardComponent } from './components/family-doctor-dashboard/family-doctor-dashboard.component';
import { FamilyDoctorRegistrationComponent } from './components/family-doctor-registration/family-doctor-registration.component';
import { FindingConsultantComponent } from './components/finding-consultant/finding-consultant.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
// import { InsuranceDashboardComponent } from './components/insurance-dashboard/insurance-dashboard.component';
import { InsuranceRegistrationComponent } from './components/insurance-registration/insurance-registration.component';
import { MedicalLegalDashboardComponent } from './components/medical-legal-dashboard/medical-legal-dashboard.component';
import { MedicalLegalRegistrationComponent } from './components/medical-legal-registration/medical-legal-registration.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { PatientRegistrationComponent } from './components/patient-registration/patient-registration.component';
import { SigninComponent } from './components/signin/signin.component';
import { SignupComponent } from './components/signup/signup.component';
import { AuthGuard } from './guards/auth.guard';
import { ViewUserComponent } from './components/view-user/view-user.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { InboxComponent } from './components/mail/inbox/inbox.component';
import { ComposeComponent } from './components/mail/compose/compose.component';
import { SentComponent } from './components/mail/sent/sent.component';
import { TrashComponent } from './components/mail/trash/trash.component';
import { ViewMailComponent } from './components/mail/view-mail/view-mail.component';
import { NotesComponent } from './components/notes/notes.component';


import { UpdateConsentFormComponent } from './components/update-consent-form/update-consent-form.component';

import { SamvaadMeetingsComponent } from './components/samvaad-meetings/samvaad-meetings.component';
import { SamvaadLogoutRedirectComponent } from './components/samvaad-logout-redirect/samvaad-logout-redirect.component';
import { ChangePasswordComponent } from './components/change-password/change-password.component';

import { AuditReportComponent } from './components/audit-report/audit-report.component';
import { SurveyFormComponent } from './components/survey-form/survey-form.component';

const routes: Routes = [
  {
    path: "",
    redirectTo: 'signin',
    pathMatch: 'full'
  },
  {
    path: 'signin',
    component: SigninComponent
  },
  {
    path: 'signup',
    component: SignupComponent
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent
  },
  {
    path: 'reset-password',
    component: ResetPasswordComponent
  },
  { 
    path: 'meeting/logout', 
    component: SamvaadLogoutRedirectComponent 
  },

  {
    path: 'finding-consultant',
    component: FindingConsultantComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL"]
    },
  },
  {
    path: 'consultant-registration/:userId',
    component: ConsultantRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "CONSULTANT"]
    },
  },
  {
    path: 'consultant-registration',
    component: ConsultantRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  {
    path: 'patient-registration/:userId',
    component: PatientRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "PATIENT"]
    },
  },
  {
    path: 'patient-registration',
    component: PatientRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "PATIENT"]
    },
  },
  {
    path: 'family-doctor-registration/:userId',
    component: FamilyDoctorRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "FAMILYDOCTOR"]
    },
  },
  {
    path: 'family-doctor-registration',
    component: FamilyDoctorRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  {
    path: 'family-doctor-dashboard',
    component: FamilyDoctorDashboardComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "FAMILYDOCTOR"]
    },
  },
  {
    path: 'insurance-registration/:userId',
    component: InsuranceRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "INSURANCE"]
    },
  },
  {
    path: 'insurance-registration',
    component: InsuranceRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  // {
  //   path: 'insurance-dashboard',
  //   component: InsuranceDashboardComponent,
  //   canActivate: [AuthGuard],
  //   data: {
  //     roles: ["ADMIN", "INSURANCE"]
  //   },
  // },
  {
    path: 'medical-legal-registration/:userId',
    component: MedicalLegalRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "MEDICALLEGAL"]
    },
  },
  {
    path: 'medical-legal-registration',
    component: MedicalLegalRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  // {
  //   path: 'medical-legal-dashboard',
  //   component: MedicalLegalDashboardComponent,
  //   canActivate: [AuthGuard],
  //   data: {
  //     roles: ["ADMIN", "MEDICALLEGAL"]
  //   },
  // },
  {
    path: 'diagnostic-registration/:userId',
    component: DiagnosticRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "DIAGNOSTIC"]
    },
  },
  {
    path: 'diagnostic-registration',
    component: DiagnosticRegistrationComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  {
    path: 'diagnostic-dashboard',
    component: DiagnosticDashboardComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN", "DIAGNOSTIC"]
    },
  },
  {
    path: 'update-consent-form/:consentFormsId',
    component: UpdateConsentFormComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  {
    path: 'update-consent-form',
    component: UpdateConsentFormComponent,
    canActivate: [AuthGuard],
    data: {
      roles: ["ADMIN"]
    },
  },
  {
    path: "etelehealth",
    component: ChildAppComponent,
    children: [
      {
        path: "admin",
        loadChildren: () => import('./components/admin/admin.module').then(m => m.AdminModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN"]
        },
      },
      {
        path: "consultant",
        loadChildren: () => import('./components/consultant/consultant.module').then(m => m.ConsultantModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "CONSULTANT"]
        },
      },
      {
        path: "patient",
        loadChildren: () => import('./components/patient/patient.module').then(m => m.PatientModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT"]
        },
      },
      {
        path: 'survey-form/:id',
        component: SurveyFormComponent, 
        canActivate: [AuthGuard],
        data: {
          roles: ["PATIENT", "INSURANCE", "MEDICALLEGAL"]
        },
      },
      {
        path: "family-doctor",
        loadChildren: () => import('./components/family-doctor/family-doctor.module').then(m => m.FamilyDoctorModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "FAMILYDOCTOR"]
        },
      },
      {
        path: "insurance",
        loadChildren: () => import('./components/insurance/insurance.module').then(m => m.InsuranceModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "INSURANCE"]
        },
      },
      {
        path: "medical-legal",
        loadChildren: () => import('./components/medical-legal/medical-legal.module').then(m => m.MedicalLegalModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "MEDICALLEGAL"]
        },
      },
      {
        path: "diagnostic",
        loadChildren: () => import('./components/diagnostic/diagnostic.module').then(m => m.DiagnosticModule),
        canLoad: [AuthGuard],
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "DIAGNOSTIC"]
        },
      },
      {
        path: 'view-user/:userId/:roleName',
        component: ViewUserComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN"]
        },
      },
      {
        path: 'mail/inbox',
        component: InboxComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/sent',
        component: SentComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/trash',
        component: TrashComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/compose',
        component: ComposeComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/view-mail/:page/:id',
        component: ViewMailComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/compose/:replay',
        component: ComposeComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'mail/compose/:replay/:isCompleted/:status',
        component: ComposeComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'notes',
        component: NotesComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'notes/:appointmentId',
        component: NotesComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'change-password',
        component: ChangePasswordComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'meeting/join/:title/:appointmentId/:id/:patientId/:roleName',
        component: SamvaadMeetingsComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      {
        path: 'audit-report',
        component: AuditReportComponent,
        canActivate: [AuthGuard],
        data: {
          roles: ["ADMIN", "PATIENT", "INSURANCE", "MEDICALLEGAL", "FAMILYDOCTOR", "CONSULTANT", "DIAGNOSTIC"]
        },
      },
      // {
      //   path: 'view-note/:noteId',
      //   component: ViewNoteComponent,
      //   canActivate: [AuthGuard],
      //   data: {
      //     roles: ["ADMIN", "PATIENT","INSURANCE","MEDICALLEGAL","FAMILYDOCTOR", "CONSULTANT" , "DIAGNOSTIC"]
      //   },
      // },
      {
        path: '**',
        component: ChildNotFoundComponent
      }
    ]
  },
  {
    path: '**',
    component: NotFoundComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
