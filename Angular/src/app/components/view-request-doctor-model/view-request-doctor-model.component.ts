import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';

export interface ConfirmDialogModel {
  loginUserId: number;
  requestId: any;
}

@Component({
  selector: 'app-view-request-doctor-model',
  templateUrl: './view-request-doctor-model.component.html',
  styleUrls: ['./view-request-doctor-model.component.scss']
})
export class ViewRequestDoctorModelComponent implements OnInit {

  requestId: string;
  loginUserId: any;
  requestedDoctorDetails: any;
  message: any;

  constructor(public dialogRef: MatDialogRef<ViewRequestDoctorModelComponent>, private stickyBarService: StickyBarService
    , @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel, private apiService: ApiService, private apiConfig: ApiConfig) {
    // Update view with given value
    this.loginUserId = data.loginUserId;
    this.requestId = data.requestId;
  }

  ngOnInit(): void {
    this.message = "Please wait....";
    this.getRequestedDoctorDetails();
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  getRequestedDoctorDetails(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDetailsByRequestId + this.requestId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.requestedDoctorDetails = res.result;
              this.message = "";
            }
          } catch (e) {
            console.log("Success Exception ViewRequestDoctorModelComponent getRequestedDoctorDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewRequestDoctorModelComponent getRequestedDoctorDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewRequestDoctorModelComponent getRequestedDoctorDetails " + e);
          }
        }
      );
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
