import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { DomSanitizer } from '@angular/platform-browser';
import { DatePipe } from '@angular/common';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

interface Gender {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-patient-registration',
  templateUrl: './patient-registration.component.html',
  styleUrls: ['./patient-registration.component.scss']
})
export class PatientRegistrationComponent implements OnInit {
  @ViewChild('addressElement') addressElement: ElementRef;
  @ViewChild('doctorAddressElement') doctorAddressElement: ElementRef;

  userId: any = null;
  user: any = null;
  profileUrl: any;
  profileImage: any;
  maxDate = new Date();
  familyDoctorList: any[] = [];
  familyDoctorNotRegisteredMsg: string = "";
  selectedFamilyDoctor: any;

  genders: Gender[] = [
    { value: 'Male', viewValue: 'Male' },
    { value: 'Female', viewValue: 'Female' },
    { value: 'Other', viewValue: 'Other' }
  ];

  eR: any = null;

  regForm = {
    "UserId": "",
    "Name": "",
    "Surname": "",
    "EmailAddress": "",
    "RoleNames": [
      this.propConfig.roles[2]
    ],
    "Timezone": this.propConfig.defaultTimezoneId,
    "Address": "",
    "DateOfBirth": "",
    "Gender": "",
    "PostalCode": "",
    "UploadProfilePicture": "",
    "Country": "",//0,
    "State": "",//0,
    "City": "",//0,
    "PhoneNumber": "",//0,
    "DoctorFirstName": "",
    "DoctorLastName": "",//0,
    "DoctorAddress": "",
    "DoctorCountry": "",
    "DoctorState": "",
    "DoctorCity": "",
    "DoctorPostalCodes": "",
    "DoctorTelePhone": "",
    "DoctorEmailID": "",
    "ConsentMedicalInformationWithCancerCareProvider": "",
    "AdminNotes": "",
    "FamilyDoctorId": this.envAndUrlService.UUID
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private propConfig: PropConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,
    private envAndUrlService: EnvAndUrlService,
    private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.userId = this.activatedRoute.snapshot.params['userId'];
    if (this.userId) {
      this.regForm.UserId = this.userId;
      this.getUserDetailsById();
    }
    this.getPlaceAutocomplete();
    this.getDoctorPlaceAutocomplete();
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      if (this.userId) {
        this.updateUser();
      } else {
        this.createUser();
      }
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Name).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Surname).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.regForm.EmailAddress).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    if (this.regForm.DoctorEmailID) {
      vO["doctorEmailID"] = this.validationService.getValue(this.validationService.setValue(this.regForm.DoctorEmailID).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    }
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  createUser(): void {
    const body = this.regForm;
    if (this.regForm.DateOfBirth && this.regForm.DateOfBirth != 'Invalid Date' && this.regForm.DateOfBirth != 'null') {
      this.regForm.DateOfBirth = new DatePipe('en-US').transform(this.regForm.DateOfBirth, 'MM/dd/yyyy');
    } else {
      this.regForm.DateOfBirth = '';
    }
    const formdata: FormData = new FormData();
    for (let key of Object.keys(this.regForm)) {
      formdata.append(key, this.regForm[key]);
    }

    if (this.profileImage) {
      formdata.append('UploadProfilePicture', this.profileImage);
    }

    this.stickyBarService.showLoader("");
    this.apiService
      .postFormDataWithBearer(this.apiConfig.createPatient, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky("User registered successfully!");
              this.router.navigate([this.routeConfig.adminPatientsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientRegistrationComponent createUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientRegistrationComponent createUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientRegistrationComponent createUser " + e);
          }
        }
      );
  }

  updateUser(): void {
    if (this.regForm.DateOfBirth && this.regForm.DateOfBirth != 'Invalid Date' && this.regForm.DateOfBirth != 'null') {
      this.regForm.DateOfBirth = new DatePipe('en-US').transform(this.regForm.DateOfBirth, 'MM/dd/yyyy');
    } else {
      this.regForm.DateOfBirth = '';
    }
    const formdata: FormData = new FormData();
    for (let key of Object.keys(this.regForm)) {
      formdata.append(key, this.regForm[key]);
    }
    for (let key of Object.keys(this.regForm)) {
      //console.log(key + ": " + formdata.get(key));
    }

    if (this.profileImage) {
      formdata.append('UploadProfilePicture', this.profileImage);
    }
    this.stickyBarService.showLoader("");

    this.apiService
      .putFormDataWithBearer(this.apiConfig.updatePatient, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.router.navigate([this.routeConfig.adminPatientsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientRegistrationComponent updateUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientRegistrationComponent updateUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientRegistrationComponent updateUser " + e);
          }
        }
      );
  }

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.regForm = res.result;
              let body = res.result.patient;
              for (let key of Object.keys(this.regForm)) {
                let responseKey = key[0].toLowerCase() + key.substr(1);
                this.regForm[key] = (this.isNotNull(body[responseKey]) ? body[responseKey] : "");
              }
              this.regForm.UserId = this.userId;
              this.regForm.Timezone = this.propConfig.defaultTimezoneId;
              if (body.dateOfBirth) {
                this.regForm.DateOfBirth = new Date(body.dateOfBirth).toISOString();
              }
              if (body.uploadProfilePicture) {
                if(body.uploadProfilePicture.includes(this.apiConfig.matchesUrl)){
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${body.uploadProfilePicture}`);
                 }else{
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${body.uploadProfilePicture}`);
                 }
                //this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${body.uploadProfilePicture}`);
              }
              if(body.DoctorFirstName && body.DoctorLastName) {
                this.getFamilyDoctorInfoByName();
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientRegistrationComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientRegistrationComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientRegistrationComponent getUserDetailsById " + e);
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

      this.regForm.Address = componentForm.street_number.value
        + (componentForm.street_number.value && componentForm.route.value ? " " : "")
        + componentForm.route.value;
      this.regForm.PostalCode = componentForm.postal_code.value;
      this.regForm.City = componentForm.locality.value;
      this.regForm.State = componentForm.administrative_area_level_1.value;
      this.regForm.Country = componentForm.country.value;

      if (place) {
        this.addressElement.nativeElement.focus();
        this.addressElement.nativeElement.blur();
      }
    });
  }

  getDoctorPlaceAutocomplete() {
    const autocomplete = new google.maps.places.Autocomplete((document.getElementById('autocompleteDoctor') as HTMLInputElement),
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

      this.regForm.DoctorAddress = componentForm.street_number.value
        + (componentForm.street_number.value && componentForm.route.value ? " " : "")
        + componentForm.route.value;
      this.regForm.DoctorPostalCodes = componentForm.postal_code.value;
      this.regForm.DoctorCity = componentForm.locality.value;
      this.regForm.DoctorState = componentForm.administrative_area_level_1.value;
      this.regForm.DoctorCountry = componentForm.country.value;

      if (place) {
        this.doctorAddressElement.nativeElement.focus();
        this.doctorAddressElement.nativeElement.blur();
      }
    });
  }

  isNotNull(value: string) {
    return value != undefined && value != null && value != "null";
  }

  clearFields(): void {
    for (let key of Object.keys(this.regForm)) {
      this.regForm[key] = "";
    }
    this.regForm.RoleNames = [
      this.propConfig.roles[2]
    ];
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  onSelectProfileFile(event) {
    if (event.target.files && event.target.files[0]) {
      this.profileImage = event.target.files[0];
      let reader = new FileReader();

      reader.readAsDataURL(event.target.files[0]); // read file as data url

      reader.onload = (event) => { // called once readAsDataURL is completed
        this.profileUrl = event.target.result;
      }
    }
  }

  clearDOBDate() {
    event.stopPropagation();
    this.regForm.DateOfBirth = '';
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.adminDashboardPath
    ]);
  }

  backListClick(): void {
    this.router.navigate([this.routeConfig.adminPatientsPath]);
  }


  familyDoctorNameInput(value: string, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
    if (value && value.length >= 2) {
      this.getFamilyDoctorInfoByName();
    } else if (!value || (value && value.length < 2)) {
      this.familyDoctorList = [];
    } else if (this.regForm.DoctorFirstName == "" && this.regForm.DoctorLastName == "") {
      this.familyDoctorList = [];
    }
  }

  checkfamilyDoctorDetails() {
    if (this.selectedFamilyDoctor) {
      if (this.regForm.DoctorFirstName.toLowerCase() != this.selectedFamilyDoctor.doctorFirstName.toLowerCase()
        || this.regForm.DoctorLastName.toLowerCase() != this.selectedFamilyDoctor.doctorLastName.toLowerCase()
        || this.regForm.DoctorEmailID.toLowerCase() != this.selectedFamilyDoctor.doctorEmailID.toLowerCase()) {
        this.familyDoctorNotRegisteredMsg = ValidationMessages.familyDoctorNotRegistered;
        this.regForm.FamilyDoctorId = this.envAndUrlService.UUID;
      } else {
        this.familyDoctorNotRegisteredMsg = "";
      }
    } else if (this.regForm.DoctorFirstName && this.regForm.DoctorLastName) {
      this.familyDoctorNotRegisteredMsg = ValidationMessages.familyDoctorNotRegistered;
      this.regForm.FamilyDoctorId = this.envAndUrlService.UUID;
    }
  }

  getFamilyDoctorInfoByName(): void {
    const body = this.regForm;
    let searchParams = "";
    if (body.DoctorFirstName) {
      searchParams = searchParams + "?FirstName=" + body.DoctorFirstName;
    }
    if (body.DoctorLastName) {
      searchParams = searchParams + (searchParams ? "&" : "?") + "LastName=" + body.DoctorLastName;
    }

    searchParams = searchParams + "&UserId=" + this.envAndUrlService.UUID;
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getFamilyDoctorInfoByName + searchParams)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.familyDoctorList = res.result.items;
              this.selectedFamilyDoctor = this.familyDoctorList.filter(fd => fd.familyDoctorId == this.regForm.FamilyDoctorId)[0];
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
    this.selectedFamilyDoctor = this.familyDoctorList.filter(fd => fd.familyDoctorId == familyDoctor.familyDoctorId)[0];

    this.regForm.DoctorFirstName = this.selectedFamilyDoctor.doctorFirstName;
    this.regForm.DoctorLastName = this.selectedFamilyDoctor.doctorLastName;
    this.regForm.DoctorAddress = this.selectedFamilyDoctor.doctorAddress1;
    this.regForm.DoctorCity = this.selectedFamilyDoctor.doctorCity;
    this.regForm.DoctorState = this.selectedFamilyDoctor.doctorState;
    this.regForm.DoctorCountry = this.selectedFamilyDoctor.doctorCountry;
    this.regForm.DoctorPostalCodes = this.selectedFamilyDoctor.doctorPostalCodes;
    this.regForm.DoctorTelePhone = this.selectedFamilyDoctor.doctorTelePhone;
    this.regForm.DoctorEmailID = this.selectedFamilyDoctor.doctorEmailID;
    this.regForm.FamilyDoctorId = this.selectedFamilyDoctor.familyDoctorId;
  }
}
