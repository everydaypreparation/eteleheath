import { Component, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { Router } from '@angular/router';

export interface ConfirmDialogModel {
  result: any;
}

@Component({
  selector: 'app-add-patient-modal',
  templateUrl: './add-patient-modal.component.html',
  styleUrls: ['./add-patient-modal.component.scss']
})
export class AddPatientModalComponent implements OnInit {

  searchPatientForm = {
    "patientId": "",
    "userId": ""
  };

  user: any = null;
  subscribeUser: any;

  constructor(public dialogRef: MatDialogRef<AddPatientModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef) {
    this.user = this.userService.getUser();
    this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
      this.user = this.userService.getUser();
    });
  }

  ngOnInit(): void {
  }

  searchPatient(): void {

    this.searchPatientForm.userId = this.user.id;
    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.searchPatient, this.searchPatientForm)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.router.navigate(["/"]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
            this.dialogRef.close(true);
          } catch (e) {
            console.log("Success Exception search Patient " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error search Patient " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception search Patient" + e);
          }
        }
      );
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
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