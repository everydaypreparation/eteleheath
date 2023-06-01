import { Component, OnInit, Inject } from '@angular/core';
import { StickyBarService } from '../../services/sticky.bar.service';
import { ApiConfig } from '../../configs/api.config';
import { ValidationMessages } from '../../shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { ApiService } from '../../services/api.service';
import { UserService } from '../../services/user.service';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { HelperService } from '../../services/helper.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';

export interface DialogData {
  appointmentId: number;
  doctorId: number;
  slotNotAvailableMessage: string;
}

@Component({
  selector: 'app-appointment-book-later-modal',
  templateUrl: './appointment-book-later-modal.component.html',
  styleUrls: ['./appointment-book-later-modal.component.scss']
})
export class AppointmentBookLaterModalComponent implements OnInit {

  user: any;
  availabilitySlots: any[] = [];
  loginUserId: any;
  appointmentId: any;
  doctorId: any;
  message: any;
  slotNotAvailableMessage: string = "";
  timezones: any[] = [];
  userTimezone: any = {};

  constructor(public dialogRef: MatDialogRef<AppointmentBookLaterModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private helperService: HelperService,
    private dialog: MatDialog,
    private propConfig: PropConfig,
    private router: Router,
    private routeConfig: RouteConfig) {
      this.appointmentId = data.appointmentId;
      this.doctorId = data.doctorId;
      //this.doctorId = '96f3dd07-d71f-4189-93cf-6ee5c8d70ceb';
      this.slotNotAvailableMessage = data.slotNotAvailableMessage;
    }

  ngOnInit(): void {
    this.message = "Please wait.";
    this.user = this.userService.getUser();
    if(this.user){
      this.loginUserId = this.user.id;
      this.getAvailabilitySlotByDoctorId();
    }
    this.getUserTimezoneOffset();
  }

  getAvailabilitySlotByDoctorId(): void {
    this.apiService
      .getWithBearer(this.apiConfig.getAllUnbookedAppointmentSlotbyDoctorId + this.doctorId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.availabilitySlots = res.result.items;
              // this.availabilitySlots = this.availabilitySlots.filter(s => new Date(this.formatDateTimeToUTC(s.slotStartTime)).getTime() > new Date().getTime());
              if(this.availabilitySlots.length == 0){
                this.message = " No slots available";
              }else{
                this.message = "";
              }
            }else{
              this.message = " No slots available";
            }
          } catch (e) {
            console.log("Success Exception AppointmentBookLaterModalComponent getAvailabilitySlotById " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error AppointmentBookLaterModalComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentBookLaterModalComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  confirmReschedule(slotId: any) {
    const message = ValidationMessages.confirmAppointmentBook;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        //this.getUserDetails();
        this.appointmentBook(slotId);
      }
    });
  }

  appointmentBook(slotId: any) {
    const body = {
      "slotId": slotId,
      "appointmentId": this.appointmentId
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .putWithBearer(this.apiConfig.updateAppointments, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.dialogRef.close(true);
              this.getUser();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
              this.dialogRef.close(true);
            }
          } catch (e) {
            console.log("Success Exception AppointmentBookLaterModalComponent cancelClick " + e);
          }
          this.stickyBarService.hideLoader("");
          this.dialogRef.close(true);
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentBookLaterModalComponent cancelClick " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentBookLaterModalComponent cancelClick " + e);
          }
        }
      );
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
            console.log("Success Exception AppointmentBookLaterModalComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentBookLaterModalComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentBookLaterModalComponent getUserTimezoneOffset " + e);
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

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  getUser(): void {
    const userId = localStorage.getItem("userId");
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUser + userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
            //   if (this.userService.userRole == "INSURANCE") {
            //   this.router.navigate([
            //     this.routeConfig.insuranceDashboardPath]);
            //   // this.router.navigate([
            //   //   this.routeConfig.insurancePaymentPath, this.appointmentId, this.doctorId
            //   // ]);
            // }else{
            //    this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
            //   // this.router.navigate([
            //   //   this.routeConfig.medicalLegalPaymentPath, this.appointmentId, this.doctorId
            //   // ]);
            // }
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getUser " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getUser " + e);
          }
        }
      );
  }

  getUserDetails(): void {
    const userId = localStorage.getItem("userId");
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUser + userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getUser " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getUser " + e);
          }
        }
      );
  }
}