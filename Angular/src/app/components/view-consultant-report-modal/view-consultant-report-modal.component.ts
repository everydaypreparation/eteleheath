import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { PropConfig } from 'src/app/configs/prop.config';
declare const $: any;

export interface ConfirmDialogModel {
  documentId: any;
  doctorName: any;
  docDate: any;
}

@Component({
  selector: 'app-view-consultant-report-modal',
  templateUrl: './view-consultant-report-modal.component.html',
  styleUrls: ['./view-consultant-report-modal.component.scss']
})
export class ViewConsultantReportModalComponent implements OnInit {

  loginUserId: any;
  documentId: any;
  docVal: any;
  imgVal: any;
  message: any;
  documentName: any;
  doctorName: any;
  docDate: any;
  docDateTime: any;
  timezones: any[] = [];
  userTimezone: any = {};
  user: any;
  
  constructor(public dialogRef: MatDialogRef<ViewConsultantReportModalComponent>, private stickyBarService: StickyBarService
    , @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel, private userService: UserService,
    private apiConfig: ApiConfig, private apiService: ApiService, private validationService: ValidationService, private helperService: HelperService, private propConfig: PropConfig) {
    // Update view with given value
    this.documentId = data.documentId;
    this.doctorName = data.doctorName;
    this.docDate = data.docDate;
  }

  ngOnInit(): void {
    //this.getAvailabilitySlotById();
    this.getUserTimezoneOffset();
    this.user = this.userService.getUser();
    this.showDocumentById(this.documentId, this.doctorName,this.docDate);
    this.message = "Please wait....";
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  showDocumentById(documentId: any, doctorName: any, docDate: any) {
    this.stickyBarService.showLoader("");
    this.apiService
    .getWithBearer(this.apiConfig.downloadReportByConsultId + documentId)
      .subscribe(
        (res: any) => {
          console.log(res.result);
          try {
            if (res.result.statusCode == 200) {
              if (res.result.report) {
                this.docVal = res.result.report;
                this.documentName = doctorName;
                this.docDateTime = docDate;
              }
              this.message = "";
            } else {
              this.message = ValidationMessages.documentNotFound;
            }
          } catch (e) {
            console.log("Success Exception ViewConsultantReportModalComponent showDocumentById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewConsultantReportModalComponent showDocumentById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewConsultantReportModalComponent showDocumentById " + e);
          }
        }
      );
  }

  downloadDocument() {
    this.helperService.downloadSelectedConsulationReportByConsultId(this.documentId, this.documentName);
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
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

}
