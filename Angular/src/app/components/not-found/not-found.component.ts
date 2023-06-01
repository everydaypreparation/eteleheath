import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss']
})
export class NotFoundComponent implements OnInit {

  constructor(
    private router: Router,
    private routeConfig: RouteConfig
  ) { }

  ngOnInit(): void {
  }

  loginClk(): void {
    this.router.navigate([this.routeConfig.signInPath]);
  }

}
