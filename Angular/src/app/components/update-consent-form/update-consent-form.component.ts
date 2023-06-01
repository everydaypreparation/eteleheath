import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { ValidationService } from 'src/app/services/validation.service';
import { UserService } from 'src/app/services/user.service';
import { HelperService } from 'src/app/services/helper.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';

@Component({
  selector: 'app-update-consent-form',
  templateUrl: './update-consent-form.component.html',
  styleUrls: ['./update-consent-form.component.scss']
})
export class UpdateConsentFormComponent implements OnInit {

  consentFormsId: any;
  user: any = null;
  eR: any = null;
  form = {
    "consentFormsId": "",
    "title": "",
    "description": "",
    "subTitle": "",
    "shortDescription": "",
    "disclaimer":""
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute) { }

  ngOnInit(): void {
    this.consentFormsId = this.activatedRoute.snapshot.params['consentFormsId'];
    if(this.consentFormsId) {
      this.getConsentFormById();
    }
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      if(this.consentFormsId) {
        this.updateConsentForm();
      } else {
        this.createConsentForm();
      }
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["title"] = this.validationService.getValue(this.validationService.setValue(this.form.title).required(ValidationMessages.enterTitle).obj);
    vO["description"] = this.validationService.getValue(this.validationService.setValue(this.form.description).required(ValidationMessages.enterDescription).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  createConsentForm(): void {
    const body = this.form;
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.createConsentForm, body)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if(res.result.statusCode==200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.router.navigate([this.routeConfig.adminConsentFormsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception UpdateConsentFormComponent createConsentForm " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error UpdateConsentFormComponent createConsentForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception UpdateConsentFormComponent createConsentForm " + e);
          }
        }
      );
  }

  updateConsentForm(): void {
    const body = this.form;
    this.stickyBarService.showLoader("");
    this.apiService
      .putWithBearer(this.apiConfig.updateConsentFormById, body)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if(res.result.statusCode==200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.router.navigate([this.routeConfig.adminConsentFormsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception UpdateConsentFormComponent updateConsentForm " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error UpdateConsentFormComponent updateConsentForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception UpdateConsentFormComponent updateConsentForm " + e);
          }
        }
      );
  }
  
  getConsentFormById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getConsentFormById + this.consentFormsId)
      .subscribe(
        (res: any) => {
          try {
            if(res.result.statusCode==200) {
              this.form = res.result;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception UpdateConsentFormComponent getConsentFormById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error UpdateConsentFormComponent getConsentFormById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception UpdateConsentFormComponent getConsentFormById " + e);
          }
        }
      );
  }

  clearFields(): void {
    for (let key of Object.keys(this.form)) {
      this.form[key] = "";
    }
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.adminDashboardPath
    ]);
  }
}
