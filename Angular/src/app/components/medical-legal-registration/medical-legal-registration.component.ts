import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiConfig } from 'src/app/configs/api.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { DatePipe } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';

interface Gender {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-medical-legal-registration',
  templateUrl: './medical-legal-registration.component.html',
  styleUrls: ['./medical-legal-registration.component.scss']
})
export class MedicalLegalRegistrationComponent implements OnInit {
  @ViewChild('addressElement') addressElement: ElementRef;

  userId: any;
  user: any = null;
  eR: any = null;
  profileUrl: any;
  profileImage: any;
  maxDate = new Date(new Date().setFullYear(new Date().getFullYear()-18));

  genders: Gender[] = [
    { value: 'Male', viewValue: 'Male' },
    { value: 'Female', viewValue: 'Female' },
    { value: 'Other', viewValue: 'Other' }
  ];

  regForm = {
    "UserId": "",
    "Name": "",
    "Surname": "",
    "EmailAddress": "",
    "RoleNames": [
      this.propConfig.roles[5]
    ],
    "Timezone": this.propConfig.defaultTimezoneId,
    "Address": "",
    "City": "",
    "State": "",
    "Country": "",
    "PostalCode": "",
    "DateOfBirth": "",
    "Gender": "",
    "Title": "",
    "UploadProfilePicture": "",
    "PhoneNumber": "",
    "Company": "",
    "RequestedOncologySubspecialty": "",
    "AdminNotes": "",
    "AmountDeposit": ""
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private propConfig: PropConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,
    private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.userId = this.activatedRoute.snapshot.params['userId'];
    if (this.userId) {
      this.regForm.UserId = this.userId;
      this.getUserDetailsById();
    }
    this.getPlaceAutocomplete();
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

    if(!this.regForm.AmountDeposit){
      this.regForm.AmountDeposit = "0";
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
      .postFormDataWithBearer(this.apiConfig.createMedicalLegal, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky("User registered successfully!");
              this.router.navigate([this.routeConfig.adminMedicalLegalsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception MedicalLegalRegistrationComponent createUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error MedicalLegalRegistrationComponent createUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception MedicalLegalRegistrationComponent createUser " + e);
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

    if(!this.regForm.AmountDeposit){
      this.regForm.AmountDeposit = "0";
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
      .putFormDataWithBearer(this.apiConfig.updateMedicalLegal, formdata)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.clearFields();
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.router.navigate([this.routeConfig.adminMedicalLegalsPath]);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception MedicalLegalRegistrationComponent updateUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error MedicalLegalRegistrationComponent updateUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception MedicalLegalRegistrationComponent updateUser " + e);
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
              let body = res.result.medicalLegal;
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
              if(body.amountDeposit == 0){
                this.regForm.AmountDeposit = "";
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception MedicalLegalRegistrationComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error MedicalLegalRegistrationComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception MedicalLegalRegistrationComponent getUserDetailsById " + e);
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
        street_number: {"value": "", "type": "short_name"},
        route: {"value": "", "type": "long_name"},
        locality: {"value": "", "type": "long_name"},
        administrative_area_level_1: {"value": "", "type": "short_name"},
        country: {"value": "", "type": "long_name"},
        postal_code: {"value": "", "type": "short_name"},
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
      this.regForm.PostalCode =  componentForm.postal_code.value;
      this.regForm.City = componentForm.locality.value;
      this.regForm.State = componentForm.administrative_area_level_1.value;
      this.regForm.Country = componentForm.country.value;

      if(place) {
        this.addressElement.nativeElement.focus();
        this.addressElement.nativeElement.blur();
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
      this.propConfig.roles[5]
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
    this.router.navigate([this.routeConfig.adminMedicalLegalsPath]);
    }
}
