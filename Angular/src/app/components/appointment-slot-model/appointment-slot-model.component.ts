import { Component, OnInit, Inject, OnDestroy, Injectable, Optional } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { NgbTimeStruct, NgbTimeAdapter } from '@ng-bootstrap/ng-bootstrap';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';
import { DatePipe } from '@angular/common';
import { ApiService } from 'src/app/services/api.service';
import * as Moment from 'moment';
import { extendMoment } from 'moment-range';

const moment = extendMoment(Moment);

export interface DialogData {
  userId: string;
  loginUserId: string;
  availabilitySlotId: string;
}

@Component({
  selector: 'app-appointment-slot-model',
  templateUrl: './appointment-slot-model.component.html',
  styleUrls: ['./appointment-slot-model.component.scss']
})
export class AppointmentSlotModelComponent implements OnInit {

  hoursDD: any[] = [
    { value: 0, text: "00" },
    { value: 1, text: "01" },
    { value: 2, text: "02" },
    { value: 3, text: "03" },
    { value: 4, text: "04" },
    { value: 5, text: "05" },
    { value: 6, text: "06" },
    { value: 7, text: "07" },
    { value: 8, text: "08" },
    { value: 9, text: "09" },
    { value: 10, text: "10" },
    { value: 11, text: "11" },
    { value: 12, text: "12" }
  ];
  minutesDD: any[] = [
    { value: 0, text: "00" },
    { value: 15, text: "15" },
    { value: 30, text: "30" },
    { value: 45, text: "45" }
  ];

  availabilitySlotForm: FormGroup;
  rows: FormArray;
  meridian = true;
  userId: any = null;
  loginUserId: any = null;
  availabilitySlotId: any = null;
  editFlag = false;
  errorAvailabilityDateMsg: any;
  minDate = new Date();
  timezones = [];
  filteredTimezones = [];
  selectedTimezoneId: any;
  selectedTimezoneAbbr: any;
  timezoneErrorMsg: string = "";
  overlapErrorMsg: string = "";

  constructor(public dialogRef: MatDialogRef<AppointmentSlotModelComponent>, private fb: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private stickyBarService: StickyBarService,
    private appointmentSlotService: AppointmentSlotService, private apiConfig: ApiConfig
    , private dialog: MatDialog, private helperService: HelperService, private apiService: ApiService) {

    this.availabilitySlotForm = this.fb.group({
      items: [null, Validators.required],
      items_value: ['no', Validators.required]
    });

    this.rows = this.fb.array([]);
    this.userId = data.userId;
    this.loginUserId = data.loginUserId;
    this.availabilitySlotId = data.availabilitySlotId;
  }

  ngOnInit(): void {
    this.getTimezoneList();
    this.availabilitySlotForm.get("items_value").setValue("yes");
    this.availabilitySlotForm.addControl('rows', this.rows);
  }

  getTimezoneList() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              this.filteredTimezones = [... this.timezones];
              if (this.availabilitySlotId) {
                this.editFlag = true;
                this.getAppointmentSlotbyId();
              } else {
                this.editFlag = false;
                this.rows.push(this.availabilityFormGroup());
              }
            }
          } catch (e) {
            console.log("Success Exception AppointmentSlotModelComponent getTimezoneList " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentSlotModelComponent getTimezoneList " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentSlotModelComponent getTimezoneList " + e);
          }
        }
      );
  }

  selectTimezone(timeZoneId: any) {
    this.timezoneErrorMsg = "";
    this.selectedTimezoneAbbr = this.timezones.filter(t => t.timeZoneId == timeZoneId)[0].abbr;
  }

  createAvailabilitySlot(): void {
    // Close the dialog, return true
    if (!this.selectedTimezoneId) {
      this.timezoneErrorMsg = ValidationMessages.allFieldsRequired;
      return;
    }
    let i = 0;
    for (i = 0; i < this.rows['controls'].length; i++) {
      const row: any = this.rows['controls'][i];
      if (!this.rows.value[i].availabilityDate || this.isNumberNull(this.rows.value[i].availabilityStartHour) || this.isNumberNull(this.rows.value[i].availabilityStartMinutes)) {
        row.controls.errorMsg.setValue(ValidationMessages.allFieldsRequired);
        break;
      }
    }
    if (i != this.rows.value.length) {
      return;
    }
    this.rows['controls'].forEach(row => {
      let availabilityStartHour = row['controls']['availabilityStartHour'].value;
      let availabilityStartMinutes = row['controls']['availabilityStartMinutes'].value;
      if (row['controls']['availabilityStartMeridiemIndicator'].value == "PM") {
        if (availabilityStartHour >= 1 && availabilityStartHour <= 11) {
          availabilityStartHour = availabilityStartHour + 12;
        }
      }
      row['controls']['availabilityStartTime'].setValue(availabilityStartHour + ":" + (availabilityStartMinutes < 10 ? "0" + availabilityStartMinutes : availabilityStartMinutes));

      let availabilityEndHour = row['controls']['availabilityEndHour'].value;
      let availabilityEndMinutes = row['controls']['availabilityEndMinutes'].value;
      if (row['controls']['availabilityEndMeridiemIndicator'].value == "PM") {
        if (Number(availabilityEndHour) >= 1 && Number(availabilityEndHour) <= 11) {
          availabilityEndHour = Number(availabilityEndHour) + 12;
        }
      }
      row['controls']['availabilityEndTime'].setValue(availabilityEndHour + ":" + availabilityEndMinutes);

      // let date = row['controls']['availabilityDate'].value;
      // row['controls']['availabilityDate'].setValue(new DatePipe('en-US').transform(date, "MM/dd/yyyy"));
      row['controls']['timeZone'].setValue(this.selectedTimezoneId);
    });

    var isOverlapping = this.checkOverlap(this.rows.value);
    if (isOverlapping == true) {
      this.overlapErrorMsg = ValidationMessages.slotOverlappingErrorMsg;
      return;
    }
    this.rows.value.forEach((item:any) => {
      item.availabilityDate = new DatePipe('en-US').transform(item.availabilityDate, "MM/dd/yyyy");
    });
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .postAvailabilitySlotWithBearer(this.apiConfig.createAvailabilitySlot, this.rows.value)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AppointmentSlotModelComponent createAvailabilitySlot " + e);
          }
          this.stickyBarService.hideLoader("");
          this.dialogRef.close(true);
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentSlotModelComponent createAvailabilitySlot " + JSON.stringify(err));
            this.appointmentSlotService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentSlotModelComponent createAvailabilitySlot " + e);
          }
        }
      );
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  isNumberNull(value: number) {
    if (value == undefined || value == null) {
      return true;
    }
    return false;
  }

  addNextAvailability() {
    if (this.rows.length <= 4) {
      this.rows.push(this.availabilityFormGroup());
    }
  }

  onAvailability(rowIndex: number) {
    this.rows.removeAt(rowIndex);
  }

  availabilityFormGroup(): FormGroup {
    return this.fb.group({
      availabilityDate: null,
      availabilityStartTime: null,
      availabilityStartHour: null,
      availabilityStartMinutes: null,
      availabilityStartMeridiemIndicator: 'AM',
      availabilityEndTime: null,
      availabilityEndHour: { value: null, disabled: true },
      availabilityEndMinutes: { value: null, disabled: true },
      availabilityEndMeridiemIndicator: 'AM',
      createdBy: this.loginUserId,
      updatedBy: this.loginUserId,
      userId: this.userId,
      timeZone: null,
      errorMsg: ""
    });
  }

  getAppointmentSlotbyId() {
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .getAvailabilitySlotByIdWithBearer(this.apiConfig.getAvailabilitySlotById + this.availabilitySlotId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              res.result.availabilityDate = new Date(new DatePipe('en-US').transform(res.result.availabilityDate.split(" ")[0], 'MM/dd/yyyy'));
              let timezone = this.timezones.filter(t => t.timeZoneId == res.result.timeZone)[0];
              if (timezone) {
                this.selectedTimezoneId = timezone.timeZoneId;
                this.selectedTimezoneAbbr = timezone.abbr;
              } else {
                this.selectedTimezoneId = "";
                this.selectedTimezoneAbbr = "";
              }
              let availabilityStartTime = this.helperService.formatTime24HourTo12Hour(res.result.availabilityStartTime).split(" ");
              let availabilityStartHHMM = availabilityStartTime[0].split(":");
              let availabilityEndTime = this.helperService.formatTime24HourTo12Hour(res.result.availabilityEndTime).split(" ");
              let availabilityEndHHMM = availabilityEndTime[0].split(":");

              this.rows.push(this.fb.group({
                id: res.result.id,
                availabilityDate: res.result.availabilityDate,
                availabilityStartTime: res.result.availabilityStartTime,
                availabilityStartHour: +(availabilityStartHHMM[0]),
                availabilityStartMinutes: +(availabilityStartHHMM[1]),
                availabilityStartMeridiemIndicator: availabilityStartTime[1],
                availabilityEndTime: res.result.availabilityEndTime,
                availabilityEndHour: { value: (+availabilityEndHHMM[0] < 10 ? "0" + availabilityEndHHMM[0] : availabilityEndHHMM[0]), disabled: true },
                availabilityEndMinutes: { value: availabilityEndHHMM[1], disabled: true },
                availabilityEndMeridiemIndicator: { value: availabilityEndTime[1], disabled: true },
                createdBy: this.loginUserId,
                updatedBy: this.loginUserId,
                userId: this.userId,
                timeZone: res.result.timeZone,
                errorMsg: ""
              }));
            } else {
              this.rows.push(this.fb.group({
                id: this.availabilitySlotId,
                availabilityDate: "",
                availabilityStartTime: null,
                availabilityStartHour: null,
                availabilityStartMinutes: null,
                availabilityStartMeridiemIndicator: null,
                availabilityEndTime: { value: null, disabled: true },
                availabilityEndHour: { value: null, disabled: true },
                availabilityEndMinutes: { value: null, disabled: true },
                availabilityEndMeridiemIndicator: { value: null, disabled: true },
                createdBy: this.loginUserId,
                updatedBy: this.loginUserId,
                userId: this.userId,
                timeZone: null,
                errorMsg: ""
              }));
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
            this.appointmentSlotService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewUserComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  updateAvailabilitySlot(): void {
    if (!this.selectedTimezoneId) {
      this.timezoneErrorMsg = ValidationMessages.allFieldsRequired;
      return;
    }
    let i = 0;
    for (i = 0; i < this.rows['controls'].length; i++) {
      const row: any = this.rows['controls'][i];
      if (!this.rows.value[i].availabilityDate || this.isNumberNull(this.rows.value[i].availabilityStartHour) || this.isNumberNull(this.rows.value[i].availabilityStartMinutes)) {
        row.controls.errorMsg.setValue(ValidationMessages.allFieldsRequired);
        break;
      }
    }
    if (i != this.rows.value.length) {
      return;
    }
    const message = ValidationMessages.editAvalabilitySlotConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        // this.dialogRef.close(true);

        this.rows['controls'].forEach(row => {
          let availabilityStartHour = row['controls']['availabilityStartHour'].value;
          let availabilityStartMinutes = row['controls']['availabilityStartMinutes'].value;
          if (row['controls']['availabilityStartMeridiemIndicator'].value == "PM") {
            if (availabilityStartHour >= 1 && availabilityStartHour <= 11) {
              availabilityStartHour = availabilityStartHour + 12;
            }
          }
          row['controls']['availabilityStartTime'].setValue(availabilityStartHour + ":" + (availabilityStartMinutes < 10 ? "0" + availabilityStartMinutes : availabilityStartMinutes));

          let availabilityEndHour = row['controls']['availabilityEndHour'].value;
          let availabilityEndMinutes = row['controls']['availabilityEndMinutes'].value;
          if (row['controls']['availabilityEndMeridiemIndicator'].value == "PM") {
            if (Number(availabilityEndHour) >= 1 && Number(availabilityEndHour) <= 11) {
              availabilityEndHour = Number(availabilityEndHour) + 12;
            }
          }
          row['controls']['availabilityEndTime'].setValue(availabilityEndHour + ":" + availabilityEndMinutes);

          let date = row['controls']['availabilityDate'].value;
          row['controls']['availabilityDate'].setValue(new DatePipe('en-US').transform(date, "MM/dd/yyyy"));
          row['controls']['timeZone'].setValue(this.selectedTimezoneId);
        });

        this.stickyBarService.showLoader("");
        this.appointmentSlotService
          .putAvailabilitySlotWithBearer(this.apiConfig.updateAvailabilitySlot, this.rows.value[0])
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception AppointmentSlotModelComponent createUser " + e);
              }
              this.stickyBarService.hideLoader("");
              this.dialogRef.close(true);
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error AppointmentSlotModelComponent createUser " + JSON.stringify(err));
                this.appointmentSlotService.catchError(err);
              } catch (e) {
                console.log("Error Exception AppointmentSlotModelComponent createUser " + e);
              }
            }
          );
      }
    });
  }

  clearDate(event: any, index: any) {
    event.stopPropagation();
    this.availabilitySlotForm.get('rows')['controls'][index].controls.availabilityDate.setValue(null);
  }

  searchTimezone(value: string) {
    if (value) {
      this.filteredTimezones = this.timezones.filter(t => t.abbr.toLowerCase().includes(value.toLowerCase())
        || t.utcOffset.toLowerCase().includes(value.toLowerCase())
        || t.timeZoneId.toLowerCase().includes(value.toLowerCase()));
    } else {
      this.filteredTimezones = this.timezones;
    }
  }

  onStartHourChange(row, hour) {
    row.controls.errorMsg.setValue("");
    let newHour = hour + 1;
    if (hour == 12) {
      newHour = 1;
      row.controls.availabilityStartMeridiemIndicator.setValue("PM");
      row.controls.availabilityEndMeridiemIndicator.setValue("PM");
    } else if (hour == 0) {
      row.controls.availabilityStartMeridiemIndicator.setValue("AM");
      row.controls.availabilityEndMeridiemIndicator.setValue("AM");
    } else if (hour == 11) {
      let meridiemIndicator = row.controls.availabilityStartMeridiemIndicator.value;
      row.controls.availabilityEndMeridiemIndicator.setValue(meridiemIndicator == "AM" ? "PM" : "AM");
      newHour = meridiemIndicator == "AM" ? 12 : 0;
    }

    row.controls.availabilityEndHour.setValue(newHour < 10 ? "0" + newHour : newHour);
  }

  onStartMinutesChange(row, minutes) {
    row.controls.errorMsg.setValue("");
    row.controls.availabilityStartMinutes.setValue(minutes);
    row.controls.availabilityEndMinutes.setValue(minutes < 10 ? "0" + minutes : minutes);
  }

  onStartMeridiemIndicatorChange(row) {
    let oldMeridiemIndicator = row.controls.availabilityStartMeridiemIndicator.value;
    let newMeridiemIndicator = oldMeridiemIndicator == "AM" ? "PM" : "AM";

    row.controls.availabilityStartMeridiemIndicator.setValue(newMeridiemIndicator);

    if (newMeridiemIndicator == "AM") {
      row.controls.availabilityEndMeridiemIndicator.setValue("AM");
      if (row.controls.availabilityStartHour.value == 12) {
        row.controls.availabilityStartHour.setValue(0);
      }
    }

    if (newMeridiemIndicator == "PM") {
      row.controls.availabilityEndMeridiemIndicator.setValue("PM");
      if (row.controls.availabilityStartHour.value == 0) {
        row.controls.availabilityStartHour.setValue(12);
      }
    }

    if (row.controls.availabilityStartHour.value == 11) {
      row.controls.availabilityEndMeridiemIndicator.setValue(oldMeridiemIndicator);
      if (newMeridiemIndicator == "PM") {
        row.controls.availabilityEndHour.setValue("00");
      }
    }
  }

  dateChanged(row: any): void {
    row.controls.errorMsg.setValue("");
  }

  checkOverlap(timeSegments) {
    timeSegments.forEach(item => {
      item.availabilityDate = new DatePipe('en-US').transform(item.availabilityDate, "MM/dd/yyyy");
    });
    //console.log(moment(timeSegments[0].availabilityDate + " " + timeSegments[0].availabilityStartTime));

    var overlap = timeSegments
      .map(r =>
        timeSegments.filter(q => q != r).map(q =>
          moment.range(
            moment(q.availabilityDate + " " + q.availabilityStartTime),
            moment(q.availabilityDate + " " + q.availabilityEndTime)
          ).overlaps(
            moment.range(
              moment(r.availabilityDate + " " + r.availabilityStartTime),
              moment(r.availabilityDate + " " + r.availabilityEndTime)
            )
          )
        )
      );

    return overlap.map(x => x.includes(true)).includes(true);
  }
}