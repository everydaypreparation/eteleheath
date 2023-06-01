import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent implements OnInit {

  emailMdl: string = "";
  eR: any = null;

  constructor(private validationService: ValidationService, private stickyBarService: StickyBarService
    ,private apiService: ApiService, private apiConfig: ApiConfig,
    private router: Router,
    private envAndUrlService: EnvAndUrlService,
    private routeConfig: RouteConfig) {}

  ngOnInit(): void {

  }

  clearFields(): void {
    this.emailMdl = "";
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    if(this.emailMdl.includes('@')){
      vO["email"] = this.validationService.getValue(this.validationService.setValue(this.emailMdl).required(ValidationMessages.enterUsernameOrEmail).noSpecialChar(ValidationMessages.emailNotString).isMail(ValidationMessages.emailNotValid).obj);
    } else {
      vO["email"] = this.validationService.getValue(this.validationService.setValue(this.emailMdl).required(ValidationMessages.enterUsernameOrEmail).alphaNumeric(ValidationMessages.usernameNotAlphanumeric).obj);
    }
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      this.emailMdl = vO.email.value;
      return true;
    }
  }

  submitFormClick(){
    if (this.isValidated()) {
      this.forgotPassword();
    }
  }

  forgotPassword(): void {
    const body = {
      emailAddress: this.emailMdl,
      baseUrl: this.envAndUrlService.HOST_URL
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithoutBearer(this.apiConfig.forgotPassword, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.clearFields();
              this.router.navigate([
                this.routeConfig.signInPath
              ]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ForgotPasswordComponent forgotPassword " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ForgotPasswordComponent forgotPassword " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ForgotPasswordComponent forgotPassword " + e);
          }
        }
      );
  }

  signInClick(): void {
    this.router.navigate([this.routeConfig.signInPath]);
  }

}
