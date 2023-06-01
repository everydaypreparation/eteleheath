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
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-request-users-modal',
  templateUrl: './request-users-modal.component.html',
  styleUrls: ['./request-users-modal.component.scss']
})
export class RequestUsersModalComponent implements OnInit {

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
  isInputShown: any = false;
  roleName: any;
  user: any;
  roles: any = [];

  constructor(public dialogRef: MatDialogRef<RequestUsersModalComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiConfig: ApiConfig, private stickyBarService: StickyBarService,
    private apiService: ApiService, private validationService: ValidationService, private userService: UserService , private router: Router,
    private routeConfig: RouteConfig, private propConfig: PropConfig) {
    // Update view with given values
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.roles = [];
    this.propConfig.roles.forEach(item => {
      const obj: any = {};
      obj.value = item;
      obj.viewValue = (item == "FAMILYDOCTOR") ? "FAMILY DOCTOR" : (item == "MEDICALLEGAL") ? "MEDICAL LEGAL" : item;
      if (item != "ADMIN"){
        this.roles.push(obj);
      }
    });
    // this.roleName = this.user.roleNames[0];
    // if (this.roleName == 'PATIENT'){
    //   this.reqDocForm.roleName = 'CONSULTANT';
    // }else{
    //  this.reqDocForm.roleName = 'PATIENT';
    //  this.isInputShown = false;
    // }
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

  selectionChangeRole(e:any){
    if (e.value == "CONSULTANT" || e.value == "FAMILYDOCTOR"){
      this.reqDocForm.hospital = "";
      this.isInputShown = true;      
    }else{
      this.isInputShown = false;
    }
  }

  createRequestDoctor(): void {
    this.reqDocForm.userId = this.loginUserId;
    this.reqDocForm.roleName = this.roleName;
    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.createRequestDoctor, this.reqDocForm)
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
            console.log("Success Exception RequestDocModalComponent createRequestDoctor " + e);
          }
          this.stickyBarService.hideLoader("");
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
    vO["roleName"] = this.validationService.getValue(this.validationService.setValue(this.roleName).required(ValidationMessages.noRoleSelected).obj);
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