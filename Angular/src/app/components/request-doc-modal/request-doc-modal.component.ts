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

interface RoleTypes {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-request-doc-modal',
  templateUrl: './request-doc-modal.component.html',
  styleUrls: ['./request-doc-modal.component.scss']
})


export class RequestDocModalComponent implements OnInit {

  roleTypes: RoleTypes[] = [
    { value: 'CONSULTANT', viewValue: 'Consultant/Oncologist' },
    { value: 'PATIENT', viewValue: 'Patient' }
  ];

  reqDocForm = {
    "userId": "",
    "firstName": "",
    "lastName": "",
    "emailAddress": "",
    "phoneNumber": "",
    "hospital": "",
    "roleName": ""
  }

  eR: any = null;
  loginUserId: any;
  isInputShown: any = true;
  userRole: any;
  user: any;
  constructor(public dialogRef: MatDialogRef<RequestDocModalComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiConfig: ApiConfig, private stickyBarService: StickyBarService,
    private apiService: ApiService, private validationService: ValidationService, private userService: UserService , private router: Router,
    private routeConfig: RouteConfig) {
    // Update view with given values
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.userRole = this.user.roleNames[0];
    if(this.userRole == 'FAMILYDOCTOR'){
      this.isInputShown = false;
      this.reqDocForm.roleName = 'PATIENT';
    }else if(this.userRole == 'PATIENT'){
     this.reqDocForm.roleName = 'FAMILYDOCTOR';
    }else{
      this.reqDocForm.roleName = 'CONSULTANT';
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
      this.createRequestDoctor();
    }
  }

  createRequestDoctor(): void {
    this.reqDocForm.userId = this.loginUserId;
    if(this.reqDocForm.roleName == 'PATIENT'){
      this.reqDocForm.hospital = "";
     }
    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.createRequestDoctor, this.reqDocForm)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              if(this.userRole == 'FAMILYDOCTOR'){
                  this.router.navigate([this.routeConfig.familyDoctorDashboardPath]);
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
            this.dialogRef.close(true);
          } catch (e) {
            console.log("Success Exception RequestDocModalComponent createRequestDoctor " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RequestDocModalComponent createRequestDoctor " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RequestDocModalComponent createRequestDoctor " + e);
          }
        }
      );
  }


  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.reqDocForm.firstName).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.reqDocForm.lastName).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.reqDocForm.emailAddress).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
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

  radioChange(name: any){
   if(name == 'PATIENT'){
    //this.reqDocForm.hospital = "";
    this.isInputShown = false;
   }else{
    this.isInputShown = true;
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
