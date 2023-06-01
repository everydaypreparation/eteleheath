import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { RouteConfig } from 'src/app/configs/route.config';
import { UserService } from 'src/app/services/user.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { AppointmentSlotModelComponent } from '../appointment-slot-model/appointment-slot-model.component';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';
import { DomSanitizer } from '@angular/platform-browser';
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-view-user',
  templateUrl: './view-user.component.html',
  styleUrls: ['./view-user.component.scss']
})
export class ViewUserComponent implements OnInit, OnDestroy {

  constructor(private activatedRoute: ActivatedRoute,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private dialog: MatDialog,
    private appointmentSlotService: AppointmentSlotService,
    private helperService: HelperService,
    private sanitizer: DomSanitizer,
    private propConfig: PropConfig) {
    this.userService.setNav("viewuser");
  }

  userId: any = null;
  user: any = null;
  roleName: any;
  subscribeUser: any;
  loginUserId: any;
  availabilitySlotId: any;
  availabilitySlots: any[] = [];
  profileUrl: any;
  timezones: any[] = [];
  userTimezone: any = {};

  ngOnInit(): void {
    this.userId = this.activatedRoute.snapshot.params['userId'];
    this.roleName = this.activatedRoute.snapshot.params['roleName'];
    if (this.userId) {
      this.getUserDetailsById();
    }

    let user = this.userService.getUser();
    if (user) {
      this.loginUserId = user.id;
    }
    else {
      this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
        let user = this.userService.getUser();
        this.loginUserId = user.id;
      });
    }
    this.getUserTimezoneOffset();
    this.getAvailabilitySlotById();
  }

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if(this.roleName =='consultant'){
                this.user = res.result.consuntant;
              }else if(this.roleName =='patient'){
                this.user = res.result.patient;
              }else if(this.roleName =='familydoctor'){
                this.user = res.result.familyDoctor;
              }else if(this.roleName =='diagnostic'){
                this.user = res.result.daignostic;
              }else if(this.roleName =='insurance'){
                this.user = res.result.insurance;
              }else if(this.roleName =='medicallegal'){
                this.user = res.result.medicalLegal;
              }
              // this.user = res.result;
              if (this.user.uploadProfilePicture) {
                if(this.user.uploadProfilePicture.includes(this.apiConfig.matchesUrl)){
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.user.uploadProfilePicture}`);
                 }else{
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.user.uploadProfilePicture}`);
                 }
                //this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.user.uploadProfilePicture}`);
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ViewPatientDetailComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewPatientDetailComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewPatientDetailComponent getUserDetailsById " + e);
          }
        }
      );
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.adminDashboardPath
    ]);
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
              for(let availabilitySlot of this.availabilitySlots) {
                let timezone = this.timezones.filter(t => t.timeZoneId==availabilitySlot.timeZone)[0];
                if(timezone) {
                  availabilitySlot.abbr = timezone.abbr;
                }
              }
              // this.availabilitySlots = this.availabilitySlots.filter(s => new Date(this.formatDateTimeToUTC(s.slotStartTime)).getTime() > new Date().getTime());
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);

            }
          } catch (e) {
            console.log("Success Exception ViewUserComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewUserComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewUserComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  addAvailabilitySlot() {
    const dialogRef = this.dialog.open(AppointmentSlotModelComponent, {
      data: { userId: this.userId, loginUserId: this.loginUserId, availabilitySlotId: null }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getAvailabilitySlotById();
      }
    });
  }

  editvailabilitySlot(slotId: string) {
    
    const dialogRef = this.dialog.open(AppointmentSlotModelComponent, {
      data: { userId: this.userId, loginUserId: this.loginUserId, availabilitySlotId: slotId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getAvailabilitySlotById();
      }
    });
  }

  // editvailabilitySlot(slotId: string) {
  //   const message = ValidationMessages.editAvalabilitySlotConfirmation;

  //   const dialogData = new ConfirmModel("Confirm Action", message);

  //   const dialogRef = this.dialog.open(ConfirmModalComponent, {
  //     maxWidth: "800px",
  //     data: dialogData
  //   });

  //   dialogRef.afterClosed().subscribe(dialogResult => {
  //     if (dialogResult == true) {
  //       const dialogRef1 = this.dialog.open(AppointmentSlotModelComponent, {
  //         data: { userId: this.userId, loginUserId: this.loginUserId, availabilitySlotId: slotId }
  //       });
    
  //       dialogRef1.afterClosed().subscribe(dialogResult1 => {
  //         if (dialogResult1 == true) {
  //           this.getAvailabilitySlotById();
  //         }
  //         console.log(dialogResult1);
  //       });
  //     }
  //   });
  // }

  deleteAvailabilitySlot(slotId: string) {
    const message = ValidationMessages.deleteAvalabilitySlotConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        console.log(`Dialog result: ${dialogResult}`);
        this.stickyBarService.showLoader("");
        this.appointmentSlotService
          .deleteAvailabilitySlotWithBearer(this.apiConfig.deleteAppointmentSlotbyId + slotId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAvailabilitySlotById();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception AppointmentSlotModelComponent deleteUserDetail " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error AppointmentSlotModelComponent deleteUserDetail " + JSON.stringify(err));
                this.appointmentSlotService.catchError(err);
              } catch (e) {
                console.log("Error Exception AppointmentSlotModelComponent deleteUserDetail " + e);
              }
            });
      } else {
        console.log(`Cancle Dialog result: ${dialogResult}`);
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
            console.log("Success Exception ViewUserComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewUserComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewUserComponent getUserTimezoneOffset " + e);
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