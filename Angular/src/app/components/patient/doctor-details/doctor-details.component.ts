import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { UserService } from 'src/app/services/user.service';
import { MatDialog } from '@angular/material/dialog';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { AppointmentBookModelComponent } from '../../appointment-book-model/appointment-book-model.component';
import { HelperService } from 'src/app/services/helper.service';
import { DatePipe } from '@angular/common';
import { PropConfig } from 'src/app/configs/prop.config';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-doctor-details',
  templateUrl: './doctor-details.component.html',
  styleUrls: ['./doctor-details.component.scss']
})
export class DoctorDetailsComponent implements OnInit, OnDestroy {

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private activatedRoute: ActivatedRoute,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private dialog: MatDialog,
    private appointmentSlotService: AppointmentSlotService,
    private helperService: HelperService,
    private propConfig: PropConfig,
    private sanitizer: DomSanitizer
  ) { }

  user: any;
  doctor: any;
  availabilitySlots: any[] = [];
  userId: any = null;
  subscribeUser: any;
  loginUserId: any = null;
  roleName: any;
  subSpecialtiesArray = [];
  timezones: any[] = [];
  userTimezone: any = {};
  doctorProfileUrl: any;

  ngOnInit(): void {
    this.userId = this.activatedRoute.snapshot.params['userId'];
    this.user = this.userService.getUser();
    if (this.userId) {
      this.getUserDetailsById();
    }
    if(this.user){
    this.loginUserId = this.user.id;
    }
    // else{
    //   this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
    //     this.user = this.userService.getUser();
    //     this.loginUserId = this.user.id;
    //   });
    // }
    this.getAvailabilitySlotById();
    this.getUserTimezoneOffset();
  }
  
  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.doctor = res.result.consuntant;
              if (this.doctor.uploadProfilePicture) {
                if(this.doctor.uploadProfilePicture.includes(this.apiConfig.matchesUrl)){
                  this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.doctor.uploadProfilePicture}`);
                 }else{
                  this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.doctor.uploadProfilePicture}`);
                 }
                //this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.doctor.uploadProfilePicture}`);
              }
              if(this.doctor && this.doctor.oncologySubSpecialty){
              this.subSpecialtiesArray = this.doctor.oncologySubSpecialty;
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DoctorDetailsComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorDetailsComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorDetailsComponent getUserDetailsById " + e);
          }
        }
      );
  }

  homeClick(): void {
    if(!this.user.isPayment && this.user.isIntake && !this.user.isAppointment && this.user.isAllowtoNewBooking && !this.user.isMissedAppointment){
      this.router.navigate([this.routeConfig.findingConsultantPath]);
    }else{
    this.router.navigate([
      this.routeConfig.patientDashboardPath
    ]);
  }
  }

  getAvailabilitySlotById(): void {
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .getAvailabilitySlotWithBearer(this.apiConfig.getAllAppointmentSlotbyDoctorId + this.userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.availabilitySlots = res.result.items;
              // this.availabilitySlots = this.availabilitySlots.filter(s => new Date(this.formatDateTimeToUTC(s.slotStartTime)).getTime() > new Date().getTime());
            }else{
              //this.stickyBarService.showErrorSticky(res.result.message);

            }
          } catch (e) {
            console.log("Success Exception DoctorDetailsComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorDetailsComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorDetailsComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  showAvailabilityBookSlotModel(slotId: any, userId: any){
    const dialogRef = this.dialog.open(AppointmentBookModelComponent, {
      data: {id: slotId , doctorId: userId, loginUser: this.loginUserId}
    });
    dialogRef.afterClosed().subscribe(dialogResult => {
      if(dialogResult == true){
      this.getAvailabilitySlotById();
      }
    });
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
              let userTimezone = this.timezones.filter(t => t.timeZoneId==this.user.timezone)[0];
              if(!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId==this.propConfig.defaultTimezoneId)[0];
              }
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                                                                .replace("UTC", "").replace(":", "");
            }
          } catch (e) {
            console.log("Success Exception DoctorDetailsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DoctorDetailsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DoctorDetailsComponent getUserTimezoneOffset " + e);
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

  ngOnDestroy(){
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
  }
}
