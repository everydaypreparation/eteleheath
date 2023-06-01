import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';

@Component({
  selector: 'app-admin-child-app',
  templateUrl: './admin-child-app.component.html',
  styleUrls: ['./admin-child-app.component.scss']
})
export class AdminChildAppComponent implements OnInit {

  mobileNav: any = {
    isMobileNav: false,
    showMobileNav: false
  };
  subscribeNav: any;
  subscribeMobileNav: any;
  menuItems: any = [
    {
      code: "dashboard",
      title: "Dashboard",
      icon: "flaticon-web-page",
      path: this.routeConfig.adminDashboardPath,
      cls: ""
    },
    {
      code: "consultants",
      title: "Consultants",
      icon: "flaticon-doctor",
      path: this.routeConfig.adminConsultantsPath,
      cls: ""
    },
    {
      code: "patients",
      title: "Patients",
      icon: "flaticon-patient",
      path: this.routeConfig.adminPatientsPath,
      cls: ""
    },
    {
      code: "familydoctors",
      title: "Family Doctors",
      icon: "flaticon-doctor",
      path: this.routeConfig.adminFamilyDoctorsPath,
      cls: ""
    },
    {
      code: "diagnostics",
      title: "Diagnostics",
      icon: "flaticon-diagnostic",
      path: this.routeConfig.adminDiagnosticsPath,
      cls: ""
    },
    {
      code: "insurances",
      title: "Insurances",
      icon: "flaticon-healthcare",
      path: this.routeConfig.adminInsurancesPath,
      cls: ""
    },
    {
      code: "medicallegals",
      title: "Medical Legals",
      icon: "flaticon-pharmacy",
      path: this.routeConfig.adminMedicalLegalsPath,
      cls: ""
    },
    {
      code: "requestDoctor",
      title: "Invite/Referrals",
      icon: "flaticon-doctor",
      path: this.routeConfig.adminRequestDoctorsPath,
      cls: ""
    },
    {
      code: "costConfiguration",
      title: "Cost Configuration",
      icon: "flaticon-doctor",
      path: this.routeConfig.costConfigurationPath,
      cls: ""
    },
    {
      code: "surveyfeedbacks",
      title: "Survey Feedbacks",
      icon: "flaticon-diagnostic",
      path: this.routeConfig.surveyFeedbacksPath,
      cls: ""
    },
    {
      code: "addNotification",
      title: "Send Notification",
      icon: "flaticon-doctor",
      path: this.routeConfig.addNotificationPath,
      cls: ""
    },
    {
      code: "emroadmins",
      title: "ETeleHealth Admins",
      icon: "flaticon-user",
      path: this.routeConfig.adminEmroAdminsPath,
      cls: ""
    },
    {
      code: "systemmonitoring",
      title: "System Monitoring",
      icon: "flaticon-cloud-service",
      path: this.routeConfig.adminSystemMonitoringPath,
      cls: ""
    },
    {
      code: "consentforms",
      title: "Consent Forms",
      icon: "flaticon-edit",
      path: this.routeConfig.adminConsentFormsPath,
      cls: ""
    }
    // {
    //   code: "Messaging",
    //   title: "Messaging",
    //   icon: "flaticon-cloud-service",
    //   path: this.routeConfig.adminMessagingPath,
    //   cls: ""
    // }
  ];

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private validationService: ValidationService,
    private changeDetectorRef: ChangeDetectorRef,
    private userService: UserService,) {
    this.subscribeNav = this.userService.changeNav.subscribe((data: any) => {
      this.menuItems.forEach((item: any, index: number) => {
        item.cls = "";
        if (data == item.code) {
          item.cls = "side-menu--active";
        }
      });
    });
    this.subscribeNav = this.userService.toggleMobileNav.subscribe((data: any) => {
      this.mobileNav.showMobileNav = !this.mobileNav.showMobileNav;
    });
    if (window.innerWidth <= 768) {
      this.mobileNav.isMobileNav = true;
    }
  }

  ngOnInit(): void {
  }

  onResize(event: any) {
    if (event.target.innerWidth <= 768) {
      this.mobileNav.isMobileNav = true;
      this.mobileNav.showMobileNav = false;
    } else {
      this.mobileNav.isMobileNav = false;
      this.mobileNav.showMobileNav = false;
    }
  }

  changePath(path: string): void {
    this.mobileNav.showMobileNav = false;
    this.router.navigate([path]);
  }

  ngOnDestroy(): void {
    if (this.subscribeNav != null) {
      this.subscribeNav.unsubscribe();
    }
    if (this.subscribeMobileNav != null) {
      this.subscribeMobileNav.unsubscribe();
    }
  }
}
