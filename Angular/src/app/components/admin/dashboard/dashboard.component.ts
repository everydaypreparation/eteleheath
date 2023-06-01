import { Component, OnInit, ChangeDetectorRef, ViewChild, OnDestroy, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { HelperService } from 'src/app/services/helper.service';
import { PropConfig } from 'src/app/configs/prop.config';
import * as moment from "moment-timezone";
import { OwlOptions } from 'ngx-owl-carousel-o';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { v4 as uuid } from 'uuid';
declare const $: any;

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit , OnDestroy{
  @ViewChild('messagesUl', { read: ElementRef }) public messagesUl: ElementRef<any>;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private userService: UserService,
    private apiConfig: ApiConfig,
    private dialog: MatDialog,
    private propConfig: PropConfig,
    private envAndUrlService: EnvAndUrlService,
    private stickyBarService: StickyBarService) {
    this.userService.setNav("dashboard");
  }


  composeBody = {
    "subject": "",
    "messagesText": "",
    "senderUserIds": "",
    "receiverUserIds": "",
    "parentId": "",
    "messageId": "",
    "appointmentId": ""
  };

  iconUrl: string = "";
  city: string ="Toronto";
  country: string ="Canada";
  timezone: string = "";
  temperature: string = "";
  currentDate: string = "";

  patientCount: any = "0";
  consultantCount: any = "0";
  familyDoctorCount: any = "0";
  diagnosticCount: any = "0";
  insuranceCount: any = "0";
  medicalLegalCount: any = "0";

  replay: any;
  userEmailIdList: any[];
  dropdownEmailsSettings: any;
  selectedEmailItems: Array<any> = [];
  user: any;
  userEmails: any[] = [];
  loginUserId: any;
  subscribeUser: any;
  inboxBody = {
    "userId": ""
  };
  timezones: any[] = [];
  userTimezone: any = {};
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

    this.dropdownEmailsSettings = {
      singleSelection: false,
      idField: 'emailId',
      textField: 'fullName',
      selectAllText: 'All',
      unSelectAllText: 'All',
      itemsShowLimit: 1,
      allowSearchFilter: true,
    };
    this.getAllUserEmails();
    this.getUserDetailsById();
    this.getAllUserEmailsByUserId(this.loginUserId, false);
    this.getUserTimezoneOffset();
    this.getAllNotificationByRole();
    this.getDashboardStats();
  }

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result.daignostic;
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

  getAllUserEmailsByUserId(loginUserId: any, isConcat: boolean): void {
    //this.inboxBody.userId = loginUserId;
    const inboxBody = {
      "userId": loginUserId,
      "limit": this.apisPages.userMessages.limit,
      "page": isConcat ? this.apisPages.userMessages.page : 1
    };
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getAllUserEmailsByUserId, inboxBody)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.userEmails = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.userMessages);
                this.messagesUl.nativeElement.scrollTop = 0;
                this.userEmails = [];
              }
              this.userEmails = [...this.userEmails, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.userMessages);
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
  

  composeMessage() {
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath]);
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

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              let userTimezone = this.timezones.filter(t => t.timeZoneId==this.user.timezone)[0];
              if(!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId==this.propConfig.defaultTimezoneId)[0];
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

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  getAllNotificationByRole(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getRolesNotifications +this.user.roleNames[0])
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

  getDashboardStats(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDashBoardCount + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.patientCount = res.result.patient;
              this.consultantCount = res.result.consultant;
              this.familyDoctorCount = res.result.familyDoctor;
              this.diagnosticCount = res.result.diagnostic;
              this.insuranceCount = res.result.insurance;
              this.medicalLegalCount = res.result.medicalLegal;
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getDashboardStats " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getDashboardStats " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getDashboardStats " + e);
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
     if (type == "messages" && this.apisPages.userMessages.more) {
        this.apisPages.userMessages.page += 1;
        this.getAllUserEmailsByUserId(this.loginUserId, true);
      }
    }
  }

  ngOnDestroy() {
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
  }

  onSelectAll(items: any) {
    if (this.userService.userRole) {
      const role = this.userService.userRole;
      if (role == "ADMIN") {
        const message = ValidationMessages.selectAllUsers;
        const dialogData = new ConfirmModel("", message);
        const dialogRef = this.dialog.open(ConfirmModalComponent, {
          maxWidth: "800px",
          data: dialogData
        });

        dialogRef.afterClosed().subscribe(dialogResult => {
          if (dialogResult == true) {
          } else {
            this.selectedEmailItems = [];
          }
        });
      }
    }
  }

  getAllUserEmails(): void {
    this.apiService.getWithBearer(this.apiConfig.getUserEmails + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userEmailIdList = res.result.items;
              if (this.replay == 'replay') {
                let replyMailId = sessionStorage.getItem('replyMailId');
                let replySubject = sessionStorage.getItem('replySubject');
                if (replyMailId) {
                  //this.selectedEmailItems = replyMailId.split(',');
                  this.selectedEmailItems = this.userEmailIdList.filter(i => replyMailId.split(',').includes(i.emailId));
                  if (this.selectedEmailItems.length == 0) {
                    this.selectedEmailItems = this.userEmailIdList.filter(i => replyMailId.split(',').includes(i.id));
                  }
                }
                if (replySubject) {
                  //$('#userSubject').val('Re: ' + replySubject);
                  this.composeBody.subject = replySubject.replace('(no subject)', '');
                }
              } else {
                this.selectedEmailItems = [];
                $('#userSubject').val('');
              }
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ComposeComponent getAllUserEmails " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ComposeComponent getAllUserEmails" + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ComposeComponent getAllUserEmails" + e);
          }
        }
      );
  }

  gotoSamvaad(Title: string = "Video Conference", roleName: string = "Admin") {

    if (this.selectedEmailItems.length > 0) {
      const appointmentId = this.envAndUrlService.UUID;
      const patientId = this.envAndUrlService.UUID;
      const meetingID = uuid();
      this.stickyBarService.showLoader("");

      // this.router.navigate([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
      const url = this.router.createUrlTree([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);

      this.apiService
        .postWithBearer(this.apiConfig.SendSamvaadEmail + url.toString(), this.selectedEmailItems)
        .subscribe(
          (res: any) => {
            try {
              this.stickyBarService.hideLoader("");
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
                window.open(window.location.origin + "/#" + url.toString(), '_blank');
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception DashboardComponent gotoSamvaad " + e);
            }
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error DashboardComponent gotoSamvaad " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception DashboardComponent gotoSamvaad " + e);
            }
          }
        );
    }
    else {
      this.stickyBarService.showErrorSticky("Please select at least one user for meeting.");
    }
  }
}
