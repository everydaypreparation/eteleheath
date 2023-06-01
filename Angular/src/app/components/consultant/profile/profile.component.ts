import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { UserService } from 'src/app/services/user.service';
import { ConfirmModalComponent, ConfirmModel } from '../../confirm-modal/confirm-modal.component';
import { AppointmentSlotModelComponent } from '../../appointment-slot-model/appointment-slot-model.component';
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { ValidationService } from 'src/app/services/validation.service';
import { DatePipe } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';

interface Gender {
  value: string;
  viewValue: string;
}
interface Designation {
  value: string;
  viewValue: string;
}
interface ConsultantTypes {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit , OnDestroy{
  @ViewChild('addressElement') addressElement: ElementRef;
  @ViewChild('availabilitySlotDv', { read: ElementRef }) public availabilitySlotDv: ElementRef<any>;

  constructor(private activatedRoute: ActivatedRoute, private stickyBarService: StickyBarService, private apiService: ApiService, private apiConfig: ApiConfig, private router: Router,
    private routeConfig: RouteConfig, private userService: UserService, private dialog: MatDialog, private propConfig: PropConfig,
    private validationService: ValidationService,private appointmentSlotService: AppointmentSlotService, private helperService: HelperService, private sanitizer: DomSanitizer) { }

    //userId: any = null;
    user: any = null;
    subscribeUser: any;
    loginUserId: any;
    availabilitySlotId: any;
    availabilitySlots: any[] = [];
    profileImage: any;
    profileUrl: any;
    eR: any = null;
    specialties: any[];
    subSpecialties: any[];
    maxDate = new Date(new Date().setFullYear(new Date().getFullYear()-18));

    genders: Gender[] = [
      { value: 'Male', viewValue: 'Male' },
      { value: 'Female', viewValue: 'Female' },
      { value: 'Other', viewValue: 'Other' }
    ];
  
    designations: Designation[] = [
      { value: 'Dr', viewValue: 'Dr' },
      { value: 'Mr', viewValue: 'Mr' },
      { value: 'Prof', viewValue: 'Prof' },
      { value: 'Mrs', viewValue: 'Mrs' },
      { value: 'Ms', viewValue: 'Ms' }
    ];
  
    consultantTypes: ConsultantTypes[] = [
      { value: 'Patient', viewValue: 'Patient' },
      { value: 'Medical Legal', viewValue: 'Medical Legal' }
    ];

    filteredTimezones = [];
    timezones: any[] = [];
    userTimezone: any = {};

    regForm = {
      "UserId": "",
      "Name": "",
      "Surname": "",
      "EmailAddress": "",
      "RoleNames": [
        this.propConfig.roles[1]
      ],
      "Timezone": this.propConfig.defaultTimezoneId,
      "Address": "",
      "City": "",
      "State": "",
      "Country": "",
      "PostalCode": "",
      "DateOfBirth": "",
      "Gender": "",
      "Credentials": "",
      "Title": "",
      "UploadProfilePicture": "",
      "PhoneNumber": "",
      "CurrentAffiliation": "",
      "HospitalAffiliation": "",
      "ProfessionalBio": "",
      "UndergraduateMedicalTraining": "",
      "OncologySpecialty": "",
      "OncologySubSpecialty": "",
      "MedicalAssociationMembership": "",
      "LicensingNumber": "",
      "DateConfirmed": "",
      "ConsultationType": "",
      "Certificate": "",
      "Residency1": "",
      "Residency2": "",
      "Fellowship": "",
      "ExperienceOrTraining": ""
    }

    apisPages: any = {
      userAvailabilitySlot: {
        page: 1,
        limit: 20,
        count: 0,
        more: true,
      }
    }

  ngOnInit(): void {
    this.getTimezoneList();

    this.user = this.userService.getUser();
    if(this.user){
      this.loginUserId = this.user.id;
      this.getUserDetailsById();
    }
    else{
      this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
        this.user = this.userService.getUser();
        this.loginUserId = this.user.id;
      });
    }
    this.getPlaceAutocomplete();
    this.getAvailabilitySlotById(false);
    this.getSpecialties();
    this.getSubSpecialties();
  }

  submitFormClick(): void {
    if (this.isValidated()) {
      if(this.loginUserId) {
        this.updateUser();
      }
    }
  }

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.consultantDashboardPath
    ]);
  }

  updateUser(): void {
    if (this.regForm.DateOfBirth && this.regForm.DateOfBirth != 'Invalid Date' && this.regForm.DateOfBirth != 'null') {
      this.regForm.DateOfBirth = new DatePipe('en-US').transform(this.regForm.DateOfBirth, 'MM/dd/yyyy');
    } else {
      this.regForm.DateOfBirth = '';
    }
    if (this.regForm.DateConfirmed && this.regForm.DateConfirmed != 'Invalid Date' && this.regForm.DateConfirmed != 'null') {
      this.regForm.DateConfirmed = new DatePipe('en-US').transform(this.regForm.DateConfirmed, 'MM/dd/yyyy');
    } else {
      this.regForm.DateConfirmed = '';
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
      .putFormDataWithBearer(this.apiConfig.updateConsultant, formdata)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              setTimeout(() => {
                this.getUser();
              }, 500);
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.getUserDetailsById();
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
    vO["title"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Title).required(ValidationMessages.enterTitle).obj);
    vO["firstName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Name).required(ValidationMessages.enterFirstName).alphabets(ValidationMessages.firstNameNotString).rangeLength([2, 40], ValidationMessages.firstNameLength).obj);
    vO["lastName"] = this.validationService.getValue(this.validationService.setValue(this.regForm.Surname).required(ValidationMessages.enterLastName).alphabets(ValidationMessages.lastNameNotString).rangeLength([2, 40], ValidationMessages.lastNameLength).obj);
    vO["email"] = this.validationService.getValue(this.validationService.setValue(this.regForm.EmailAddress).required(ValidationMessages.enterEmail).noSpecialChar(ValidationMessages.emailNotString).rangeLength([6, 128], ValidationMessages.emailLength).isMail(ValidationMessages.emailNotValid).obj);
    vO["hospitalAffiliation"] = this.validationService.getValue(this.validationService.setValue(this.regForm.HospitalAffiliation).required(ValidationMessages.enterHospitalAffiliation).obj);
    vO["oncologySpecialty"] = this.validationService.getValue(this.validationService.setValue(this.regForm.OncologySpecialty).required(ValidationMessages.enterOncologySpecialty).obj);
    vO["oncologySubSpecialty"] = this.validationService.getValue(this.validationService.setValue(this.regForm.OncologySubSpecialty).required(ValidationMessages.enterOncologySubSpecialty).obj);
    vO["consultationType"] = this.validationService.getValue(this.validationService.setValue(this.regForm.ConsultationType).required(ValidationMessages.consultationType).obj);
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
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent getUser " + e);
          }
          this.stickyBarService.hideLoader("");
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
      .getWithBearer(this.apiConfig.getUserDetailsById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.regForm = res.result;
              let body = res.result.consuntant;
              for (let key of Object.keys(this.regForm)) {
                let responseKey = key[0].toLowerCase() + key.substr(1);
                this.regForm[key] = (this.isNotNull(body[responseKey]) ? body[responseKey] : "");
              }
              this.regForm.UserId = this.loginUserId;
              if (body.dateConfirmed) {
                this.regForm.DateConfirmed = new Date(body.dateConfirmed).toISOString();
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

  isNotNull(value: string) {
    return value != undefined && value != null && value != "null";
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }


  getAvailabilitySlotById(isConcat: boolean): void {
    let pageLimit = this.apisPages.userAvailabilitySlot.limit;
    let pages = (isConcat ? this.apisPages.userAvailabilitySlot.page : 1);
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .getAvailabilitySlotWithBearer(this.apiConfig.getAllAppointmentSlotbyDoctorId + this.loginUserId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          console.log(res);
          try {
            if (res.result.statusCode == 200) {
              //this.availabilitySlots = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.userAvailabilitySlot);
                this.availabilitySlotDv.nativeElement.scrollTop = 0;
                this.availabilitySlots = [];
              }
              this.availabilitySlots = [...this.availabilitySlots, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.userAvailabilitySlot);
              
              for(let availabilitySlot of this.availabilitySlots) {
                let timezone = this.timezones.filter(t => t.timeZoneId==availabilitySlot.timeZone)[0];
                if(timezone) {
                  availabilitySlot.abbr = timezone.abbr;
                }
              }
              // this.availabilitySlots = this.availabilitySlots.filter(s => new Date(this.formatDateTimeToUTC(s.slotStartTime)).getTime() > new Date().getTime());
            }else{
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ViewUserComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewUserComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewUserComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  addAvailabilitySlot(){
    const dialogRef = this.dialog.open(AppointmentSlotModelComponent, {
      data: {userId: this.loginUserId , loginUserId: this.loginUserId, availabilitySlotId: null},
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getAvailabilitySlotById(false);
      }
    });
  }

  editvailabilitySlot(slotId: string){
    const dialogRef1 = this.dialog.open(AppointmentSlotModelComponent, {
      data: {userId: this.loginUserId , loginUserId: this.loginUserId, availabilitySlotId: slotId},
      autoFocus: false
    });

    dialogRef1.afterClosed().subscribe(dialogResult1 => {
      if (dialogResult1 == true) {
        this.getAvailabilitySlotById(false);
      }
    });
  }

  deleteAvailabilitySlot(slotId: string) {
    const message = ValidationMessages.deleteAvalabilitySlotConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.appointmentSlotService
          .deleteAvailabilitySlotWithBearer(this.apiConfig.deleteAppointmentSlotbyId + slotId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAvailabilitySlotById(false);
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception AppointmentSlotModelComponent deleteUserDetail " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error AppointmentSlotModelComponent deleteUserDetail " + JSON.stringify(err));
                this.appointmentSlotService.catchError(err);
              } catch (e) {
                console.log("Error Exception AppointmentSlotModelComponent deleteUserDetail " + e);
              }
            });
      }
    });
  }

  cancelClick(): void {
    this.router.navigate([this.routeConfig.consultantDashboardPath]);
  }

  getTimezoneList() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          this.getAvailabilitySlotById(false);
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

  formatTime24HourTo12Hour(time: any) {
    return this.helperService.formatTime24HourTo12Hour(time);
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

  getSpecialties(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getSpecialties)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.specialties = res.result.items;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent getSpecialties " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent getSpecialties " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent getSpecialties " + e);
          }
        }
      );
  }

  getSubSpecialties(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getSubSpecialties)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.subSpecialties = res.result.items;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ProfileComponent getSpecialties " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ProfileComponent getSpecialties " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ProfileComponent getSpecialties " + e);
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

  clearDOBDate() {
    event.stopPropagation();
    this.regForm.DateOfBirth = '';
  }

  clearDateConfirmed() {
    event.stopPropagation();
    this.regForm.DateConfirmed = '';
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  resetApisPage(pageObj: any) {
    pageObj.page = 1;
    pageObj.count = 0;
    pageObj.more = true;
  }
  
  paginationHandler(result: any, pageObj: any): void {
    if (!result.items) {
      pageObj.more = false;
    }
    pageObj.count += result.items.length;
    if (result.count == 0 || (result.count == pageObj.count)) {
      pageObj.more = false;
    }
  }
  
  scrollHandler(e: any, type: string): void {
    if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight) {
      if (type == "userAvailabilitySlot" && this.apisPages.userAvailabilitySlot.more) {
        this.apisPages.userAvailabilitySlot.page += 1;
        this.getAvailabilitySlotById(true);
      }
    }
  }

  ngOnDestroy(){
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
  }
}