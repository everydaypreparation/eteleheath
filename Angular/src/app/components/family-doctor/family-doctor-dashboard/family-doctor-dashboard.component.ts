import { Component, OnInit } from '@angular/core';
import { RequestDocModalComponent } from '../../request-doc-modal/request-doc-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from 'src/app/services/user.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { HelperService } from 'src/app/services/helper.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { OwlOptions } from 'ngx-owl-carousel-o';
import * as moment from "moment-timezone";
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-family-doctor-dashboard',
  templateUrl: './family-doctor-dashboard.component.html',
  styleUrls: ['./family-doctor-dashboard.component.scss']
})
export class FamilyDoctorDashboardComponent implements OnInit {

  constructor(
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private dialog: MatDialog,
    private envAndUrlService: EnvAndUrlService,
    private propConfig: PropConfig) { }
  user: any;
  loginUserId: any;

  iconUrl: string = "";
  city: string = "Toronto";
  country: string = "Canada";
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


  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.getUserTimezoneOffset();
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
              let body = res.result.insurance;
              this.city = body.city != null ? body.city : this.city;
              this.getWeather(this.city);
              this.country = body.country != null ? body.country : this.country;
              this.timezone = body.timezone != null ? body.timezone : moment.tz.guess();
              this.currentDate = moment(new Date()).tz(this.timezone).format();
            }
          } catch (e) {
            console.log("Success Exception FamilyDoctorDashboardComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error FamilyDoctorDashboardComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception FamilyDoctorDashboardComponent getUserDetailsById " + e);
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
            console.log("Success Exception FamilyDoctorDashboardComponent getWeather " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error FamilyDoctorDashboardComponent getWeather " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception FamilyDoctorDashboardComponent getWeather " + e);
          }
        }
      );
  }

  requestPatient(): void {
    const dialogRef = this.dialog.open(RequestDocModalComponent, {
      data: {userId: this.loginUserId , loginUserId: this.loginUserId, availabilitySlotId: null},
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });

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
