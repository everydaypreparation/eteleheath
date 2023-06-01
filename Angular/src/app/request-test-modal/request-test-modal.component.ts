import { Component, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { FormControl } from '@angular/forms';
import { ValidationService } from '../services/validation.service';
import { ValidationMessages } from '../shared/validation-messages.enum';
import { ApiService } from '../services/api.service';
import { UserService } from '../services/user.service';
import { PropConfig } from '../configs/prop.config';
import { DatePipe } from '@angular/common';

export interface ConfirmDialogModel {
  patientName: any;
  appointmentDate: any;
  patientId: any;
  id: any;
  consultantId: any;
}

@Component({
  selector: 'app-request-test-modal',
  templateUrl: './request-test-modal.component.html',
  styleUrls: ['./request-test-modal.component.scss']
})
export class RequestTestModalComponent implements OnInit {

  requestTestForm = {
    "patientId": "",
    "consultantId": "",
    "diagnosticId": "",
    "reportType": "",
    "reportDetails": "",
    "dueDate": ""
  }

  reportType = ["Health Record", "Diagnostic Images-Scans", "Diagnostic Reports", "Medical Report", "Others"];
  patientName: any;
  appointmentDate: any;
  patientId: any;
  id: any = null;
  consultantId: any;
  user: any;
  timezones: any[] = [];
  diagnosticsFiltered: any = [];
  diagnostics: any = [];
  diagnosticId: any = null;
  userTimezone: any = {};
  eR: any = null;

  constructor(public dialogRef: MatDialogRef<RequestTestModalComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiConfig: ApiConfig, private stickyBarService: StickyBarService,  private apiService: ApiService,
     private validationService: ValidationService, private userService: UserService, private propConfig: PropConfig) {
    // Update view with given values
    this.patientName = data.patientName;
    this.appointmentDate = data.appointmentDate;
    this.id = data.id;
    this.consultantId = data.consultantId;
    this.patientId = data.patientId;
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.getUserTimezoneOffset();
    this.getDiagnostics();
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  saveRequestTest() {
    if (!this.isValidated()) {
    } else {
      this.requestTestForm.patientId = this.patientId;
      this.requestTestForm.consultantId = this.consultantId;
      this.requestTestForm.diagnosticId = this.diagnosticId;
      this.stickyBarService.showLoader("");
      if (this.requestTestForm.dueDate && this.requestTestForm.dueDate != 'null') {
        this.requestTestForm.dueDate = new DatePipe('en-US').transform(this.requestTestForm.dueDate, 'MM/dd/yyyy');
      } else {
        this.requestTestForm.dueDate = '';
      }
      console.log(this.requestTestForm);
      this.apiService
        .postWithBearer(this.apiConfig.requestForTest, this.requestTestForm)
        .subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.dialogRef.close(true);
                this.stickyBarService.showSuccessSticky(res.result.message);
              } else {
                this.dialogRef.close(true);
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception RequestTestModalComponent saveNotes " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error RequestTestModalComponent saveNotes " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception RequestTestModalComponent saveNotes " + e);
            }            
          }
        );
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["diagnostic"] = this.validationService.getValue(this.validationService.setValue(this.diagnosticId).required(ValidationMessages.noDiagnosticSelected).obj);
    vO["reportType"] = this.validationService.getValue(this.validationService.setValue(this.requestTestForm.reportType).required(ValidationMessages.requiredField).obj);
    vO["reportDetails"] = this.validationService.getValue(this.validationService.setValue(this.requestTestForm.reportDetails).required(ValidationMessages.requiredField).obj);
    vO["dueDate"] = this.validationService.getValue(this.validationService.setValue(this.requestTestForm.dueDate).required(ValidationMessages.requiredField).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  clearDOB(event: any) {
    event.stopPropagation();
    this.requestTestForm.dueDate = null;
  }

  getDiagnostics(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserByRoles + "diagnostic")
      .subscribe(
        (res: any) => {
          try {
            this.diagnostics = [];
            if (res.result.statusCode == 200) {
              res.result.getUserDetailsListOutputs.forEach(item => {
                const obj:any = {};
                let name = item.title? item.title+" " :"";
                name += item.name? item.name+" " :"";
                name += item.surname? item.surname+" " :"";
                obj.value = item.userId;
                obj.viewValue= name;
                obj.userId= item.userId;
                obj.id= item.id;
                this.diagnostics.push(obj);
              });
              this.diagnosticsFiltered = [... this.diagnostics];
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception RequestTestModalComponent getUserListByRoleName " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RequestTestModalComponent getUserListByRoleName " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RequestTestModalComponent getUserListByRoleName " + e);
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
              let userTimezone = this.timezones.filter(t => t.timeZoneId == this.user.timezone)[0];
              if (!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId == this.propConfig.defaultTimezoneId)[0];
              }
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                .replace("UTC", "").replace(":", "");
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  searchDiagnostic(value: string) {
    if (value) {
      this.diagnosticsFiltered = this.diagnostics.filter(t => t.viewValue.toLowerCase().includes(value.toLowerCase()));
    } else {
      this.diagnosticsFiltered = this.diagnostics;
    }
  }
}

/**
 * Class to represent confirm dialog model.
 *
 * It has been kept here to keep it as part of shared component.
 */
export class ConfirmDialogModel {

  constructor(public title: string, public message: string) {
  }


}
