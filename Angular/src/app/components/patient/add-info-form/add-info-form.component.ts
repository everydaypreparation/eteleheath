import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { ValidationService } from 'src/app/services/validation.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { SignaturePad } from 'angular2-signaturepad';
import { MatTabGroup } from '@angular/material/tabs';
import { DatePipe } from '@angular/common';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { CMapCompressionType } from 'pdfjs-dist';
import { RescheduleAppointmentModalComponent } from '../../reschedule-appointment-modal/reschedule-appointment-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { UploadPaymentDocumentModalComponent } from '../../upload-payment-document-modal/upload-payment-document-modal.component';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';
import { PropConfig } from 'src/app/configs/prop.config';

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
  selector: 'app-add-info-form',
  templateUrl: './add-info-form.component.html',
  styleUrls: ['./add-info-form.component.scss']
})
export class AddInfoFormComponent implements OnInit {

  @ViewChild('tabs') tabGroup: MatTabGroup;
  @ViewChild(SignaturePad) signaturePad: SignaturePad;
  @ViewChild('addressElement') addressElement: ElementRef;
  @ViewChild('doctorAddressElement') doctorAddressElement: ElementRef;

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
    "isPatient": true,
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
    "DiseaseDetails": "",
    "FamilyDoctorId": "",
    "DoctorFirstName": "",
    "DoctorLastName": "",
    "DoctorAddress": "",
    "DoctorCity": "",
    "DoctorState": "",
    "DoctorCountry": "",
    "DoctorPostalCodes": "",
    "DoctorTelePhone": "",
    "DoctorEmailID": "",
    "AppointmentId": "",
    "ConsentFormsMasterId": "",
    "ReportDocumentPaths": [],
    "RadiationDocumentPaths": [],
    "OtherDocumentPaths": [],
    "Signature": "",
    "DateConfirmation": "",
    "UserConsentId": "",
    "ConsultantReportsIds": [],
    "RelationshipWithPatient": "",
    "RepresentativeFirstName": "",
    "RepresentativeLastName": ""
  };

  details = {
    "appoinmentId": ""
  };

  appointmentId: any;
  familyDoctorList: any[] = [];
  selectedFamilyDoctor: any;
  familyDoctorNotRegisteredMsg: string = "";
  consentFormsMaster: any[];
  patientConsentForm: any;
  reportIdCount = 1;
  radiationIdCount = 1;
  othersIdCount = 1;
  formattedDateConfirmation = new Date();
  isActiveReports = true;
  isActiveRadiation = false;
  isActiveOthers = false;
  doctorId: any;
  maxDate = new Date();
  familyDetails = false;
  user: any;
  consultId: '';
  familyDoctorFirstName: any;
  consultantReports: any[] = [];
  patientInputShown = true;
  representativeInputShown = false;
  selectedEmailID: any;
  selectedTelePhone: any;
  timezones: any[] = [];
  userTimezone: any = {};

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
    private envAndUrlService: EnvAndUrlService,
    private dialog: MatDialog,
    private propConfig: PropConfig) { }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
    this.doctorId = this.activatedRoute.snapshot.params['doctorId'];
    this.getUserTimezoneOffset();
    this.getConsentFormsMaster();
    if (this.user.isIntake) {
      this.getDetails(this.appointmentId);
      this.getDocumentListByUserId();
    } else {
      this.getUserDetailsById();
      this.getDocumentListByUserId();
    }
    this.getConsultantReports();
    // this.getPlaceAutocomplete();
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      this.checkSlotAvailability(true);
      //this.submitConsentForm();
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.FirstName).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.LastName).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.EmailID).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);

    if (this.representativeInputShown) {
      vO["representativeFirstName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.RepresentativeFirstName).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
      vO["representativeLastName"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.RepresentativeLastName).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
      vO["relationshipWithPatient"] = this.validationService.getValue(this.validationService.setValue(this.intakeForm.RelationshipWithPatient).required(ValidationMessages.enterRelationshipWithPatient).alphabets(ValidationMessages.relationshipWithPatientNotString).rangeLength([2, 80], ValidationMessages.relationshipWithPatientLength).obj);
    }
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
      if (this.eR.key == 'representativeFirstName' || this.eR.key == 'representativeLastName' || this.eR.key == 'relationshipWithPatient') {
        this.tabGroup.selectedIndex = 0;
      }
      return false;
    } else {
      return true;
    }
  }

  familyDoctorNameInput(value: string, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
    if (value && value.length >= 2) {
      this.getFamilyDoctorInfoByName();
    } else if (!value || (value && value.length < 2)) {
      this.familyDoctorList = [];
    } else if (this.intakeForm.DoctorFirstName == "" && this.intakeForm.DoctorLastName == "") {
      this.familyDoctorList = [];
    }
  }

  checkfamilyDoctorDetails() {
    if (this.selectedFamilyDoctor) {
      if (this.intakeForm.DoctorFirstName.toLowerCase() != this.selectedFamilyDoctor.doctorFirstName.toLowerCase()
        || this.intakeForm.DoctorLastName.toLowerCase() != this.selectedFamilyDoctor.doctorLastName.toLowerCase()
        || this.intakeForm.DoctorEmailID.toLowerCase() != this.selectedFamilyDoctor.doctorEmailID.toLowerCase()) {
        this.familyDoctorNotRegisteredMsg = ValidationMessages.familyDoctorNotRegistered;
        this.intakeForm.FamilyDoctorId = "";
        this.familyDetails = false;
        //this.intakeForm.DoctorLastName = "";
        this.intakeForm.DoctorAddress = "";
        this.intakeForm.DoctorCity = "";
        this.intakeForm.DoctorState = "";
        this.intakeForm.DoctorCountry = "";
        this.intakeForm.DoctorPostalCodes = "";
        this.intakeForm.DoctorTelePhone = "";
        this.intakeForm.DoctorEmailID = "";
      } else {
        this.familyDoctorNotRegisteredMsg = "";
        this.familyDetails = true;
      }
    }
    else if (this.intakeForm.DoctorFirstName && !this.selectedFamilyDoctor) {
      if (this.intakeForm.FamilyDoctorId && this.familyDoctorFirstName && this.intakeForm.DoctorFirstName
        && this.intakeForm.DoctorFirstName.toLowerCase() == this.familyDoctorFirstName.toLowerCase()) {
        this.familyDoctorNotRegisteredMsg = "";
        this.familyDetails = true;
      } else {
        this.familyDoctorNotRegisteredMsg = ValidationMessages.familyDoctorNotRegistered;
        this.intakeForm.FamilyDoctorId = "";
        this.familyDetails = false;
      }
    }
  }

  getFamilyDoctorInfoByName(): void {
    const body = this.intakeForm;
    let searchParams = "";
    if (body.DoctorFirstName) {
      searchParams = searchParams + "?FirstName=" + body.DoctorFirstName;
    }
    if (body.DoctorLastName) {
      searchParams = searchParams + (searchParams ? "&" : "?") + "LastName=" + body.DoctorLastName;
    }
    searchParams = searchParams + "&UserId=" + this.user.id;
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getFamilyDoctorInfoByName + searchParams)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.familyDoctorList = res.result.items;
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

  displayDoctorFirstName(familyDoctor: any): string {
    return familyDoctor && familyDoctor.doctorFirstName ? familyDoctor.doctorFirstName : familyDoctor;
  }

  displayDoctorLastName(familyDoctor: any): string {
    return familyDoctor && familyDoctor.doctorLastName ? familyDoctor.doctorLastName : familyDoctor;
  }

  familyDoctorSelected(familyDoctor: any): void {
    this.familyDoctorNotRegisteredMsg = "";
    this.familyDetails = true;
    this.selectedFamilyDoctor = this.familyDoctorList.filter(fd => fd.familyDoctorId == familyDoctor.familyDoctorId)[0];

    this.intakeForm.DoctorFirstName = this.selectedFamilyDoctor.doctorFirstName;
    this.intakeForm.DoctorLastName = this.selectedFamilyDoctor.doctorLastName;
    this.intakeForm.DoctorAddress = this.selectedFamilyDoctor.doctorAddress;
    this.intakeForm.DoctorCity = this.selectedFamilyDoctor.doctorCity;
    this.intakeForm.DoctorState = this.selectedFamilyDoctor.doctorState;
    this.intakeForm.DoctorCountry = this.selectedFamilyDoctor.doctorCountry;
    this.intakeForm.DoctorPostalCodes = this.selectedFamilyDoctor.doctorPostalCodes;
    this.intakeForm.DoctorTelePhone = this.selectedFamilyDoctor.doctorTelePhone;
    this.intakeForm.DoctorEmailID = this.selectedFamilyDoctor.doctorEmailID;
    this.intakeForm.FamilyDoctorId = this.selectedFamilyDoctor.familyDoctorId;
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
              this.patientConsentForm = this.consentFormsMaster.find(c => c.title == "PATIENT CONSENT");
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent getConsentFormsMaster " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent getConsentFormsMaster " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent getConsentFormsMaster " + e);
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
        //console.log(this.intakeForm.ReportDocumentPaths);
      }
    });
  }

  // selectFile(event, reportType) {
  //   if(event.target.files.length > 0) {
  //     let docInfo = new documentInfo();
  //     let file = event.target.files[0];
  //     let type = file.type.split('/')[0];
  //     docInfo["file"] = file;
  //     docInfo["name"] = file.name;
  //     docInfo["ext"] = file.name.split('.')[1];
  //     docInfo["docType"]=type;

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

  //     if(reportType=='report') {
  //       docInfo["id"] = this.reportIdCount;
  //       docInfo["type"] = reportType;
  //       this.intakeForm.ReportDocumentPaths.push(docInfo);
  //       this.reportIdCount++;
  //     } else if(reportType=='radiation') {
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
    } else {
      let arr = this.intakeForm.OtherDocumentPaths.filter(f => f.id == reportDocId);
      if (arr.length > 0) {
        this.deletePatientRecord(reportDocId);
      }
    }
    this.intakeForm.ReportDocumentPaths = this.intakeForm.ReportDocumentPaths.filter(f => f.id != reportDocId);
    this.intakeForm.OtherDocumentPaths = this.intakeForm.OtherDocumentPaths.filter(fo => fo.id != reportDocId);

  }

  submitConsentForm(): void {
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

    this.stickyBarService.showLoader("");

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
              // this.createActiveCase();
              // setTimeout(() => {
              //   this.clearFields();
              // }, 1000);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent submitPatientConsentForm " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent submitPatientConsentForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent submitPatientConsentForm " + e);
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

    if (!body.ConsultId) {
      body.ConsultId = "";
    }
    const formdata: FormData = new FormData();
    for (let key of Object.keys(body)) {
      formdata.append(key, body[key]);
    }

    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.createConsultantReport, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.router.navigate([
                this.routeConfig.patientPaymentPath, this.appointmentId, this.doctorId
              ]);
              // setTimeout(() => {
              //   // this.router.navigate([
              //   //   this.routeConfig.patientDashboardPath
              //   // ]);
              //   this.router.navigate([
              //     this.routeConfig.patientPaymentPath, this.appointmentId, this.doctorId
              //   ]);
              // }, 1000);
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent createActiveCase " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent createActiveCase " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent createActiveCase " + e);
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
      //                         + (componentForm.street_number.value && componentForm.route.value ? " " : "")
      //                         + componentForm.route.value;
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


  getPlaceAutocompleteForDoctor() {
    const autocomplete = new google.maps.places.Autocomplete((document.getElementById('doctorAddressAutocomplete') as HTMLInputElement),
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
      //                         + (componentForm.street_number.value && componentForm.route.value ? " " : "")
      //                         + componentForm.route.value;
      this.intakeForm.DoctorAddress = place.formatted_address;
      this.intakeForm.DoctorPostalCodes = componentForm.postal_code.value;
      this.intakeForm.DoctorCity = componentForm.locality.value;
      this.intakeForm.DoctorState = componentForm.administrative_area_level_1.value;
      this.intakeForm.DoctorCountry = componentForm.country.value;

      if (place) {
        this.doctorAddressElement.nativeElement.focus();
        this.doctorAddressElement.nativeElement.blur();
      }
    });
  }

  clearFields(): void {
    for (let key of Object.keys(this.intakeForm)) {
      this.intakeForm[key] = "";
    }
    this.intakeForm.AppointmentId = "";
    this.intakeForm.FamilyDoctorId = "";
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
    if (type == "doctorEmail") {
      this.checkfamilyDoctorDetails();
    }
  }

  clearSignature() {
    this.signaturePad.clear();
  }

  homeClick(): void {
    if(!this.user.isPayment && this.user.isIntake && !this.user.isAppointment && this.user.isAllowtoNewBooking && !this.user.isMissedAppointment){
      this.router.navigate([this.routeConfig.findingConsultantPath]);
    }else{
    this.router.navigate([
      this.routeConfig.patientDashboardPath
    ]);
  }
    // this.router.navigate([
    //   this.routeConfig.patientDashboardPath
    // ]);
  }

  childTabHighlihtReports() {
    this.isActiveReports = true;
    this.isActiveRadiation = false;
    this.isActiveOthers = false;


  }

  childTabHighlihtRadiation() {
    this.isActiveRadiation = true;
    this.isActiveReports = false;
    this.isActiveOthers = false;
  }

  childTabHighlihtOthers() {
    this.isActiveOthers = true;
    this.isActiveRadiation = false;
    this.isActiveReports = false;
  }

  clearDOB(event: any) {
    event.stopPropagation();
    this.intakeForm.DateOfBirth = null;
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

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.user.id)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let body = res.result.patient;
              if (body.dateOfBirth) {
                this.intakeForm.DateOfBirth = new Date(body.dateOfBirth).toISOString();
              }
              if (body.name) {
                this.intakeForm.FirstName = body.name;
              }
              if (body.surname) {
                this.intakeForm.LastName = body.surname;
              }
              if (body.country) {
                this.intakeForm.Country = body.country;
              }
              if (body.city) {
                this.intakeForm.City = body.city;
              }
              if (body.state) {
                this.intakeForm.State = body.state;
              }
              if (body.address) {
                this.intakeForm.Address = body.address;
              }
              if (body.postalCode) {
                this.intakeForm.PostalCode = body.postalCode;
              }
              if (body.emailAddress) {
                this.intakeForm.EmailID = body.emailAddress;
              }
              if (body.phoneNumber) {
                this.intakeForm.TelePhone = body.phoneNumber;
              }
              if (body.gender) {
                this.intakeForm.Gender = body.gender;
              }
              if (body.doctorFirstName) {
                this.intakeForm.DoctorFirstName = body.doctorFirstName;
              }
              if (body.doctorLastName) {
                this.intakeForm.DoctorLastName = body.doctorLastName;
              }
              if (body.doctorAddress) {
                this.intakeForm.DoctorAddress = body.doctorAddress;
              }
              if (body.doctorCity) {
                this.intakeForm.DoctorCity = body.doctorCity;
              }
              if (body.doctorState) {
                this.intakeForm.DoctorState = body.doctorState;
              }
              if (body.doctorCountry) {
                this.intakeForm.DoctorCountry = body.doctorCountry;
              }
              if (body.doctorPostalCodes) {
                this.intakeForm.DoctorPostalCodes = body.doctorPostalCodes;
              }
              if (body.doctorTelePhone) {
                this.intakeForm.DoctorTelePhone = body.doctorTelePhone;
              }
              if (body.doctorEmailID) {
                this.intakeForm.DoctorEmailID = body.doctorEmailID;
              }
              if (body.familyDoctorId && body.familyDoctorId != this.envAndUrlService.UUID) {
                this.intakeForm.FamilyDoctorId = body.familyDoctorId;
                this.familyDetails = true;
                this.familyDoctorFirstName = body.doctorFirstName;
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent getUserDetailsById " + e);
          }
        }
      );
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
              this.intakeForm.EmailID = body.emailID;
              if (body.gender && body.Gender != 'null') {
                this.intakeForm.Gender = body.gender;
              }
              if (body.reasonForConsult && body.reasonForConsult != 'null') {
                this.intakeForm.ReasonForConsult = body.reasonForConsult;
              }
              if (body.diseaseDetails && body.diseaseDetails != 'null') {
                this.intakeForm.DiseaseDetails = body.diseaseDetails;
              }
              if (body.familyDoctorId && body.familyDoctorId != 'null') {
                this.intakeForm.FamilyDoctorId = body.familyDoctorId;
                this.familyDetails = true;
                this.familyDoctorFirstName = body.doctorFirstName;
              }
              if (body.doctorFirstName && body.doctorFirstName != 'null') {
                this.intakeForm.DoctorFirstName = body.doctorFirstName;
              }
              if (body.doctorLastName && body.doctorLastName != 'null') {
                this.intakeForm.DoctorLastName = body.doctorLastName;
              }
              if (body.doctorAddress && body.doctorAddress != 'null') {
                this.intakeForm.DoctorAddress = body.doctorAddress;
              }
              if (body.doctorCity && body.doctorCity != 'null') {
                this.intakeForm.DoctorCity = body.doctorCity;
              }
              if (body.doctorState && body.doctorState != 'null') {
                this.intakeForm.DoctorState = body.doctorState;
              }
              if (body.doctorCountry && body.doctorCountry != 'null') {
                this.intakeForm.DoctorCountry = body.doctorCountry;
              }
              if (body.doctorPostalCodes && body.doctorPostalCodes != 'null') {
                this.intakeForm.DoctorPostalCodes = body.doctorPostalCodes;
              }
              if (body.doctorTelePhone && body.doctorTelePhone != 'null') {
                this.intakeForm.DoctorTelePhone = body.doctorTelePhone;
              }
              if (body.doctorEmailID && body.doctorEmailID != 'null') {
                this.intakeForm.DoctorEmailID = body.doctorEmailID;
              }
              if (body.representativeFirstName && body.representativeFirstName != 'null') {
                this.intakeForm.RepresentativeFirstName = body.representativeFirstName;
                this.patientInputShown = false;
                this.representativeInputShown = true;
              }
              if (body.representativeLastName && body.representativeLastName != 'null') {
                this.intakeForm.RepresentativeLastName = body.representativeLastName;
              }
              if (body.relationshipWithPatient && body.relationshipWithPatient != 'null') {
                this.intakeForm.RelationshipWithPatient = body.relationshipWithPatient;
              }
              if (body.consultantReportsIds && body.consultantReportsIds != 'null') {
                this.intakeForm.ConsultantReportsIds = body.consultantReportsIds;
              }

              this.intakeForm.UserConsentId = body.userConsentId;
              this.consultId = body.consultId;
              this.intakeForm.AppointmentId = body.appointmentId;
              // for (let item of body.items) {
              //   this.uploadPreviewImage(item);
              // }
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

  getDocumentListByUserId(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDocumentListByUserId + this.user.id)
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
            console.log("Success Exception AddInfoFormComponent getDocumentListByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent getDocumentListByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent getDocumentListByUserId " + e);
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

  getConsultantReports(): void {
    let body = {
      "userId": this.user.id,
      "roleName": this.user.roleNames[0],
      "doctorId":this.doctorId
    }

    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.getPatientConsultantReportByPatientId, body)
      .subscribe(
        (res: any) => {
          console.log(res.result);
          try {
            if (res.result.statusCode == 200) {
              this.consultantReports = res.result.items;
              // for (let report of this.consultantReports) {
              //   let dateTime = this.formatDateTimeToUTC(report.appointmentDate);
              //   let latestDate = new DatePipe('en-US').transform(dateTime, 'dd MMM');
              //   let appointmentDate = latestDate.split(" ");
              //   report.date = appointmentDate[0];
              //   report.month = appointmentDate[1];
              // }
            }
          } catch (e) {
            console.log("Success Exception AddInfoFormComponent getConsultantReports " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddInfoFormComponent getConsultantReports " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddInfoFormComponent getConsultantReports " + e);
          }
        }
      );
  }

  addConsultReport(val: any, consultId: any) {
    let con = val.source.checked;
    if (con) {
      this.intakeForm.ConsultantReportsIds.push(consultId);
    } else {
      let index = this.intakeForm.ConsultantReportsIds.indexOf(consultId);
      this.intakeForm.ConsultantReportsIds.splice(index, 1);
    }
    console.log(this.intakeForm.ConsultantReportsIds);
  }

  canbeChecked(id): boolean {
    return this.intakeForm.ConsultantReportsIds.includes(id);
  }

  radioChange(name: any) {
    if (name == 'patient') {
      this.patientInputShown = true;
      this.representativeInputShown = false;
      this.intakeForm.EmailID = this.selectedEmailID;
      this.intakeForm.TelePhone = this.selectedTelePhone;
    } else {
      this.representativeInputShown = true;
      this.patientInputShown = false;
      this.selectedEmailID = this.intakeForm.EmailID;
      this.intakeForm.EmailID = "";

      this.selectedTelePhone = this.intakeForm.TelePhone;
      this.intakeForm.TelePhone = "";
    }
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              let userTimezone = this.timezones.filter(t => t.timeZoneId == this.user.timezone)[0];
              if (!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId == this.propConfig.defaultTimezoneId)[0];
              }
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                .replace("UTC", "").replace(":", "");
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }
}