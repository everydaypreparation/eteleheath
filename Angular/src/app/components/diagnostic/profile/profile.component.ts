import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { PropConfig } from 'src/app/configs/prop.config';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationService } from 'src/app/services/validation.service';
import { DatePipe } from '@angular/common';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { DomSanitizer } from '@angular/platform-browser';

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

  userId: any;
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
    //"isActive": true,
    "RoleNames": [
      this.propConfig.roles[6]
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
    "PhoneNumber": ""
  }
  timezones = [];
  filteredTimezones = [];

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private propConfig: PropConfig,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private sanitizer: DomSanitizer) { }

  ngOnInit(): void {
    this.getTimezoneList();
    let user = this.userService.getUser();
    if (user) {
      this.userId = user.id;
      this.getUserDetailsById();
    }
    this.getPlaceAutocomplete();
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      if (this.userId) {
        this.updateUser();
      }
    }
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
      .putFormDataWithBearer(this.apiConfig.updateDiagnostic, formdata)
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

  isValidated(): boolean {
    const vO: any = {};
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Name).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Surname).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.regForm.EmailAddress).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    vO["timezone"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Timezone).required(ValidationMessages.enterTimezone).obj);
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
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
              this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
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
  
  getUserDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserDetailsById + this.userId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.regForm = res.result;
              let body = res.result.daignostic;
              for (let key of Object.keys(this.regForm)) {
                let responseKey = key[0].toLowerCase() + key.substr(1);
                this.regForm[key] = (this.isNotNull(body[responseKey]) ? body[responseKey] : "");
              }
              this.regForm.UserId = this.userId;
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

  cancelClick(): void {
    this.router.navigate([this.routeConfig.diagnosticEmptyDashboardPath]);
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

    if (this.userService.getUser().isAppointment == false) {
      this.router.navigate([
        this.routeConfig.diagnosticDashboardPath
      ]);
    } else if (this.userService.getUser().isAppointment == true) {
      this.router.navigate([
        this.routeConfig.diagnosticDashboardDetailsPath
      ]);
    }

    // this.router.navigate([
    //   this.routeConfig.diagnosticDashboardPath
    // ]);
  }
}
