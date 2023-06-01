import { Component, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { ValidationService } from 'src/app/services/validation.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';

export interface ConfirmDialogModel {
  costId: any;
}

interface CostType {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-add-cost-modal',
  templateUrl: './add-cost-modal.component.html',
  styleUrls: ['./add-cost-modal.component.scss']
})
export class AddCostModalComponent implements OnInit {

  addCostForm = {
    "roleName": "",
    "keyName": "",
    "value": ""
  }


  updateCostForm = {
    "costId": "",
    "keyName": "",
    "value": ""
  }

  eR: any = null;
  loginUserId: any;
  costId: any;
  userRole: any;
  user: any;


  costType: CostType[] = [
    { value: 'Consultation Fee', viewValue: 'Consultation Fee' },
    { value: 'Base Rate', viewValue: 'Base Rate' },
    { value: 'Per Page Rate', viewValue: 'Per Page Rate' },
    { value: 'Upto Pages', viewValue: 'Upto Pages' }
  ];

  constructor(public dialogRef: MatDialogRef<AddCostModalComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiConfig: ApiConfig, private stickyBarService: StickyBarService,
    private apiService: ApiService, private validationService: ValidationService, private userService: UserService, private router: Router,
    private routeConfig: RouteConfig) {
    this.costId = data.costId;
    // Update view with given values
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.userRole = this.user.roleNames[0];
    if (this.costId) {
      this.getCost();
    }
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      this.addCost();
    }
  }

  addCost(): void {
    this.stickyBarService.showLoader("");
    this.apiService.postFormDataWithBearer(this.apiConfig.createCost, this.addCostForm)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
            this.dialogRef.close(true);
          } catch (e) {
            console.log("Success Exception AddCostModalComponent addCost " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddCostModalComponent addCost " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddCostModalComponent addCost " + e);
          }
        }
      );
  }

  updateCost(): void {
    if (this.isValidated()) {
      this.updateCostForm.costId = this.costId;
      this.updateCostForm.keyName = this.addCostForm.keyName;
      this.updateCostForm.value = this.addCostForm.value;
      this.stickyBarService.showLoader("");
      this.apiService.putFormDataWithBearer(this.apiConfig.updateCost, this.updateCostForm)
        .subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
              this.dialogRef.close(true);
            } catch (e) {
              console.log("Success Exception AddCostModalComponent updateCost " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error AddCostModalComponent updateCost " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception AddCostModalComponent updateCost " + e);
            }
          }
        );
    }
  }


  getCost(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getCost + this.costId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.addCostForm = res.result;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception CostConfigurationComponent getAllCost " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error CostConfigurationComponent getAllCost " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception CostConfigurationComponent getAllCost " + e);
          }
        }
      );
  }


  isValidated(): boolean {
    const vO: any = {};
    vO["keyName"] = this.validationService.getValue(this.validationService.setValue(this.addCostForm.keyName).required(ValidationMessages.requiredField).obj);
    vO["value"] = this.validationService.getValue(this.validationService.setValue(this.addCostForm.value).required(ValidationMessages.requiredField).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }


  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
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
