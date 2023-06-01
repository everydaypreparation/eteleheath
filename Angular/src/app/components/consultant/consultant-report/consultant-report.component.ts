import { Component, OnInit, ViewChild } from '@angular/core';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { SignaturePad } from 'angular2-signaturepad';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { RouteConfig } from 'src/app/configs/route.config';

@Component({
  selector: 'app-consultant-report',
  templateUrl: './consultant-report.component.html',
  styleUrls: ['./consultant-report.component.scss']
})
export class ConsultantReportComponent implements OnInit {

  @ViewChild(SignaturePad) signaturePad: SignaturePad;

  public signaturePadOptions: Object = {
    'minWidth': 1.5,
    'maxWidth': 1.5,
    'canvasWidth': 300,
    'canvasHeight': 150,
  };

  constructor(
    private validationService: ValidationService,
    private userService: UserService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,
    private router: Router, 
    private routeConfig: RouteConfig) { }

  eR: any = null;
  user: any;
  consultId: any;
  appointmentId: any;
  consultantReportBody = {
    "ConsultId": "",
    "Purpose": "",
    "Allergies": "",
    "Investigation": "",
    "Impression": "",
    "Plan": "",
    "FamilyHistory": "",
    "SocialHistory": "",
    "Medication": "",
    "PastMedicalHistory": "",
    "Notes": "",
    "ReviewOfHistory": "",
    "UserId": "",
    "AppointmentId": "",
    "SignaturePath":""
  };

  ngOnInit(): void {
    this.consultId = this.activatedRoute.snapshot.params['consultId'];
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
    this.user = this.userService.getUser();

    this.getConsultantReport();
  }

  getConsultantReport() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getConsultantReport + this.consultId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result;
              
              for (let key of Object.keys(this.consultantReportBody)) {
                let responseKey = key[0].toLowerCase() + key.substr(1);
                this.consultantReportBody[key] = (this.isNotNull(body[responseKey]) ? body[responseKey] : "");
              }
              
              let mimetype = "data:image/png;base64";
              if(body.signature) {
                this.signaturePad.fromDataURL(mimetype + "," + body.signature);
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ConsultantReportComponent getConsultantReport " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ConsultantReportComponent getConsultantReport " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ConsultantReportComponent getConsultantReport " + e);
          }
        }
      );
  }

  consultReportSubmit() {
    if (!this.isValidated()) {
    } else {
      this.consultantReportBody.UserId = this.user.id;

      var signatureBase64 = this.signaturePad.toDataURL();
      var data = atob(signatureBase64.substring("data:image/png;base64,".length)),
        asArray = new Uint8Array(new ArrayBuffer(data.length));
      for (var i = 0, len = data.length; i < len; ++i) {
        asArray[i] = data.charCodeAt(i);
      }
      var signatureBlob = new Blob([asArray.buffer], { type: "image/png" });

      this.stickyBarService.showLoader("");

      const formdata: FormData = new FormData();
      for (let key of Object.keys(this.consultantReportBody)) {
        formdata.append(key, this.consultantReportBody[key]);
      }

      formdata.append('SignaturePath', signatureBlob);
      
      this.apiService
        .postFormDataWithBearer(this.apiConfig.completeConsultantReport, formdata)
        .subscribe(
          (res: any) => {
            try {
              this.stickyBarService.hideLoader("");
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
                setTimeout(() => {
                  if(this.appointmentId){
                    this.router.navigate([this.routeConfig.consultantPatientDetailsPath, this.appointmentId
                    ]);
                  }else{
                  this.router.navigate([this.routeConfig.consultantDashboardPath]);
                }
                  }, 1000);
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception ConsultantReportComponent consultReportSubmit " + e);
            }
          },
          (err: any) => {
            this.stickyBarService.hideLoader("");
            try {
              console.log("Error ConsultantReportComponent consultReportSubmit " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception ConsultantReportComponent consultReportSubmit " + e);
            }
          }
        );
    }
  }

  consultReportSave() {
    this.consultantReportBody.UserId = this.user.id;

    var signatureBase64 = this.signaturePad.toDataURL();
    var data = atob(signatureBase64.substring("data:image/png;base64,".length)),
      asArray = new Uint8Array(new ArrayBuffer(data.length));
    for (var i = 0, len = data.length; i < len; ++i) {
      asArray[i] = data.charCodeAt(i);
    }
    var signatureBlob = new Blob([asArray.buffer], { type: "image/png" });

    this.stickyBarService.showLoader("");

    const formdata: FormData = new FormData();
    for (let key of Object.keys(this.consultantReportBody)) {
      formdata.append(key, this.consultantReportBody[key]);
    }

    formdata.append('SignaturePath', signatureBlob);

    this.apiService
      .postFormDataWithBearer(this.apiConfig.createConsultantReport, formdata)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ConsultantReportComponent consultReportSubmit " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          this.stickyBarService.hideLoader("");
          try {
            console.log("Error ConsultantReportComponent consultReportSubmit " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ConsultantReportComponent consultReportSubmit " + e);
          }
        }
    );
  }

  clearSignature() {
    this.signaturePad.clear();
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["purpose"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Purpose).required(ValidationMessages.requiredField).obj);
    vO["allergies"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Allergies).required(ValidationMessages.requiredField).obj);
    vO["investigation"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Investigation).required(ValidationMessages.requiredField).obj);
    vO["reviewOfHistory"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.ReviewOfHistory).required(ValidationMessages.requiredField).obj);
    vO["medication"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Medication).required(ValidationMessages.requiredField).obj);
    vO["impression"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Impression).required(ValidationMessages.requiredField).obj);
    vO["notes"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Notes).required(ValidationMessages.requiredField).obj);
    vO["socialHistory"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.SocialHistory).required(ValidationMessages.requiredField).obj);
    vO["plan"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.Plan).required(ValidationMessages.requiredField).obj);
    vO["pastMedicalHistory"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.PastMedicalHistory).required(ValidationMessages.requiredField).obj);
    vO["familyHistory"] = this.validationService.getValue(this.validationService.setValue(this.consultantReportBody.FamilyHistory).required(ValidationMessages.requiredField).obj);
    this.eR = this.validationService.findError(vO);
    if (!this.eR && this.signaturePad.isEmpty()) {
      this.eR = {};
      this.eR.key = 'signature';
      this.eR.message = ValidationMessages.signatureRequired;
    }
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  isNotNull(value: string) {
    return value!=undefined && value!=null && value!="null";
  }

  consultReportCancle(){
    this.router.navigate([this.routeConfig.consultantDashboardPath]);
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.consultantDashboardPath
    ]);
  }
}
