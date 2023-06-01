import { Injectable } from '@angular/core';
import { environment } from "../../environments/environment"

@Injectable({
  providedIn: 'root'
})
export class EnvAndUrlService {

  public PAYPAL_CLIENT_ID: string = environment.PAYPAL_CLIENT_ID;
  public GOOGLE_MAP_KEY: string = environment.GOOGLE_MAP_KEY;
  public WEATHER_API: string = environment.WEATHER_API;
  public WEATHER_API_KEY: string = environment.WEATHER_API_KEY;
  public UUID: string = environment.UUID;
  public SELLER_ID: string = environment.SELLER_ID;
  public BASE_URL: string = environment.BASE_URL;
  public HOST_URL: string = environment.HOST_URL;
  public SIGNAL_R_URL: string = environment.SIGNAL_R_URL;

  constructor() {
    this.loadScript(`https://maps.googleapis.com/maps/api/js?key=${this.GOOGLE_MAP_KEY}&libraries=places`);
    this.loadScript(`https://www.paypal.com/sdk/js?client-id=${this.PAYPAL_CLIENT_ID}`);
  }

  loadScript(url: string): void {
    let fileref = document.createElement('script');
    fileref.setAttribute("type", "text/javascript");
    fileref.setAttribute("src", url);
    fileref.defer = true;
    if (typeof fileref != "undefined") {
      document.getElementsByTagName("head")[0].appendChild(fileref)
    }
  }

  loadCss(url: string): void {
    let fileref = document.createElement("link")
    fileref.setAttribute("rel", "stylesheet")
    fileref.setAttribute("type", "text/css")
    fileref.setAttribute("href", url)
    if (typeof fileref != "undefined") {
      document.getElementsByTagName("head")[0].appendChild(fileref)
    }
  }
}
