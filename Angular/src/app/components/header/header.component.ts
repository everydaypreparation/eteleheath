import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { UserService } from 'src/app/services/user.service';
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { MatMenuTrigger } from '@angular/material/menu';
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {

  @ViewChild('triggerSett') triggerSett: MatMenuTrigger;

  subscribeUser: any;
  subscribeSamvaadNavIcon: any;
  user: any = null;
  IsImpersonating: boolean = false;
  rememberMe: boolean = false;
  timezones: any[] = [];
  userTimezone: any = {};
  notifications: any = [];
  notificationId: string = "";
  noNotificationsMsg: string = "Loading...";
  notificationsLimit: number = 10;
  notificationsPage: number = 1;
  notificationsCount: any = 0;
  isShowMore: boolean = false;
  isShowSamvaadMenuIcon: boolean = false;

  constructor(
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private propConfig: PropConfig,
    private dialog: MatDialog) {
    this.user = this.userService.getUser();
    this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
      this.user = this.userService.getUser();
    });
    this.subscribeSamvaadNavIcon = this.userService.toggleSamvaadNavIcon.subscribe((data: any) => {
      this.isShowSamvaadMenuIcon = data.isShow;
    });
    this.IsImpersonating = localStorage.getItem("IsImpersonating") ? localStorage.getItem("IsImpersonating").toLowerCase() == "true" : false;
    this.rememberMe = localStorage.getItem("rememberMe") == "true" ? true : false;
  }

  ngOnInit(): void {
    this.getUserTimezoneOffset();
  }

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              let userTimezone = this.timezones.filter(t => t.timeZoneId == this.user.timezone)[0];
              if (!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId == this.propConfig.defaultTimezoneId)[0];
              }
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                .replace("UTC", "").replace(":", "");
              this.countCustomNotifications();
            }
          } catch (e) {
            console.log("Success Exception HeaderComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HeaderComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HeaderComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  countCustomNotifications() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.countCustomNotifications)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.notificationsCount = res.result.count;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception HeaderComponent countCustomNotifications " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HeaderComponent countCustomNotifications " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HeaderComponent countCustomNotifications " + e);
          }
        }
      );
  }

  getCustomNotificationClk() {
    this.notifications = [];
    this.noNotificationsMsg = "Loading...";
    this.notificationsPage = 1;
    this.isShowMore = false;
    this.getCustomNotifications(false);
    this.countCustomNotifications();
  }

  getCustomNotifications(isConcat: boolean) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getCustomNotifications + "?limit=" + this.notificationsLimit + "&page=" + this.notificationsPage)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (res.result.items.length == this.notificationsLimit) {
                this.isShowMore = true;
              } else {
                this.isShowMore = false;
              }
              if (isConcat) {
                this.notifications = [...this.notifications, ...res.result.items];
              } else {
                this.notifications = res.result.items;
              }
              if (this.notifications.length == 0) {
                this.noNotificationsMsg = "No Notification(s)";
                this.isShowMore = false;
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception HeaderComponent getCustomNotifications " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HeaderComponent getCustomNotifications " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HeaderComponent getCustomNotifications " + e);
          }
        }
      );
  }

  setCustomNotificationClk(e: any, notificationId: string) {
    e.stopPropagation();
    this.notificationId = notificationId;
    this.setCustomNotification();
  }

  setCustomNotification() {
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.setCustomNotifications, [this.notificationId])
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.countCustomNotifications();
              const obj = this.notifications.find((item: any) => item.id == this.notificationId);
              if (obj) {
                obj.state = 1;
              }
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception HeaderComponent setCustomNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            console.log("Error HeaderComponent setCustomNotification " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HeaderComponent setCustomNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        }
      );
  }

  deleteCustomNotificationClk(e: any, notificationId: string) {
    e.stopPropagation();
    this.notificationId = notificationId;
    this.deleteCustomNotification();
  }

  deleteCustomNotification() {
    this.stickyBarService.showLoader("");
    this.apiService.deleteWithBearer(this.apiConfig.deleteCustomNotifications + "?notficationId=" + this.notificationId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.countCustomNotifications();
              this.notifications = this.notifications.filter((item: any) => item.id !== this.notificationId);
              if (this.notifications.length == 0) {
                this.noNotificationsMsg = "No Notification(s)";
                this.isShowMore = false;
              }
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception HeaderComponent deleteCustomNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            console.log("Error HeaderComponent deleteCustomNotification " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HeaderComponent deleteCustomNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        }
      );
  }

  showMoreNotifications(e: any) {
    e.stopPropagation();
    this.notificationsPage += 1;
    this.getCustomNotifications(true);
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  toggleMobileNav(): void {
    this.userService.toggleMobileNavClick();
  }

  toggleSamvaadNav(): void {
    this.userService.toggleSamvaadNavClick();
  }

  closeHeaderMenu() {
    this.triggerSett.closeMenu();
  }

  profileClick(): void {
    this.closeHeaderMenu();
    const role = this.userService.userRole;
    if (role == "ADMIN") {
      this.router.navigate([
        this.routeConfig.adminProfilePath
      ]);
    } else if (role == "PATIENT") {
      this.router.navigate([
        this.routeConfig.patientProfilePath
      ]);
    } else if (role == "CONSULTANT") {
      this.router.navigate([
        this.routeConfig.consultantProfilePath
      ]);
    } else if (role == "FAMILYDOCTOR") {
      this.router.navigate([
        this.routeConfig.familyDoctorProfilePath
      ]);
    } else if (role == "INSURANCE") {
      this.router.navigate([
        this.routeConfig.insuranceProfilePath
      ]);
    } else if (role == "MEDICALLEGAL") {
      this.router.navigate([
        this.routeConfig.medicalLegalProfilePath
      ]);
    } else if (role == "DIAGNOSTIC") {
      this.router.navigate([
        this.routeConfig.diagnosticProfilePath
      ]);
    }
  }

  messagingClick() {
    this.closeHeaderMenu();
    this.router.navigate([this.routeConfig.messagingInboxPath]);
  }

  consultantreportClick() {
    this.router.navigate([this.routeConfig.consultantReportPath]);
  }

  addNotesClick() {
    this.closeHeaderMenu();
    this.router.navigate([this.routeConfig.notesPath]);
  }

  auditReportClick() {
    this.closeHeaderMenu();
    this.router.navigate([this.routeConfig.auditReportPath]);
  }

  changePasswordClick() {
    this.closeHeaderMenu();
    this.router.navigate([this.routeConfig.changePasswordPath]);
  }

  stopImpersonation() {
    this.closeHeaderMenu();
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.StopImpersonation + this.rememberMe).subscribe(
      (res: any) => {
        try {
          this.stickyBarService.hideLoader("");
          if (res.result.statusCode == 200) {

            localStorage.setItem("app-tn", res.result.accessToken);
            localStorage.IsImpersonating = false;
            localStorage.setItem("userId", localStorage.getItem("Impersonator"));
            this.router.navigate([this.routeConfig.signInPath]);

            this.stickyBarService.showSuccessSticky(res.result.message);
          } else {
            this.stickyBarService.showErrorSticky(res.result.message);
          }
        } catch (e) {
          console.log("Success Exception HeaderComponent stopImpersonation " + e);
        }
      },
      (err: any) => {
        try {
          this.stickyBarService.hideLoader("");
          console.log("Error HeaderComponent stopImpersonation " + JSON.stringify(err));
          this.apiService.catchError(err);
        } catch (e) {
          console.log("Error Exception HeaderComponent stopImpersonation " + e);
        }
      });
  }

  logoutClick(): void {
    this.closeHeaderMenu();
    const message = ValidationMessages.confirmLogout;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        if (this.userService.clearUserData()) {
          this.router.navigate([this.routeConfig.signInPath]);
        }
      }
    });
  }

  ngOnDestroy(): void {
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
    if (this.subscribeSamvaadNavIcon != null) {
      this.subscribeSamvaadNavIcon.unsubscribe();
    }
  }
}
