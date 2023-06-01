import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {

  confirmPasswordMdl: string = "";
  eR: any = null;
  newPasswordMdl: string = "";
  confirErrorMsg: string = "";
  emailId: string = "";
  token: string = "";

  constructor(private validationService: ValidationService, private stickyBarService: StickyBarService
    , private apiService: ApiService, private apiConfig: ApiConfig,
    private router: Router,
    private routeConfig: RouteConfig, private route: ActivatedRoute) {
    this.route.queryParams.subscribe(params => { 
     this.emailId = params['email'];
     this.token = params['token']; 
    });

  }

  ngOnInit(): void {
    // let emailId = this.route.snapshot.params['email'];
    // let token = this.route.snapshot.params['token'];
   

  }

  clearFields(): void {
    this.newPasswordMdl = "";
    this.confirmPasswordMdl = "";
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["newPassword"] = this.validationService.getValue(this.validationService.setValue(this.newPasswordMdl).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);
    vO["confirmPassword"] = this.validationService.getValue(this.validationService.setValue(this.confirmPasswordMdl).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);

    // vO["newPassword"] = this.validationService.getValue(this.validationService.setValue(this.newPasswordMdl).required("Enter new password").isString("Password should be string").rangeLength([2, 150], "Password should be 2 to 150 characters long").obj);
    // vO["confirmPassword"] = this.validationService.getValue(this.validationService.setValue(this.confirmPasswordMdl).required("Enter confirm password").isString("Password should be string").rangeLength([2, 150], "Password should be 2 to 150 characters long").obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      this.newPasswordMdl = vO.newPassword.value;
      this.confirmPasswordMdl = vO.confirmPassword.value;
      return true;
    }
  }

  submitFormClick() {
    if (this.isValidated()) {
      if (this.newPasswordMdl != this.confirmPasswordMdl) {
        this.confirErrorMsg = ValidationMessages.passwordMatch;
        return;
      } else {
        this.confirErrorMsg = "";
        this.resetPassword();
      }
    }
  }

  resetPassword(): void {
    const body = {
    token: this.token,
    email: this.emailId,
    password:  this.newPasswordMdl,
    confirmPassword: this.confirmPasswordMdl
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithoutBearer(this.apiConfig.resetPassword, body)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.clearFields();
              this.router.navigate([this.routeConfig.signInPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ResetPasswordComponent resetPassword " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ResetPasswordComponent resetPassword " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ResetPasswordComponent resetPassword " + e);
          }
        }
      );
  }

  signInClick(): void {
    this.router.navigate([this.routeConfig.signInPath]);
  }

}
