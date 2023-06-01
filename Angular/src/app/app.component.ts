import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { EnvAndUrlService } from './services/env-and-url.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  constructor(private router: Router, private routeConfig: RouteConfig, private envAndUrlService: EnvAndUrlService,) { }

  ngOnInit() {
    window.addEventListener('storage', (event) => {
      if (event.storageArea == localStorage) {
        //let IsImpersonating = localStorage.getItem('IsImpersonating');
        //if(IsImpersonating == undefined) { // you can update this as per your key
        // DO LOGOUT FROM THIS TAB AS WELL
        if (event["key"] == "app-tn" || event["key"] == "Impersonator" ||
          event["key"] == "IsImpersonating" || event["key"] == "userId") {
          this.router.navigate([this.routeConfig.signInPath]); // If you are using router 
        }
        // OR
        // window.location.href = '<home page URL>';
        //}
      }
    }, false);
  }
}
