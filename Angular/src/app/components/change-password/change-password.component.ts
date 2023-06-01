import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ValidationService } from 'src/app/services/validation.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent implements OnInit {

  user: any;
  eR: any = null;
  passswordMatch: any;

  changePassword = {
    "currentPassword": "",
    "newPassword": "",
    "confirmPassword": ""
  }

  changePasswordForm = {
    "currentPassword": "",
    "newPassword": ""
  }

  constructor(private userService: UserService, private router: Router, private routeConfig: RouteConfig
    , private helperService: HelperService, private validationService: ValidationService, private stickyBarService: StickyBarService
    , private apiConfig: ApiConfig, private apiService: ApiService) { }

  ngOnInit(): void {
    this.user = this.userService.getUser();
  }

  saveChangePassword() {
    this.passswordMatch = "";
    if (!this.isValidated()) {
    } else {
      if (this.changePassword.newPassword == this.changePassword.confirmPassword) {
        this.passswordMatch = "";
        this.stickyBarService.showLoader("");
        this.changePasswordForm.currentPassword = this.changePassword.currentPassword;
        this.changePasswordForm.newPassword = this.changePassword.newPassword;
        this.apiService
          .postWithBearer(this.apiConfig.changePassword, this.changePasswordForm)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.stickyBarService.showSuccessSticky(res.result.message);
                  this.clearFields();
                  this.clearFieldsForm();
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                  this.clearFields();
                  this.clearFieldsForm();
                }
              } catch (e) {
                console.log("Success Exception ChangePasswordComponent saveChangePassword " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                console.log("Error ChangePasswordComponent saveChangePassword " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception ChangePasswordComponent saveChangePassword " + e);
              }
              this.stickyBarService.hideLoader("");
            }
          );
      } else {
        this.passswordMatch = ValidationMessages.passwordMatches;
      }
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["currentPassword"] = this.validationService.getValue(this.validationService.setValue(this.changePassword.currentPassword).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);
    vO["newPassword"] = this.validationService.getValue(this.validationService.setValue(this.changePassword.newPassword).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);
    vO["confirmPassword"] = this.validationService.getValue(this.validationService.setValue(this.changePassword.confirmPassword).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  clearFields(): void {
    for (let key of Object.keys(this.changePassword)) {
      this.changePassword[key] = "";
    }
  }

  clearFieldsForm(): void {
    for (let key of Object.keys(this.changePasswordForm)) {
      this.changePasswordForm[key] = "";
    }
  }

  homeClick(): void {
    if (this.userService.userRole) {
      const role = this.userService.userRole;
      if (role == "PATIENT") {
        this.router.navigate([this.routeConfig.patientDashboardPath]);
      } else if (role == "INSURANCE") {
        this.helperService.navigateInsuranceUser(this.user);
      } else if (role == "MEDICALLEGAL") {
        this.helperService.navigateMedicalLegalUser(this.user);
      } else if (role == "ADMIN") {
        this.router.navigate([this.routeConfig.adminDashboardPath]);
      } else if (role == "FAMILYDOCTOR") {
        let usr =  this.userService.getUser();
        if (!usr.isCase) {
          this.router.navigate([this.routeConfig.familyDoctorsEmptyDashboardPath]);
        } else {
          this.router.navigate([this.routeConfig.familyDoctorDashboardPath]);
        }
      } else if (role == "DIAGNOSTIC") {
        if (this.userService.getUser().isAppointment == false) {
          this.router.navigate([
            this.routeConfig.diagnosticDashboardPath
          ]);
        } else if (this.userService.getUser().isAppointment == true) {
          this.router.navigate([
            this.routeConfig.diagnosticDashboardDetailsPath
          ]);
        }
      } else if (role == "CONSULTANT") {
        this.router.navigate([this.routeConfig.consultantDashboardPath]);
      }
    }
  }

}
