import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';

@Component({
  selector: 'app-child-not-found',
  templateUrl: './child-not-found.component.html',
  styleUrls: ['./child-not-found.component.scss']
})
export class ChildNotFoundComponent implements OnInit {

  constructor(private router: Router,
    private routeConfig: RouteConfig,) { }

  ngOnInit(): void {
  }

  loginClk(): void {
    this.router.navigate([this.routeConfig.signInPath]);
  }

}
