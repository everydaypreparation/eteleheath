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

export interface DialogData {
  appointmentId: number;
  doctorId: number;
  slotNotAvailableMessage: string;
}

@Component({
  selector: 'app-reschedule-appointment-modal',
  templateUrl: './reschedule-appointment-modal.component.html',
  styleUrls: ['./reschedule-appointment-modal.component.scss']
})
export class RescheduleAppointmentModalComponent implements OnInit {

  user: any;
  availabilitySlots: any[] = [];
  loginUserId: any;
  appointmentId: any;
  doctorId: any;
  message: any;
  slotNotAvailableMessage: string = "";
  timezones: any[] = [];
  userTimezone: any = {};

  constructor(public dialogRef: MatDialogRef<RescheduleAppointmentModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private helperService: HelperService,
    private dialog: MatDialog,
    private propConfig: PropConfig) {
      this.appointmentId = data.appointmentId;
      this.doctorId = data.doctorId;
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
    //this.stickyBarService.showLoader("");
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
              //this.availabilitySlots = this.availabilitySlots.filter(a => !a.isBooked);
            }else{
              this.message = " No slots available";
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception RescheduleAppointmentModalComponent getAvailabilitySlotById " + e);
          }
          //this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            //this.stickyBarService.hideLoader("");
            console.log("Error RescheduleAppointmentModalComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RescheduleAppointmentModalComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  confirmReschedule(slotId: any) {
    const message = ValidationMessages.confirmRescheduleAppointment;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.rescheduleAppointment(slotId);
      }
    });
  }

  rescheduleAppointment(slotId: any) {
    const body = {
      "id": this.appointmentId,
      "doctorId": this.doctorId,
      "slotId": slotId,
      "userId": this.loginUserId
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.rescheduleAppointment, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.dialogRef.close(true);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
              this.dialogRef.close(true);
            }
          } catch (e) {
            console.log("Success Exception RescheduleAppointmentModalComponent cancelClick " + e);
          }
          this.stickyBarService.hideLoader("");
          this.dialogRef.close(true);
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RescheduleAppointmentModalComponent cancelClick " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RescheduleAppointmentModalComponent cancelClick " + e);
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
            console.log("Success Exception RescheduleAppointmentModalComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RescheduleAppointmentModalComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RescheduleAppointmentModalComponent getUserTimezoneOffset " + e);
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
}