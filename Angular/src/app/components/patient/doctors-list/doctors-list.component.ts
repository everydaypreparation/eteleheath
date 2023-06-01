import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { RequestDocModalComponent } from '../../request-doc-modal/request-doc-modal.component';
import { RequestUsersModalComponent } from '../../request-users-modal/request-users-modal.component';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-doctors-list',
  templateUrl: './doctors-list.component.html',
  styleUrls: ['./doctors-list.component.scss']
})
export class DoctorsListComponent implements OnInit {

  user: any;
  specialties: any[];
  specialtyName: string = "";
  consultants: any[];
  errMessage: string = "";
  minDate = new Date();
  loginUserId: any = null;
  timezones: any[] = [];
  userTimezone: any = {};
  doctorProfileUrl: any[] = [];

  roleName = 'consultant';

  searchForm: any = {
    "specialtyName": "",
    "consultantName": "",
    "hospitalName": "",
    "nextAvailability": "",
    "timeZone": ""
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private activatedRoute: ActivatedRoute,
    private dialog: MatDialog,
    private helperService: HelperService,
    private userService: UserService,
    private propConfig: PropConfig,
    private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.activatedRoute.queryParams.subscribe(queryParams => {
      this.specialtyName = "";
      this.searchForm.consultantName = "";
      this.searchForm.hospitalName = "";
      this.searchForm.nextAvailability = "";
      if (queryParams.hasOwnProperty("specialtyName")) {
        this.specialtyName = queryParams.specialtyName;
      }
      if (queryParams.hasOwnProperty("consultantName")) {
        this.searchForm.consultantName = queryParams.consultantName;
      }
      if (queryParams.hasOwnProperty("hospitalName")) {
        this.searchForm.hospitalName = queryParams.hospitalName;
      }
      if (queryParams.hasOwnProperty("nextAvailability")) {
        this.searchForm.nextAvailability = new Date(queryParams.nextAvailability.toString());
      }
      this.searchForm.specialtyName = this.specialtyName;
      this.searchForm.timeZone = this.user.timezone;
      this.getSpecialties();
      this.getUserTimezoneOffset();
    });
  }

  searchConsultantClk(): void {
    const body = { ...this.searchForm };
    if (this.searchForm.nextAvailability && this.searchForm.nextAvailability != 'null') {
      body.nextAvailability = new DatePipe('en-US').transform(this.searchForm.nextAvailability, 'MM/dd/yyyy');
    }
    const basePath = this.routeConfig.patientDoctorListPath;
    const queryParamsObj: any = {};
    if (body.specialtyName) {
      queryParamsObj.specialtyName = body.specialtyName;
    }
    if (body.consultantName) {
      queryParamsObj.consultantName = body.consultantName;
    }
    if (body.hospitalName) {
      queryParamsObj.hospitalName = body.hospitalName;
    }
    if (body.nextAvailability) {
      queryParamsObj.nextAvailability = body.nextAvailability;
    }
    queryParamsObj.ran = Math.floor(100000 + Math.random() * 900000);
    if (Object.keys(queryParamsObj).length > 0) {
      this.router.navigate([basePath], { queryParams: queryParamsObj });
    } else {
      this.router.navigate([basePath]);
    }
  }

  searchConsultant(): void {
    const body = { ...this.searchForm };
    this.stickyBarService.showLoader("");
    if (this.searchForm.nextAvailability && this.searchForm.nextAvailability != 'null') {
      body.nextAvailability = new DatePipe('en-US').transform(this.searchForm.nextAvailability, 'MM/dd/yyyy');
    } else {
      body.nextAvailability = '';
    }
    this.apiService
      .postWithBearer(this.apiConfig.searchConsultant, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.consultants = res.result.items;
              if (this.consultants.length > 0) {
                for (let profile of res.result.items) {
                  if (profile.profileUrl) {
                    if (profile.profileUrl.includes(this.apiConfig.matchesUrl)) {
                      profile.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${profile.profileUrl}`);
                    } else {
                      profile.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${profile.profileUrl}`);
                    }
                  }
                }
              }
              this.errMessage = ValidationMessages.NoDoctorsResultsFound;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DoctorsListComponent searchConsultantForPatient " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorsListComponent searchConsultantForPatient " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorsListComponent searchConsultantForPatient " + e);
          }
        }
      );
  }

  getSpecialties(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getSpecialties)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.specialties = res.result.items;
              this.changeDetectorRef.detectChanges();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DoctorsListComponent getSpecialties " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorsListComponent getSpecialties " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorsListComponent getSpecialties " + e);
          }
        }
      );
  }

  viewDoctorDetail(userId: string) {
    this.router.navigate([
      this.routeConfig.patientDoctorDetailsPath, userId
    ]);
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
              let userTimezone = this.timezones.filter(t => t.timeZoneId == this.user.timezone)[0];
              if (!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId == this.propConfig.defaultTimezoneId)[0];
              }
              this.searchForm.timeZone = userTimezone.timeZoneId;
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                .replace("UTC", "").replace(":", "");
              this.searchConsultant();
            }
          } catch (e) {
            console.log("Success Exception DoctorsListComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorsListComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorsListComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  formatTime24HourTo12Hour(time: any) {
    return this.helperService.formatTime24HourTo12Hour(time);
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  clearNextAvailability(event: any) {
    event.stopPropagation();
    this.searchForm.nextAvailability = null;
  }

  homeClick(): void {
    // this.router.navigate([this.routeConfig.patientDashboardPath]);
    if(!this.user.isPayment && this.user.isIntake && !this.user.isAppointment && this.user.isAllowtoNewBooking && !this.user.isMissedAppointment){
      this.router.navigate([this.routeConfig.findingConsultantPath]);
    }else{
    this.router.navigate([
      this.routeConfig.patientDashboardPath
    ]);
  }
  }
  requestDoc(): void {
    const dialogRef = this.dialog.open(RequestUsersModalComponent, {
      data: { userId: this.loginUserId, loginUserId: this.loginUserId, availabilitySlotId: null },
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });
  }
}
