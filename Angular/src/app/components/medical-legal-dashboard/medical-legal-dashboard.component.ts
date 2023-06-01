import { Component, OnInit } from '@angular/core';
import { RouteConfig } from 'src/app/configs/route.config';
import { Router } from '@angular/router';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import * as moment from "moment-timezone";
import { UserService } from 'src/app/services/user.service';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-medical-legal-dashboard',
  templateUrl: './medical-legal-dashboard.component.html',
  styleUrls: ['./medical-legal-dashboard.component.scss']
})
export class MedicalLegalDashboardComponent implements OnInit {

  constructor(private router: Router,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private envAndUrlService: EnvAndUrlService,
    private routeConfig: RouteConfig) { }

  user: any;
  userId: any = null;
  subscribeUser: any;
  loginUserId: any = null;

  iconUrl: string = "";
  city: string ="Toronto";
  country: string ="Canada";
  timezone: string = "";
  temperature: string = "";
  currentDate: string = "";

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
  }

  startNewCaseClick(): void {
    this.router.navigate([
      this.routeConfig.findingConsultantPath
    ]);
  }

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result.medicalLegal;
              this.city = body.city != null ? body.city : this.city;
              this.getWeather(this.city);
              this.country = body.country != null ? body.country : this.country;
              this.timezone = body.timezone != null ? body.timezone : moment.tz.guess();
              this.currentDate = moment(new Date()).tz(this.timezone).format();
            }
          } catch (e) {
            console.log("Success Exception MedicalLegalDashboardComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error MedicalLegalDashboardComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception MedicalLegalDashboardComponent getUserDetailsById " + e);
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
            console.log("Success Exception MedicalLegalDashboardComponent getWeather " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error MedicalLegalDashboardComponent getWeather " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception MedicalLegalDashboardComponent getWeather " + e);
          }
        }
      );
  }
}
