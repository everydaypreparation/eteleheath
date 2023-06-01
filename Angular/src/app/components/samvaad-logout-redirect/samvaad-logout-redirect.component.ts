import { Component, OnInit } from '@angular/core';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';
import { ApiConfig } from '../../configs/api.config';

@Component({
  selector: 'app-samvaad-logout-redirect',
  templateUrl: './samvaad-logout-redirect.component.html',
  styleUrls: ['./samvaad-logout-redirect.component.scss']
})
export class SamvaadLogoutRedirectComponent implements OnInit {

  constructor(
    private envAndUrlService: EnvAndUrlService,
  ) { }

  ngOnInit(): void {
    window.parent.location.href = this.envAndUrlService.HOST_URL;
  }
}

