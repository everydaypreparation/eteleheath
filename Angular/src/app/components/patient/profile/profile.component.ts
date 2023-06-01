import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationService } from 'src/app/services/validation.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { DomSanitizer } from '@angular/platform-browser';
import { DatePipe } from '@angular/common';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

interface Gender {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  @ViewChild('addressElement') addressElement: ElementRef;
  @ViewChild('doctorAddressElement') doctorAddressElement: ElementRef;

  user: any = null;
  subscribeUser: any;
  loginUserId: any;
  profileUrl: any;
  profileImage: any;
  eR: any = null;
  familyDoctorList: any[] = [];
  familyDoctorNotRegisteredMsg: string = "";
  selectedFamilyDoctor: any;
  patientId: any;

  genders: Gender[] = [
    { value: 'Male', viewValue: 'Male' },
    { value: 'Female', viewValue: 'Female' },
    { value: 'Other', viewValue: 'Other' }
  ];
  timezones = [];
  filteredTimezones = [];
  formattedDateOfBirth: any;
  maxDate = new Date();

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
    "FamilyDoctorId": this.envAndUrlService.UUID
  }

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private activatedRoute: ActivatedRoute,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private dialog: MatDialog,
    private propConfig: PropConfig,
    private validationService: ValidationService,
    private helperService: HelperService,
    private envAndUrlService: EnvAndUrlService,
    private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.getTimezoneList();
    let user = this.userService.getUser();
    if(user){
      this.loginUserId = user.id;
      this.getUserDetailsById();
    }
    else{
      this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
        user = this.userService.getUser();
        this.loginUserId = user.id;
      });
    }
    this.getPlaceAutocomplete();
    this.getDoctorPlaceAutocomplete();
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      if(this.loginUserId) {
        this.updateUser();
      }
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Name).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Surname).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.regForm.EmailAddress).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    vO["timezone"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Timezone).required(ValidationMessages.enterTimezone).obj);
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

  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.regForm = res.result;
              let body = res.result.patient;
              this.patientId = body.id;
              for (let key of Object.keys(this.regForm)) {
                let responseKey = key[0].toLowerCase() + key.substr(1);
                this.regForm[key] = (this.isNotNull(body[responseKey]) ? body[responseKey] : "");
              }
              this.regForm.UserId = this.loginUserId;
              if(!this.regForm.Timezone) {
                this.regForm.Timezone = this.propConfig.defaultTimezoneId;
              }
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
            console.log("Success Exception ProfileComponent getUserDetailsById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent getUserDetailsById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent getUserDetailsById " + e);
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

    if (this.profileImage) {
      formdata.append('UploadProfilePicture', this.profileImage);
    }
    this.stickyBarService.showLoader("");

    this.apiService
      .putFormDataWithBearer(this.apiConfig.updatePatient, formdata)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              setTimeout(() => {
                this.getUser();
              }, 500);
            } else {              
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent updateUser " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent updateUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent updateUser " + e);
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
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
              console.log('pppp----');
              console.log(res.result);
              if (res.result.isAllowtoNewBooking) {
                this.router.navigate([this.routeConfig.findingConsultantPath]);
              } else {
                this.router.navigate([this.routeConfig.patientDashboardPath]);
              }
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent getUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent getUser " + e);
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
      this.regForm.Address = place.formatted_address;
      // this.regForm.Address = componentForm.street_number.value
      //                         + (componentForm.street_number.value && componentForm.route.value ? " " : "")
      //                         + componentForm.route.value;
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

  getDoctorPlaceAutocomplete() {
    const autocomplete = new google.maps.places.Autocomplete((document.getElementById('autocompleteDoctor') as HTMLInputElement),
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

      // this.regForm.DoctorAddress = componentForm.street_number.value
      //                         + (componentForm.street_number.value && componentForm.route.value ? " " : "")
      //                         + componentForm.route.value;
      this.regForm.DoctorAddress = place.formatted_address;
      this.regForm.DoctorPostalCodes =  componentForm.postal_code.value;
      this.regForm.DoctorCity = componentForm.locality.value;
      this.regForm.DoctorState = componentForm.administrative_area_level_1.value;
      this.regForm.DoctorCountry = componentForm.country.value;

      if(place) {
        this.doctorAddressElement.nativeElement.focus();
        this.doctorAddressElement.nativeElement.blur();
      }
    });
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  isNotNull(value: string) {
    return value != undefined && value != null && value != "null";
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

  cancelClick(): void {
    this.router.navigate([
      this.routeConfig.patientDashboardPath
    ]);
  }

  getTimezoneList() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              this.filteredTimezones = [... this.timezones];
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent getTimezoneList " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent getTimezoneList " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent getTimezoneList " + e);
          }
        }
      );
  }

  searchTimezone(value: string) {
    if(value) {
      this.filteredTimezones = this.timezones.filter(t => t.abbr.toLowerCase().includes(value.toLowerCase())
                                                  || t.utcOffset.toLowerCase().includes(value.toLowerCase())
                                                  || t.timeZoneId.toLowerCase().includes(value.toLowerCase()));
    } else {
      this.filteredTimezones = this.timezones;
    }
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.patientDashboardPath
    ]);
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
    searchParams = searchParams + "&UserId=" + this.loginUserId;
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
