import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatDialog } from '@angular/material/dialog';
import { AddPatientModalComponent } from '../../add-patient-modal/add-patient-modal.component';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import * as moment from "moment-timezone";
import { OwlOptions } from 'ngx-owl-carousel-o';
import { PropConfig } from 'src/app/configs/prop.config';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  showEmptyDashboard: boolean = true;

  user: any;
  userId: any = null;
  subscribeUser: any;
  loginUserId: any = null;
  showContent:boolean = false;
  iconUrl: string = "";
  city: string ="Toronto";
  country: string ="Canada";
  timezone: string = "";
  temperature: string = "";
  currentDate: string = "";
  notificationArr: any[] = [];
  userTimezone: any = {};
  timezones: any[] = [];
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

  constructor(private router: Router, 
    private routeConfig: RouteConfig, 
    private dialog: MatDialog,
    private stickyBarService: StickyBarService,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private userService: UserService,
    private envAndUrlService: EnvAndUrlService,
    private propConfig: PropConfig
    ) { }

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
    this.getActivatePatient();
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

  showDashboard(){
    this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
  }

  AddPatient() {
    const dialogRef = this.dialog.open(AddPatientModalComponent, {
      /*data: { "noteId": "No" }*/
    });
  }

  getActivatePatient() {
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.activePatient, { "userId": this.loginUserId })
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              if (res.result.items) {
                this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
              }
              else{
                this.getArchivePatient();
              }
            } else {
              this.showContent = true;
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            this.showContent = true;
            console.log("Success Exception DashboardComponent getActivatePatient " + e);
          }
        },
        (err: any) => {
          try {
            this.showContent = true;
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getActivatePatient " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getActivatePatient " + e);
          }
        }
      );
  }

  getArchivePatient() {
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.archivePatient, { "userId": this.loginUserId })
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              if (res.result.items) {
                this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
              }else{
                this.showContent = true;
              }
            } else {
              this.showContent = true;
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            this.showContent = true;
            console.log("Success Exception DashboardComponent getArchivePatient " + e);
          }
        },
        (err: any) => {
          try {
            this.showContent = true;
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getArchivePatient " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getArchivePatient " + e);
          }
        }
      );
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

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
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

}
