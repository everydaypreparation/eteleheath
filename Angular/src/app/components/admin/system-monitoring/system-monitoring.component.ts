import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-system-monitoring',
  templateUrl: './system-monitoring.component.html',
  styleUrls: ['./system-monitoring.component.scss']
})
export class SystemMonitoringComponent implements OnInit {

  constructor(private userService: UserService,) { 
    this.userService.setNav("systemmonitoring");
  }

  ngOnInit(): void {
  }

}
