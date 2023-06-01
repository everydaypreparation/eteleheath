import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { HelperService } from 'src/app/services/helper.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})

export class SignupComponent implements OnInit {

  firstNameMdl: string = "";
  lastNameMdl: string = "";
  emailMdl: string = "";
  loadContent: boolean = false;
  eR: any = null;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private propConfig: PropConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private userService: UserService,
    private helperService: HelperService,
    private stickyBarService: StickyBarService,) {
    if (this.userService.isUserAuthenticated()) {
      this.getUser();
    } else {
      this.loadContent = true;
    }
  }

  ngOnInit(): void {
  }

  signUpClick(): void {
    if (this.isValidated()) {
      this.signUp();
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    this.firstNameMdl = this.firstNameMdl.trim();
    this.lastNameMdl = this.lastNameMdl.trim();
    this.emailMdl = this.emailMdl.trim();
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.firstNameMdl).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.lastNameMdl).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.emailMdl).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      this.firstNameMdl = vO.firstName.value;
      this.lastNameMdl = vO.lastName.value;
      this.emailMdl = vO.email.value;
      return true;
    }
  }

  signUp(): void {
    const body = {
      name: this.firstNameMdl,
      surname: this.lastNameMdl,
      emailAddress: this.emailMdl,
      isActive: true,
      timezone: this.propConfig.defaultTimezoneId,
      roleNames: [
        this.propConfig.roles[2]
      ],
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithoutBearer(this.apiConfig.signupPatient, body)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              setTimeout(() => {
                this.clearFields();
                this.router.navigate([
                  this.routeConfig.signInPath
                ]);
              }, 1000);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception SignupComponent signUp " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SignupComponent signUp " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SignupComponent signUp " + e);
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
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.userService.setUser(res.result);
              if (res.result.roleNames) {
                const role = this.userService.userRole;
                let user = this.userService.getUser();
                if (role == "ADMIN") {
                  this.router.navigate([
                    this.routeConfig.adminDashboardPath
                  ]);
                } else if (role == "PATIENT") {
                  this.router.navigate([
                    this.routeConfig.findingConsultantPath
                  ]);
                } else if (role == "CONSULTANT") {
                  this.router.navigate([
                    this.routeConfig.consultantDashboardPath
                  ]);
                } else if (role == "FAMILYDOCTOR") {
                  this.router.navigate([
                    this.routeConfig.familyDoctorDashboardPath
                  ]);
                } else if (role == "INSURANCE") {
                  this.helperService.navigateInsuranceUser(user);
                } else if (role == "MEDICALLEGAL") {
                  this.helperService.navigateMedicalLegalUser(user);
                } else if (role == "DIAGNOSTIC") {
                  this.router.navigate([
                    this.routeConfig.diagnosticDashboardPath
                  ]);
                }
              }
            } else {
              this.loadContent = true;
            }
          } catch (e) {
            console.log("Success Exception SignupComponent getUser " + e);
            this.loadContent = true;
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SignupComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SignupComponent getUser " + e);
          }
          this.loadContent = true;
        }
      );
  }

  signInClick(): void {
    this.router.navigate([this.routeConfig.signInPath]);
  }

  clearFields(): void {
    this.firstNameMdl = "";
    this.lastNameMdl = "";
    this.emailMdl = "";
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }
}