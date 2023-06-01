import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss']
})
export class SigninComponent implements OnInit {

  emailMdl: string = "";
  passwordMdl: string = "";
  loadContent: boolean = false;
  eR: any = null;
  remamberMe: boolean = false;
  rememberMeCheckbox = false;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private validationService: ValidationService,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private helperService: HelperService) {
    if (this.userService.isUserAuthenticated()) {
      this.getUser();
    } else {
      this.loadContent = true;
    }
  }

  ngOnInit(): void {
    if (localStorage.rememberMe && localStorage.loginEmailId && localStorage.loginPassword) {
      this.emailMdl = localStorage.loginEmailId;
      this.passwordMdl = localStorage.loginPassword;
      this.rememberMeCheckbox = localStorage.rememberMe;
      this.remamberMe = true;
    }
  }

  signInClick(): void {
    if (this.isValidated()) {
      this.signIn();
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    this.emailMdl = this.emailMdl.trim();
    this.passwordMdl = this.passwordMdl.trim();
    if (this.emailMdl.includes('@')) {
      vO["email"] = this.validationService.getValue(this.validationService.setValue(this.emailMdl).required(ValidationMessages.enterUsernameOrEmail).noSpecialChar(ValidationMessages.emailNotString).isMail(ValidationMessages.emailNotValid).obj);
    } else {
      vO["email"] = this.validationService.getValue(this.validationService.setValue(this.emailMdl).required(ValidationMessages.enterUsernameOrEmail).alphaNumeric(ValidationMessages.usernameNotAlphanumeric).obj);
    }
    vO["password"] = this.validationService.getValue(this.validationService.setValue(this.passwordMdl).required(ValidationMessages.enterPassword).rangeLength([6, 40], ValidationMessages.passwordLength).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      this.emailMdl = vO.email.value;
      this.passwordMdl = vO.password.value;
      return true;
    }
  }

  signIn(): void {
    const body = {
      userNameOrEmailAddress: this.emailMdl,
      password: this.passwordMdl,
      rememberClient: true
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithoutBearer(this.apiConfig.signin, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userService.setToken(res.result.accessToken);
              localStorage.setItem("userId", res.result.userId);
              if (this.remamberMe == true && this.emailMdl && this.passwordMdl) {
                localStorage.loginEmailId = this.emailMdl;
                localStorage.loginPassword = this.passwordMdl;
                localStorage.rememberMe = this.remamberMe;
              } else {
                localStorage.loginEmailId = "";
                localStorage.loginPassword = "";
                localStorage.rememberMe = false;
              }
              this.getUser();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception SigninComponent signIn " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SigninComponent signIn " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SigninComponent signIn " + e);
          }
        }
      );
  }

  getUser(): void {
    const userId = localStorage.getItem("userId");
    if (!userId) {
      this.loadContent = true;
      return;
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUser + userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.userService.setUser(res.result);
              if (this.userService.userRole) {
                const role = this.userService.userRole;
                let user = this.userService.getUser();
                if (role == "ADMIN") {

                  localStorage.Impersonator = user.id;
                  localStorage.IsImpersonating = false;
                  localStorage.removeItem("survay-form");

                  if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.router.navigate([
                      this.routeConfig.adminDashboardPath
                    ]);
                  }
                } else if (role == "PATIENT") {
                  // this.router.navigate([
                  //   this.routeConfig.findingConsultantPath
                  // ]);
                  if (localStorage.getItem('survay-form') && localStorage.getItem('survay-form').includes('/survey-form')) {
                    let params = localStorage.getItem('survay-form').replace(this.routeConfig.surveyFormPath, "").split("/");
                    let id = params[2];
                    this.router.navigate([this.routeConfig.surveyFormPath + "/" + id]);
                  }
                  else if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.helperService.navigatePatient(user);
                  }
                } else if (role == "CONSULTANT") {
                  localStorage.removeItem("survay-form");
                  if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.router.navigate([
                      this.routeConfig.consultantDashboardPath
                    ]);
                  }
                } else if (role == "FAMILYDOCTOR") {
                  // this.router.navigate([
                  //   this.routeConfig.familyDoctorDashboardPath
                  // ]);
                  localStorage.removeItem("survay-form");
                  if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.helperService.navigateFamilyDoctorUser(user);
                  }
                } else if (role == "INSURANCE") {
                  if (localStorage.getItem('survay-form') && localStorage.getItem('survay-form').includes('/survey-form')) {
                    let params = localStorage.getItem('survay-form').replace(this.routeConfig.surveyFormPath, "").split("/");
                    let id = params[2];
                    this.router.navigate([this.routeConfig.surveyFormPath + "/" + id]);
                  }
                  else if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.helperService.navigateInsuranceUser(user);
                  }
                } else if (role == "MEDICALLEGAL") {
                  if (localStorage.getItem('survay-form') && localStorage.getItem('survay-form').includes('/survey-form')) {
                    let params = localStorage.getItem('survay-form').replace(this.routeConfig.surveyFormPath, "").split("/");
                    let id = params[2];
                    this.router.navigate([this.routeConfig.surveyFormPath + "/" + id]);
                  }
                  else if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.helperService.navigateMedicalLegalUser(user);
                  }
                } else if (role == "DIAGNOSTIC") {
                  localStorage.removeItem("survay-form");
                  if (localStorage.getItem('samvaadMeeting') && localStorage.getItem('samvaadMeeting').includes('/meeting/join/')) {
                    this.router.navigate([localStorage.getItem('samvaadMeeting')]);
                  }
                  else {
                    this.helperService.navigateDiagnosticUser(user);
                  }
                  // this.router.navigate([
                  //   this.routeConfig.diagnosticDashboardPath
                  // ]);
                }
                //  else if (role == "DIAGNOSTIC" && res.result.isAppointment == true){
                //   this.router.navigate([
                //     this.routeConfig.diagnosticDashboardDetailsPath
                //   ]);
                // }

              }
            } else {
              this.loadContent = true;
            }
          } catch (e) {
            console.log("Success Exception SigninComponent getUser " + e);
            this.loadContent = true;
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SigninComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SigninComponent getUser " + e);
          }
          this.loadContent = true;
        }
      );
  }

  forgotPasswordClick(): void {
    this.router.navigate([this.routeConfig.forgotPasswordPath]);
  }

  signUpClick(): void {
    this.router.navigate([this.routeConfig.signUpPath]);
  }

  clearFields(): void {
    this.emailMdl = "";
    this.passwordMdl = "";
  }

  validatorPress(e: any, type: any): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
    const charCode = e.which ? e.which : e.keyCode;
    if (charCode == 13) {
      this.signInClick();
    }
  }

  rememberMeEvent(val: any) {
    this.remamberMe = val.target.checked;
  }
}
