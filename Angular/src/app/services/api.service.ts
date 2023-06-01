import { Injectable } from "@angular/core";
import { HttpClient, HttpRequest, HttpEvent } from "@angular/common/http";
import { HttpHeaders } from "@angular/common/http";
import { UserService } from "./user.service";
import { Router } from "@angular/router";
import { ApiConfig } from '../configs/api.config';
import { RouteConfig } from '../configs/route.config';
import { StickyBarService } from "./sticky.bar.service";
import { Observable } from 'rxjs';
import { EnvAndUrlService } from "./env-and-url.service";

@Injectable()
export class ApiService {

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

  postWithoutBearer(Url: string, body: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("post: " + this.hitUrl + " body: " + JSON.stringify(body));

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json"
      })
    };
    try {
      return this.http.post(this.hitUrl, body, options);
    } catch (e) {
      console.log("Exception ApiService postWithoutBearer " + e);
    }
  }

  getWithoutBearer(Url: string): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("get: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json"
      })
    };
    try {
      return this.http.post(this.hitUrl, options);
    } catch (e) {
      console.log("Exception ApiService getWithoutBearer " + e);
    }
  }

  postWithBearer(Url: string, body: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("post: " + this.hitUrl + " body: " + JSON.stringify(body));

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json",
        Authorization: "Bearer " + this.userService.getToken()
      })
    };
    try {
      return this.http.post(this.hitUrl, body, options);
    } catch (e) {
      console.log("Exception ApiService postWithBearer " + e);
    }
  }

  getWithBearer(Url: string): any {
    try {
      this.hitUrl = this.envAndUrlService.BASE_URL + Url;
      //console.log("get: " + this.hitUrl);

      const options = {
        headers: new HttpHeaders({
          'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
          "Content-Type": "application/json",
          Authorization: "Bearer " + this.userService.getToken()
        })
      };
      return this.http.get(this.hitUrl, options);
    } catch (e) {
      console.log("Exception ApiService getWithBearer " + e);
    }
  }

  postFormDataWithBearer(Url: string, formData: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("form post: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        Authorization: "Bearer " + this.userService.getToken()
      })
    };
    try {
      return this.http.post(this.hitUrl, formData, options);
    } catch (e) {
      console.log("Exception ApiService postFormDataWithBearer " + e);
    }
  }

  putFormDataWithBearer(Url: string, formData: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("form put: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        Authorization: "Bearer " + this.userService.getToken()
      })
    };
    try {
      return this.http.put(this.hitUrl, formData, options);
    } catch (e) {
      console.log("Exception ApiService putFormDataWithBearer " + e);
    }
  }

  putWithBearer(Url: string, body: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("put: " + this.hitUrl + " body: " + JSON.stringify(body));

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json",
        Authorization: "Bearer " + this.userService.getToken()
      })
    };
    try {
      return this.http.put(this.hitUrl, body, options);
    } catch (e) {
      console.log("Exception ApiService putWithBearer " + e);
    }
  }

  deleteWithBearer(Url: string): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + Url;
    //console.log("del: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        "Content-Type": "application/json",
        Authorization: "Bearer " + this.userService.getToken()
      })
    };
    try {
      return this.http.delete(this.hitUrl, options);
    } catch (e) {
      console.log("Exception ApiService deleteWithBearer " + e);
    }
  }

  uploadAttachment(url: any, fileSize: any, file: File, messageId: any): Observable<HttpEvent<any>> {
    this.hitUrl = this.envAndUrlService.BASE_URL + url;
    const formData: FormData = new FormData();
    formData.append('FileSize', fileSize);
    formData.append('AttachmentFile', file);
    formData.append('MessageId', messageId);
    const req = new HttpRequest('POST', this.hitUrl, formData, {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        Authorization: "Bearer " + this.userService.getToken()
      }),
      reportProgress: true
    });
    return this.http.request(req);
  }

  downloadAttachment(url: any, attachmentId: any): any {
    this.hitUrl = this.envAndUrlService.BASE_URL + url;
    const formData: FormData = new FormData();
    formData.append('AttachmenId', attachmentId);
    const req = new HttpRequest('POST', this.hitUrl, {
      headers: new HttpHeaders({
        'Access-Control-Allow-Origin': this.envAndUrlService.HOST_URL,
        Authorization: "Bearer " + this.userService.getToken()
      }),
    });
    return this.http.request(req);
  }

  /** get weather details */
  getWeather(Url: string): any {
    this.hitUrl = Url;
    //console.log("get: " + this.hitUrl);

    const options = {
      headers: new HttpHeaders({
      })
    };
    try {
      return this.http.get(this.hitUrl, options);
    } catch (e) {
      console.log("Exception ApiService getWeather " + e);
    }
  }

  catchError(err: any): void {
    try {
      if (!navigator.onLine) {
        this.stickyBarService.showErrorSticky("No internet connection.");
        return;
      }
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
