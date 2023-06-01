import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { HttpHeaders } from "@angular/common/http";
import { UserService } from "./user.service";
import { Router } from "@angular/router";
import { ApiConfig } from '../configs/api.config';
import { RouteConfig } from '../configs/route.config';
import { StickyBarService } from "./sticky.bar.service";
import { EnvAndUrlService } from "./env-and-url.service";

@Injectable({
  providedIn: 'root'
})
export class SamvaadmeetingsService {

  private hitUrl: string;

  constructor(
    private router: Router,
    private http: HttpClient,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private routeConfig: RouteConfig,
    private stickyBarService: StickyBarService,
    private envAndUrlService: EnvAndUrlService,
  ) { }

  joinMeeting(apiUrl: string, meetingID: string, fullName: string) {

    this.hitUrl = this.envAndUrlService.BASE_URL + apiUrl + `?meetingID=${meetingID}&fullName=${fullName}`;
    //console.log("post: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json",
        Authorization: "Bearer " + this.userService.getToken()
      })
    };

    try {
      return this.http.get(this.hitUrl, options);
    } catch (e) {
      console.log("Exception ApiService postWithBearer " + e);
    }
  }

  catchError(err: any): void {
    try {
      if (err) {
        if (err.error && err.error.error) {
          if (err.error.error.message == "Unknown Error") {
            this.stickyBarService.showErrorSticky("Something went wrong, please try again.");
          }
          else {
            this.stickyBarService.showErrorSticky(err.error.error.message);
          }
        } else if (err && err.status == 0) {
          if (err.statusText == "Unknown Error") {
            this.stickyBarService.showErrorSticky("Something went wrong, please try again.");
          }
          else {
            this.stickyBarService.showErrorSticky(err.statusText);
          }
        }
      } else {
        this.stickyBarService.showErrorSticky("Something went wrong, please try again.");
      }
    } catch (e) {
      this.stickyBarService.showErrorSticky("Something went wrong, please try again.");
    }
  }
}
