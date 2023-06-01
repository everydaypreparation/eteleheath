import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { ValidationService } from 'src/app/services/validation.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { SignaturePad } from 'angular2-signaturepad';
import { MatTabGroup } from '@angular/material/tabs';
import { DatePipe } from '@angular/common';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { RescheduleAppointmentModalComponent } from '../../reschedule-appointment-modal/reschedule-appointment-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from 'src/app/services/helper.service';
import { UploadPaymentDocumentModalComponent } from '../../upload-payment-document-modal/upload-payment-document-modal.component';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

interface Gender {
  value: string;
  viewValue: string;
}

class documentInfo {
  id: number;
  ext: string;
  docType: string;
  name: string;
  file: File;
  preview: any;
}

@Component({
  selector: 'app-patient-info-form',
  templateUrl: './patient-info-form.component.html',
  styleUrls: ['./patient-info-form.component.scss']
})
export class PatientInfoFormComponent implements OnInit {
 
  @ViewChild('tabs') tabGroup: MatTabGroup;
  @ViewChild(SignaturePad) signaturePad: SignaturePad;
  @ViewChild('addressElement') addressElement: ElementRef;

  public signaturePadOptions: Object = {
    'minWidth': 1.5,
    'maxWidth': 1.5,
    'canvasWidth': 300,
    'canvasHeight': 100,
  };

  genders: Gender[] = [
    { value: 'Male', viewValue: 'Male' },
    { value: 'Female', viewValue: 'Female' },
    { value: 'Other', viewValue: 'Other' }
  ];

  eR: any = null;
  intakeForm = {
    "UserId": "",
    "isPatient": false,
    "FirstName": "",
    "LastName": "",
    "Address": "",
    "City": "",
    "State": "",
    "Country": "",
    "PostalCode": "",
    "TelePhone": "",
    "EmailID": "",
    "DateOfBirth": "",
    "Gender": "",
    "ReasonForConsult": "",
    "AppointmentId": "",
    "ConsentFormsMasterId": "",
    "ReportDocumentPaths": [],
    "RadiationDocumentPaths": [],
    "OtherDocumentPaths": [],
    "Signature": "",
    "DateConfirmation": "",
    "NoOfPages": "",
    "UserConsentId": ""
  };

  details = {
    "appoinmentId": ""
  };

  user: any;
  appointmentId: any;
  consentFormsMaster: any[];
  patientConsentForm: any;
  reportIdCount = 1;
  radiationIdCount = 1;
  othersIdCount = 1;
  formattedDateConfirmation = new Date();
  activeDocumentsChildTab = 'report';
  doctorId: any;
  maxDate = new Date();
  consultId: '';
  patientList: any;
  patientNotRegisteredMsg: any;
  selectedPatient: any;
  patientDetails = false;
  profileUrl: any;
  patientId: any;

  selectedIndex: number = 0;
  moveToSelectedTab(tabName: string) {
    for (let i = 0; i < document.querySelectorAll('.mat-tab-label-content').length; i++) {
      if ((<HTMLElement>document.querySelectorAll('.mat-tab-label-content')[i]).innerText == tabName) {
        (<HTMLElement>document.querySelectorAll('.mat-tab-label')[i]).click();
      }
    }
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,
    private userService: UserService,
    private dialog: MatDialog,
    private envAndUrlService: EnvAndUrlService,
    private helperService: HelperService) { }

  ngOnInit(): void {
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
    this.doctorId = this.activatedRoute.snapshot.params['doctorId'];
    this.user = this.userService.getUser();
    this.getConsentFormsMaster();
    if(this.user.isBookLater == false){
    this.checkSlotAvailability(false);
    }
    //this.checkSlotAvailability(false);
    this.getDetails(this.appointmentId);
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      //this.checkSlotAvailability(true);
      if(this.user.isBookLater == false){
      this.checkSlotAvailability(true);
      }else{
        this.submitConsentForm();
      }
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.FirstName).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.LastName).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    // vO["email"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.EmailID).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    vO["numberOfPages"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.NoOfPages).required(ValidationMessages.enterNumberOfPages).obj);

    this.eR = this.validationService.findError(vO);
    if (!this.eR && this.signaturePad.isEmpty()) {
      this.eR = {};
      this.eR.key = 'signature';
      this.eR.message = ValidationMessages.signatureRequired;
    }
    if (this.eR) {
      if (this.eR.key == 'firstName' || this.eR.key == 'lastName' || this.eR.key == 'email') {
        this.tabGroup.selectedIndex = 0;
      }

      if (this.eR.key == 'numberOfPages') {
        this.tabGroup.selectedIndex = 1;
      }
      return false;
    } else {
      return true;
    }
  }

  getConsentFormsMaster(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getConsentFormsMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.consentFormsMaster = res.result.items;
              this.patientConsentForm = this.consentFormsMaster.find(c => c.title == "LEGAL TEAM CONSENT");
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getConsentFormsMaster " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getConsentFormsMaster " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getConsentFormsMaster " + e);
          }
        }
      );
  }

  selectFile() {
    const dialogRef = this.dialog.open(UploadPaymentDocumentModalComponent);
    dialogRef.afterClosed().subscribe(event => {
      if (event && event[0] && event[0].target.files.length > 0) {
        let docInfo = new documentInfo();
        let file = event[0].target.files[0];
        let type = file.type.split('/')[0];
        docInfo["file"] = file;
        docInfo["name"] = event[1];
        docInfo["ext"] = file.name.split('.')[1];
        docInfo["docType"] = type;

        if (type == "application" && docInfo.ext == "pdf") {
          docInfo["preview"] = "assets/images/pdf.png";
        } else if (type == "application" && (docInfo.ext == "doc" || docInfo.ext == "docx")) {
          docInfo["preview"] = "assets/images/doc.png";
        } else if (type == "application" && (docInfo.ext == "xlsx" || docInfo.ext == "xls")) {
          docInfo["preview"] = "assets/images/xls.png";
        } else {
          docInfo["preview"] = "assets/images/dummy-ico.png";
        }

        // if (type == "image") {
        //   let reader: any = new FileReader();
        //   reader = new FileReader();
        //   reader.onload = (e: any) => {
        //     docInfo["preview"] = reader.result;
        //   };
        //   reader.readAsDataURL(file);
        // } else {
        //   return;
        // }

        docInfo["id"] = this.reportIdCount;
        docInfo["type"] = event[1];
        this.intakeForm.ReportDocumentPaths.push(docInfo);
        this.intakeForm.OtherDocumentPaths.push(docInfo);
        this.reportIdCount++;

        console.log(this.intakeForm.ReportDocumentPaths);
        // if (reportType == 'report') {
        //   docInfo["id"] = this.reportIdCount;
        //   docInfo["type"] = reportType;
        //   this.intakeForm.ReportDocumentPaths.push(docInfo);
        //   this.reportIdCount++;
        // } else if (reportType == 'radiation') {
        //   docInfo["id"] = this.radiationIdCount;
        //   docInfo["type"] = reportType;
        //   this.intakeForm.RadiationDocumentPaths.push(docInfo);
        //   this.radiationIdCount++;
        // } else {
        //   docInfo["id"] = this.othersIdCount;
        //   docInfo["type"] = reportType;
        //   this.intakeForm.OtherDocumentPaths.push(docInfo);
        //   this.othersIdCount++;
        // }
      }
    });
  }

  // selectFile(event, reportType) {
  //   if (event.target.files.length > 0) {
  //     let docInfo = new documentInfo();
  //     let file = event.target.files[0];
  //     let type = file.type.split('/')[0];
  //     docInfo["file"] = file;
  //     docInfo["name"] = file.name;
  //     docInfo["ext"] = file.name.split('.')[1];
  //     docInfo["docType"] = type;

  //     if (type == "application" && docInfo.ext == "pdf") {
  //       docInfo["preview"] = "assets/images/pdf.png";
  //     } else if (type == "application" && (docInfo.ext == "doc" || docInfo.ext == "docx")) {
  //       docInfo["preview"] = "assets/images/doc.png";
  //     } else if (type == "application" && (docInfo.ext == "xlsx" || docInfo.ext == "xls")) {
  //       docInfo["preview"] = "assets/images/xls.png";
  //     } else if (type == "image") {
  //       let reader: any = new FileReader();
  //       reader = new FileReader();
  //       reader.onload = (e: any) => {
  //         docInfo["preview"] = reader.result;
  //       };
  //       reader.readAsDataURL(file);
  //     } else {
  //       return;
  //     }

  //     if (reportType == 'report') {
  //       docInfo["id"] = this.reportIdCount;
  //       docInfo["type"] = reportType;
  //       this.intakeForm.ReportDocumentPaths.push(docInfo);
  //       this.reportIdCount++;
  //     } else if (reportType == 'radiation') {
  //       docInfo["id"] = this.radiationIdCount;
  //       docInfo["type"] = reportType;
  //       this.intakeForm.RadiationDocumentPaths.push(docInfo);
  //       this.radiationIdCount++;
  //     } else {
  //       docInfo["id"] = this.othersIdCount;
  //       docInfo["type"] = reportType;
  //       this.intakeForm.OtherDocumentPaths.push(docInfo);
  //       this.othersIdCount++;
  //     }
  //   }
  // }

  removeFile(reportDocId) {
    let arr = this.intakeForm.ReportDocumentPaths.filter(f => f.id == reportDocId);
    if (arr.length > 0) {
     // this.deletePatientRecord(reportDocId);
    }else{
      let arr = this.intakeForm.OtherDocumentPaths.filter(f => f.id == reportDocId);
      if (arr.length > 0) {
        this.deletePatientRecord(reportDocId);
      }
    }
    this.intakeForm.ReportDocumentPaths = this.intakeForm.ReportDocumentPaths.filter(f => f.id != reportDocId);
    this.intakeForm.OtherDocumentPaths = this.intakeForm.OtherDocumentPaths.filter(fo => fo.id != reportDocId);

    // if (reportType == 'report') {
    //   this.intakeForm.ReportDocumentPaths = this.intakeForm.ReportDocumentPaths.filter(f => f.id != reportDocId || f.type != reportType);
    // } else if (reportType == 'radiation') {
    //   this.intakeForm.RadiationDocumentPaths = this.intakeForm.RadiationDocumentPaths.filter(f => f.id != reportDocId || f.type != reportType);
    // } else {
    //   this.intakeForm.OtherDocumentPaths = this.intakeForm.OtherDocumentPaths.filter(f => f.id != reportDocId || f.type != reportType);
    // }
  }

  submitConsentForm(): void {
    this.stickyBarService.showLoader("");
    const body = this.intakeForm;
    if (!this.intakeForm.UserConsentId) {
      body.UserConsentId = this.envAndUrlService.UUID;
    }

    body.AppointmentId = this.appointmentId;
    if (body.DateOfBirth && body.DateOfBirth != 'null') {
      body.DateOfBirth = new DatePipe('en-US').transform(this.intakeForm.DateOfBirth, 'MM/dd/yyyy');
    } else {
      body.DateOfBirth = '';
    }
    body.DateConfirmation = new DatePipe('en-US').transform(this.formattedDateConfirmation, 'MM/dd/yyyy');

    body.UserId = this.userService.getUser().id;
    body.ConsentFormsMasterId = this.patientConsentForm.consentFormsId;

    var signatureBase64 = this.signaturePad.toDataURL();
    var data = atob(signatureBase64.substring("data:image/png;base64,".length)),
      asArray = new Uint8Array(new ArrayBuffer(data.length));
    for (var i = 0, len = data.length; i < len; ++i) {
      asArray[i] = data.charCodeAt(i);
    }
    var signatureBlob = new Blob([asArray.buffer], { type: "image/png" });

    const formdata: FormData = new FormData();
    for (let key of Object.keys(this.intakeForm)) {
      formdata.append(key, body[key]);
    }

    for (let reportDoc of body.ReportDocumentPaths) {
      formdata.append('ReportDocumentPaths', reportDoc.file, reportDoc.name + '.' + reportDoc.ext);
    }

    // for (let radiationDoc of body.RadiationDocumentPaths) {
    //   formdata.append('RadiationDocumentPaths', radiationDoc.file);
    // }
    // for (let otherDoc of body.OtherDocumentPaths) {
    //   formdata.append('OtherDocumentPaths', otherDoc.file);
    // }

    formdata.append('Signature', signatureBlob);
    this.apiService
      .postFormDataWithBearer(this.apiConfig.submitPatientConsentForm, formdata)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.getUser();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent submitPatientConsentForm " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent submitPatientConsentForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent submitPatientConsentForm " + e);
          }
        }
      );
  }

  getUser(): void {
    const userId = localStorage.getItem("userId");
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUser + userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
              this.createActiveCase();
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getUser " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getUser " + e);
          }
        }
      );
  }

  createActiveCase() {
    const body = {
      "AppointmentId": this.appointmentId,
      "UserId": this.doctorId,
      //"consultId": this.consultId
      "ConsultId": this.consultId
    };

    if(!body.ConsultId){
      body.ConsultId = "";
    }
    const formdata: FormData = new FormData();
    for (let key of Object.keys(body)) {
      formdata.append(key, body[key]);
      //console.log(key, body[key]);
    }

    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.createConsultantReport, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              //this.clearFields();
              this.router.navigate([
                this.routeConfig.insurancePaymentPath, this.appointmentId, this.doctorId
              ]);
            //   if(this.user.isBookLater == false){
            //   this.router.navigate([
            //     this.routeConfig.insurancePaymentPath, this.appointmentId, this.doctorId
            //   ]);
            // }else{
            //   this.router.navigate([this.routeConfig.insuranceDashboardPath]);
            // }
              //this.router.navigate([this.routeConfig.insuranceDashboardPath]);
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent createActiveCase " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent createActiveCase " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent createActiveCase " + e);
          }
        }
      );
  }

  getPlaceAutocomplete() {
    const autocomplete = new google.maps.places.Autocomplete((document.getElementById('autocomplete') as HTMLInputElement),
      {
        types: ['geocode']  // 'establishment' / 'address' / 'geocode'
      });

    google.maps.event.addListener(autocomplete, 'place_changed', () => {

      const componentForm = {
        street_number: { "value": "", "type": "short_name" },
        route: { "value": "", "type": "long_name" },
        locality: { "value": "", "type": "long_name" },
        administrative_area_level_1: { "value": "", "type": "short_name" },
        country: { "value": "", "type": "long_name" },
        postal_code: { "value": "", "type": "short_name" },
      };

      const place = autocomplete.getPlace();

      for (const component of place.address_components) {
        const addressType = component.types[0];

        if (componentForm[addressType]) {
          componentForm[addressType].value = component[componentForm[addressType].type];
        }
      }

      // this.intakeForm.Address1 = componentForm.street_number.value
      //   + (componentForm.street_number.value && componentForm.route.value ? " " : "")
      //   + componentForm.route.value;
      this.intakeForm.Address = place.formatted_address;
      this.intakeForm.PostalCode = componentForm.postal_code.value;
      this.intakeForm.City = componentForm.locality.value;
      this.intakeForm.State = componentForm.administrative_area_level_1.value;
      this.intakeForm.Country = componentForm.country.value;

      if (place) {
        this.addressElement.nativeElement.focus();
        this.addressElement.nativeElement.blur();
      }
    });
  }

  checkSlotAvailability(isSubmit: boolean): void {
    let body = {
      "AppointmentId": this.appointmentId
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.IsSlotAvailable, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!res.result.isAvailable) {
                this.showAvailableSlotsToReschedule(isSubmit);
              } else {
                if (isSubmit) {
                  this.submitConsentForm();
                }
              }
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent checkSlotAvailability " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent checkSlotAvailability " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent checkSlotAvailability " + e);
          }
        }
      );
  }

  showAvailableSlotsToReschedule(isSubmit: boolean) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": this.appointmentId, "doctorId": this.doctorId, "slotNotAvailableMessage": ValidationMessages.slotNotAvailable }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult) {
        if (isSubmit) {
          this.submitConsentForm();
        }
      }
    });
  }

  clearFields(): void {
    for (let key of Object.keys(this.intakeForm)) {
      this.intakeForm[key] = "";
    }
    this.intakeForm.AppointmentId = "";
    this.intakeForm.ConsentFormsMasterId = "";
    this.intakeForm.ReportDocumentPaths = [];
    this.intakeForm.RadiationDocumentPaths = [];
    this.intakeForm.OtherDocumentPaths = [];
    this.clearSignature();
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  clearSignature() {
    this.signaturePad.clear();
  }

  homeClick(): void {
    this.router.navigate([this.routeConfig.insuranceDashboardPath]);
    //this.helperService.navigateInsuranceUser(this.user);
  }

  documentsChildTabClick(tabLabel) {
    this.activeDocumentsChildTab = tabLabel;
  }

  clearDOB(event: any) {
    event.stopPropagation();
    this.intakeForm.DateOfBirth = null;
  }

  getDetails(appointmentId: any) {
    this.stickyBarService.showLoader("");
    this.details.appoinmentId = appointmentId;
    this.apiService.postWithBearer(this.apiConfig.getDetails, this.details)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result;
              if (body.dateOfBirth && body.dateOfBirth != 'Invalid Date' && body.dateOfBirth != 'null') {
                this.intakeForm.DateOfBirth = new DatePipe('en-US').transform(body.dateOfBirth, 'yyyy-MM-dd');
              } else {
                this.intakeForm.DateOfBirth = '';
              }
              this.intakeForm.DateConfirmation = new DatePipe('en-US').transform(body.dateConfirmation, 'MM/dd/yyyy');
              this.intakeForm.UserId = body.userId;
              this.intakeForm.ConsentFormsMasterId = body.consentFormsId;
              let mimetype = "data:image/png;base64";
              if (body.signature) {
                this.signaturePad.fromDataURL(mimetype + "," + body.signature);
              }
              this.intakeForm.FirstName = body.firstName;
              this.intakeForm.LastName = body.lastName;
              if (body.address && body.address != 'null') {
              this.intakeForm.Address = body.address;
              }
              // this.intakeForm.Address2 = body.address2;
              if (body.city && body.city != 'null') {
              this.intakeForm.City = body.city;
              }
              if (body.state && body.state != 'null') {
              this.intakeForm.State = body.state;
              }
              if (body.country && body.country != 'null') {
              this.intakeForm.Country = body.country;
              }
              if (body.postalCode && body.postalCode != 'null') {
              this.intakeForm.PostalCode = body.postalCode;
              }
              if (body.telePhone && body.telePhone != 'null') {
              this.intakeForm.TelePhone = body.telePhone;
              }
              if (body.emailID && body.emailID != 'null') {
                this.intakeForm.EmailID = body.emailID;
              }
              if (body.gender && body.Gender != 'null') {
              this.intakeForm.Gender = body.gender;
              }
              if (body.reasonForConsult && body.reasonForConsult != 'null') {
                this.intakeForm.ReasonForConsult = body.reasonForConsult;
              }
              this.intakeForm.AppointmentId = body.appointmentId;
              this.intakeForm.NoOfPages = body.noOfPages;
              this.intakeForm.UserConsentId = body.userConsentId;
              this.consultId = body.consultId;
              // if(body.patientId){
              //   this.patientId = body.patientId;
              //   this.getDocumentListByUserId(body.patientId);
              // }
              for (let item of body.items) {
                this.uploadPreviewImage(item);
              }
              // this.intakeForm = body;
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getDetails " + e);
          }
        }
      );
  }

  getDocumentListByUserId(patientId: any): void {
    this.intakeForm.OtherDocumentPaths = [];
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDocumentListByUserId +patientId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let documents = res.result.items;
              for (let item of documents) {
                this.uploadPreviewImage(item);
              }
            }
          } catch (e) {
            console.log("Success Exception PatientInfoFormComponent getDocumentListByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientInfoFormComponent getDocumentListByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientInfoFormComponent getDocumentListByUserId " + e);
          }
        }
      );
  }

  uploadPreviewImage(item: any) {
    let docInfo = new documentInfo();
    docInfo["name"] = item.documentName.split('.')[0];
    docInfo["ext"] = item.documentName.split('.')[1];
    docInfo["docType"] = 'application';

    if (docInfo.ext == "pdf") {
      docInfo["preview"] = "assets/images/pdf.png";
    } else if ((docInfo.ext == "doc" || docInfo.ext == "docx")) {
      docInfo["preview"] = "assets/images/doc.png";
    } else if ((docInfo.ext == "xlsx" || docInfo.ext == "xls")) {
      docInfo["preview"] = "assets/images/xls.png";
    } else {
      docInfo["preview"] = "assets/images/dummy-ico.png";
    }

    docInfo["id"] = item.documentId;
    docInfo["type"] = item.documentName.split('.')[0];
    this.intakeForm.OtherDocumentPaths.push(docInfo);
  }

  isNotNull(value: string) {
    return value != undefined && value != null && value != "null";
  }

  deletePatientRecord(documentId) {
    this.stickyBarService.showLoader("");
    this.apiService
      .deleteWithBearer(this.apiConfig.deleteDocumentById + documentId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent deletePatientRecord " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent deletePatientRecord " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent deletePatientRecord " + e);
          }
        }
      );
  }

  clearDOBDate() {
    event.stopPropagation();
    this.intakeForm.DateOfBirth = '';
  }

  patientNameInput(value: string, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
    if (value && value.length >= 2) {
      this.getPatientInfoByName();
    } else if (!value || (value && value.length < 2)) {
      this.patientList = [];
    } else if (this.intakeForm.FirstName == "" && this.intakeForm.LastName == "") {
      this.patientList = [];
    }
  }

  checkPatientDetails() {
    if (this.selectedPatient) {
      if (this.intakeForm.FirstName.toLowerCase() != this.selectedPatient.doctorFirstName.toLowerCase()
        || this.intakeForm.LastName.toLowerCase() != this.selectedPatient.doctorLastName.toLowerCase()
        || this.intakeForm.EmailID.toLowerCase() != this.selectedPatient.doctorEmailID.toLowerCase()) {
        this.patientNotRegisteredMsg = ValidationMessages.patientNotRegistered;
        this.patientId = "";
        this.intakeForm.OtherDocumentPaths = [];
        this.detailsEmpty();
        // this.intakeForm.FamilyDoctorId = "";
        this.patientDetails = false;
      } else {
        this.patientNotRegisteredMsg = "";
        this.patientDetails = true;
      }
    } else if (this.intakeForm.FirstName && this.intakeForm.LastName) {
      this.patientNotRegisteredMsg = ValidationMessages.patientNotRegistered;
      this.patientDetails = false;
      this.patientId = "";
      this.intakeForm.OtherDocumentPaths = [];
      this.detailsEmpty();
    }
  }

  detailsEmpty() {
    this.intakeForm.Address = "";
    this.intakeForm.City = "";
    this.intakeForm.State = "";
    this.intakeForm.Country = "";
    this.intakeForm.PostalCode = "";
    this.intakeForm.TelePhone = "";
    this.intakeForm.EmailID = "";
    this.intakeForm.DateOfBirth = "";
    this.intakeForm.Gender = "";
    this.intakeForm.ReasonForConsult = "";
  }

  getPatientInfoByName(): void {
    const body = this.intakeForm;
    let searchParams = "";
    if (body.FirstName) {
      searchParams = searchParams + "?FirstName=" + body.FirstName + "&IsPatient=true";
    }
    if (body.LastName) {
      searchParams = searchParams + (searchParams ? "&" : "?") + "LastName=" + body.LastName + "&IsPatient=true";
    }
    searchParams = searchParams + "&UserId=" + this.user.id;

    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getPatientInfoByName + searchParams)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.patientList = res.result.items;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent getFamilyDoctorInfoByName " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent getFamilyDoctorInfoByName " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent getFamilyDoctorInfoByName " + e);
          }
        }
      );
  }

  displayPatientFirstName(patient: any): string {
    return patient && patient.doctorFirstName ? patient.doctorFirstName : patient;
  }

  displayPatientLastName(patient: any): string {
    return patient && patient.doctorLastName ? patient.doctorLastName : patient;
  }

  patientSelected(patient: any): void {
   
    this.patientNotRegisteredMsg = "";
    this.patientDetails = true;
    this.selectedPatient = this.patientList.filter(fd => fd.doctorEmailID == patient.doctorEmailID)[0];
    this.intakeForm.FirstName = this.selectedPatient.doctorFirstName;
    this.intakeForm.LastName = this.selectedPatient.doctorLastName;
    this.intakeForm.Address = this.selectedPatient.doctorAddress1;
    this.intakeForm.City = this.selectedPatient.doctorCity;
    this.intakeForm.State = this.selectedPatient.doctorState;
    this.intakeForm.Country = this.selectedPatient.doctorCountry;
    this.intakeForm.PostalCode = this.selectedPatient.doctorPostalCodes;
    this.intakeForm.TelePhone = this.selectedPatient.doctorTelePhone;
    this.intakeForm.EmailID = this.selectedPatient.doctorEmailID;
    this.patientId = this.selectedPatient.familyDoctorId;
    if (this.selectedPatient.doctorDOB && this.selectedPatient.doctorDOB != 'Invalid Date' && this.selectedPatient.doctorDOB != 'null') {
      this.intakeForm.DateOfBirth = new DatePipe('en-US').transform(this.selectedPatient.doctorDOB, 'yyyy-MM-dd');
    } else {
      this.intakeForm.DateOfBirth = '';
    }
    this.intakeForm.Gender = this.selectedPatient.doctorGender;
    if(this.patientId){
      this.getDocumentListByUserId(this.patientId);
    }
    //this.intakeForm.FamilyDoctorId = this.selectedPatient.familyDoctorId;
  }
}
