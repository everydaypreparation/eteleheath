import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { UserService } from 'src/app/services/user.service';
import { OwlOptions } from 'ngx-owl-carousel-o';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { RescheduleAppointmentModalComponent } from 'src/app/components/reschedule-appointment-modal/reschedule-appointment-modal.component';
import { PropConfig } from 'src/app/configs/prop.config';
import { CancelAppointmentModalComponent } from '../../cancel-appointment-modal/cancel-appointment-modal.component';
import { RequestUsersModalComponent } from '../../request-users-modal/request-users-modal.component';
import * as moment from "moment-timezone";
import { NotesModelComponent } from '../../notes-model/notes-model.component';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {

  @ViewChild('activeUl', { read: ElementRef }) public activeUl: ElementRef<any>;
  @ViewChild('archiveUl', { read: ElementRef }) public archiveUl: ElementRef<any>;
  @ViewChild('missedAppointmentUl', { read: ElementRef }) public missedAppointmentUl: ElementRef<any>;
  @ViewChild('upcomingAppointmentUl', { read: ElementRef }) public upcomingAppointmentUl: ElementRef<any>;
  @ViewChild('notesUl', { read: ElementRef }) public notesUl: ElementRef<any>;
  @ViewChild('messagesUl', { read: ElementRef }) public messagesUl: ElementRef<any>;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private helperService: HelperService,
    private dialog: MatDialog,
    private envAndUrlService: EnvAndUrlService,
    private propConfig: PropConfig) { }

  user: any;

  iconUrl: string = "";
  city: string = "Toronto";
  country: string = "Canada";
  timezone: string = "";
  temperature: string = "";
  currentDate: string = "";

  selectedTab: string = "Messages";

  userId: any = null;
  subscribeUser: any;
  loginUserId: any = null;
  upcomingAppointments: any[] = [];
  upcomingAppointment: any;
  missedAppointments: any[] = [];
  activeCases: any[] = [];
  archiveCases: any[] = [];
  timezones: any[] = [];
  userTimezone: any = {};
  userEmails: any[] = [];
  notes: any[] = [];
  newPatient: any = "0";
  totalPatient: any = "0";
  dueReport: any = "0";
  notificationArr: any[] = [];
  customOptions: OwlOptions = {
    loop: true,
    mouseDrag: false,
    touchDrag: false,
    pullDrag: false,
    dots: false,
    navSpeed: 700,
    navText: ['<span class="material-icons">navigate_before</span>', '<span class="material-icons">navigate_next</span>'],
    responsive: {
      0: {
        items: 1
      },
      500: {
        items: 1
      },
      840: {
        items: 1
      },
      1040: {
        items: 1
      },
      1366: {
        items: 1
      },
      1920: {
        items: 1
      }
    },
    nav: true
  };

  apisPages: any = {
    active: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    archive: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    missedAppointment: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    upcomingAppointment: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    notes: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    userMessages: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    }
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    else {
      this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
        this.user = this.userService.getUser();
        this.loginUserId = this.user.id;
      });
    }

    this.getUserDetailsById();
    this.getUserTimezoneOffset();
    this.getMissingAppointment(false);
    this.getAllUserEmailsByUserId(this.loginUserId, false);
    this.getAllNotesByUserId(false);
    this.getActiveCases(false);
    this.getArchiveCases(false);
    this.getConsultantStats();
    this.getAllNotificationByRole();
  }

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result.consuntant;
              this.city = body.city != null ? body.city : this.city;
              this.getWeather(this.city);
              this.country = body.country != null ? body.country : this.country;
              this.timezone = body.timezone != null ? body.timezone : moment.tz.guess();
              this.currentDate = moment(new Date()).tz(this.timezone).format();
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getUserDetailsById " + e);
          }
        }
      );
  }

  getWeather(city) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWeather(this.envAndUrlService.WEATHER_API + city + "&units=imperial&appid=" + this.envAndUrlService.WEATHER_API_KEY)
      .subscribe(
        (res: any) => {
          try {
            if (res.base) {
              this.temperature = Math.round(res.main.temp) + " F";
              this.iconUrl = "http://openweathermap.org/img/w/" + res.weather[0].icon + ".png";
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getWeather " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getWeather " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getWeather " + e);
          }
        }
      );
  }

  getAllUpcomingAppointment(isConcat: boolean): void {
    let pageLimit = this.apisPages.upcomingAppointment.limit;
    let pages = (isConcat ? this.apisPages.upcomingAppointment.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllUpcomingAppointmentById + this.loginUserId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.upcomingAppointments = res.result.items;
              // this.upcomingAppointment = res.result.items[0];
              if (!isConcat) {
                this.upcomingAppointment = res.result.items[0];
                this.resetApisPage(this.apisPages.upcomingAppointment);
                this.upcomingAppointmentUl.nativeElement.scrollTop = 0;
                this.upcomingAppointments = [];
              }
              this.upcomingAppointments = [...this.upcomingAppointments, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.upcomingAppointment);
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  getMissingAppointment(isConcat: boolean): void {
    let pageLimit = this.apisPages.missedAppointment.limit;
    let pages = (isConcat ? this.apisPages.missedAppointment.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getMissedAppointments + this.loginUserId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!isConcat) {
                this.resetApisPage(this.apisPages.missedAppointment);
                this.missedAppointmentUl.nativeElement.scrollTop = 0;
                this.missedAppointments = [];
              }
              this.missedAppointments = [...this.missedAppointments, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.missedAppointment);
              // this.missedAppointments = res.result.items;
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getMissingAppointment " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getMissingAppointment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getMissingAppointment " + e);
          }
        }
      );
  }

  cancelClick(appointmentId: any) {

    const message = ValidationMessages.confirmCancelAppointment;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(CancelAppointmentModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult && dialogResult[0] == true) {
        this.cancelAppointment(appointmentId, dialogResult[1]);
      }
    });
  }

  cancelAppointment(appointmentId: any, reason: any) {
    const body = {
      "Id": appointmentId,
      "UserId": this.loginUserId,
      "Reason": reason
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.cancelAppointment, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.getAllUpcomingAppointment(false);
              this.getActiveCases(false);
              this.getMissingAppointment(false);
              this.getArchiveCases(false);
              this.getConsultantStats();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent cancelClick " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent cancelClick " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent cancelClick " + e);
          }
        }
      );
  }

  rescheduleClick(appointmentId: any, doctorId: any) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": appointmentId, "doctorId": doctorId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getAllUpcomingAppointment(false);
        this.getActiveCases(false);
        this.getMissingAppointment(false);
        this.getArchiveCases(false);
        this.getConsultantStats();
      }
    });
  }

  getAllUserEmailsByUserId(loginUserId: any, isConcat: boolean): void {
    const inboxBody = {
      "userId": loginUserId,
      "limit": this.apisPages.userMessages.limit,
      "page": isConcat ? this.apisPages.userMessages.page : 1
    };
    // this.inboxBody.userId = loginUserId;
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getAllUserEmailsByUserId, inboxBody)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.userEmails = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.userMessages);
                this.messagesUl.nativeElement.scrollTop = 0;
                this.userEmails = [];
              }
              this.userEmails = [...this.userEmails, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.userMessages);
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAllUserEmailsByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAllUserEmailsByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAllUserEmailsByUserId " + e);
          }
        }
      );
  }

  setReplyCurrentMailId(replyMailId: any, replySubject: any) {
    if (replyMailId) {
      sessionStorage.setItem('replyMailId', replyMailId);
    } else {
      sessionStorage.setItem('replyMailId', '');
    }
    if (replySubject) {
      sessionStorage.setItem('replySubject', replySubject);
    } else {
      sessionStorage.setItem('replySubject', '');
    }
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  composeMessage() {
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath]);
  }

  nextAppointmentClick(appointmentId: any) {
    this.router.navigate([
      this.routeConfig.consultantPatientDetailsPath, appointmentId
    ]);
  }

  getAllNotesByUserId(isConcat: boolean): void {
    let pageLimit = this.apisPages.notes.limit;
    let pages = (isConcat ? this.apisPages.notes.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllNotesByUserId + this.loginUserId + "&AppointmentId=" + this.envAndUrlService.UUID+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.notes = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.notes);
                this.notesUl.nativeElement.scrollTop = 0;
                this.notes = [];
              }
              this.notes = [...this.notes, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.notes);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAllNotesByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAllNotesByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAllNotesByUserId " + e);
          }
        }
      );
  }

  getActiveCases(isConcat: boolean) {
    const body = {
      "userId": this.loginUserId,
      "roleName": this.user.roleNames[0],
      "limit": this.apisPages.active.limit,
      "page": isConcat ? this.apisPages.active.page : 1
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.getActiveCases, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!isConcat) {
                this.resetApisPage(this.apisPages.active);
                this.activeUl.nativeElement.scrollTop = 0;
                this.activeCases = [];
              }
              this.activeCases = [...this.activeCases, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.active);
              // for (let activeCase of this.activeCases) {
              //   let appointmentDate = activeCase.appointmentDate.split(" ");
              //   activeCase.date = appointmentDate[0];
              //   activeCase.month = appointmentDate[1];
              // }
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getActiveCases " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getActiveCases " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getActiveCases " + e);
          }
        }
      );
  }

  getArchiveCases(isConcat: boolean) {
    const body = {
      "userId": this.loginUserId,
      "roleName": this.user.roleNames[0],
      "limit": this.apisPages.archive.limit,
      "page": isConcat ? this.apisPages.archive.page : 1
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.getArchiveCases, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!isConcat) {
                this.resetApisPage(this.apisPages.archive);
                this.archiveUl.nativeElement.scrollTop = 0;
                this.archiveCases = [];
              }
              this.archiveCases = [...this.archiveCases, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.archive);
              // for (let archiveCase of this.archiveCases) {
              //   let appointmentDate = archiveCase.appointmentDate.split(" ");
              //   archiveCase.date = appointmentDate[0];
              //   archiveCase.month = appointmentDate[1];
              // }
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getArchiveCases " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getArchiveCases " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getArchiveCases " + e);
          }
        }
      );
  }

  createReport(consultId: string) {
    this.router.navigate([
      this.routeConfig.consultantReportPath, consultId
    ]);
  }

  deleteReport(consultId: string) {
    this.stickyBarService.showLoader("");
    this.apiService
      .deleteWithBearer(this.apiConfig.deleteConsultantReport + consultId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent deleteReport " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent deleteReport " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent deleteReport " + e);
          }
        }
      );
  }

  downloadReport(consultId) {
    this.helperService.downloadReportByConsultId(consultId);
  }

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          this.getAllUpcomingAppointment(false);
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
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          this.getAllUpcomingAppointment(false);
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  formatTime24HourTo12Hour(time: any) {
    return this.helperService.formatTime24HourTo12Hour(time);
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  getConsultantStats(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getConsultantStats + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.newPatient = res.result.newPatient;
              this.dueReport = res.result.reportDue;
              this.totalPatient = res.result.totalPatient;
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getConsultantStats " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getConsultantStats " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getConsultantStats " + e);
          }
        }
      );
  }

  ngOnDestroy() {
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
  }

  gotoSamvaad(appointmentId, meetingID, Title, patientId, roleName) {
    if (Title === null || Title === undefined || Title === '') {
      Title = "Video Conference";
    }

    if (appointmentId === null || appointmentId === undefined || appointmentId === '') {
      appointmentId = this.envAndUrlService.UUID;
    }

    if (patientId === null || patientId === undefined || patientId === '') {
      patientId = this.envAndUrlService.UUID;
    }

    // this.router.navigate([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    const url = this.router.createUrlTree([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    window.open(window.location.origin + "/#" + url.toString(), '_blank');
  }

  requestDoc(): void {
    const dialogRef = this.dialog.open(RequestUsersModalComponent, {
      data: { userId: this.loginUserId, loginUserId: this.loginUserId, availabilitySlotId: null },
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });
  }

  addNote() {

    const dialogRef = this.dialog.open(NotesModelComponent, {
      data: { "loginUserId": this.loginUserId, "noteId": "", "note": "", "appointmentId": this.envAndUrlService.UUID }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotesByUserId(false);
      }
    });

  }

  onTabChange(event: MatTabChangeEvent) {
    this.selectedTab = event.tab.textLabel
  }

  getAllNotificationByRole(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getRolesNotifications + this.user.roleNames[0])
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.notificationArr = res.result.items;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAllNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAllNotification " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAllNotification " + e);
          }
        }
      );
  }

  resetApisPage(pageObj: any) {
    pageObj.page = 1;
    pageObj.count = 0;
    pageObj.more = true;
  }

  paginationHandler(result: any, pageObj: any): void {
    if (!result.items) {
      pageObj.more = false;
    }
    pageObj.count += result.items.length;
    if (result.count == 0 || (result.count == pageObj.count)) {
      pageObj.more = false;
    }
  }

  scrollHandler(e: any, type: string): void {
    if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight) {
      if (type == "active" && this.apisPages.active.more) {
        this.apisPages.active.page += 1;
        this.getActiveCases(true);
      } else if (type == "archive" && this.apisPages.archive.more) {
        this.apisPages.archive.page += 1;
        this.getArchiveCases(true);
      }else if (type == "missed" && this.apisPages.missedAppointment.more) {
        this.apisPages.missedAppointment.page += 1;
        this.getMissingAppointment(true);
      }else if (type == "upcomingAppointment" && this.apisPages.upcomingAppointment.more) {
        this.apisPages.upcomingAppointment.page += 1;
        this.getAllUpcomingAppointment(true);
      }else if (type == "notes" && this.apisPages.notes.more) {
        this.apisPages.notes.page += 1;
        this.getAllNotesByUserId(true);
      }else if (type == "messages" && this.apisPages.userMessages.more) {
        this.apisPages.userMessages.page += 1;
        this.getAllUserEmailsByUserId(this.loginUserId, true);
      }
    }
  }
}
