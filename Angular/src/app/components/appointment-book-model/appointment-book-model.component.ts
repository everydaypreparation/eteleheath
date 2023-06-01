import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { RouteConfig } from 'src/app/configs/route.config';
import { Router } from '@angular/router';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { UserService } from 'src/app/services/user.service';
import { HelperService } from 'src/app/services/helper.service';
import { v4 as uuid } from 'uuid';
import { ApiService } from 'src/app/services/api.service';

export interface DialogData {
  id: string;
  doctorId: string;
  loginUser: string;
}

@Component({
  selector: 'app-appointment-book-model',
  templateUrl: './appointment-book-model.component.html',
  styleUrls: ['./appointment-book-model.component.scss']
})
export class AppointmentBookModelComponent implements OnInit {

  doctorId: string;
  loginUser: string;
  slotId: string;
  appointmentId: string;
  eR: any = null;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    public dialogRef: MatDialogRef<AppointmentBookModelComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private validationService: ValidationService,
    private stickyBarService: StickyBarService,
    private appointmentSlotService: AppointmentSlotService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private apiService: ApiService) {
     // Update view with given values
    this.doctorId = data.doctorId;
    this.loginUser = data.loginUser;
    this.slotId = data.id;   }

  appointmentBookForm = {
  "title": "",
  "agenda": "",
  "userId": "",
  "doctorId": "",
  "slotId": "",
  "referral": "",
  "meetingId": ""
  };

  ngOnInit(): void {
  }

  onConfirm(): void {
    // Close the dialog, return true
    if (!this.isValidated()) {
    }else{
    this.appointmentBookForm.slotId = this.slotId;
    this.appointmentBookForm.userId = this.loginUser;
    this.appointmentBookForm.doctorId = this.doctorId;
    this.appointmentBookForm.meetingId = uuid();
    this.appointmentBook(this.appointmentBookForm);
    this.dialogRef.close(true);
    }
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  appointmentBook(value: any): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .postAppointmentBookWithBearer(this.apiConfig.appointmentBook, value)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.appointmentId = res.result.id;
              this.getUser();
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AppointmentBookModelComponent appointmentBook " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentBookModelComponent appointmentBook " + JSON.stringify(err));
            this.appointmentSlotService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentBookModelComponent appointmentBook " + e);
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
              if (this.userService.userRole) {
                const role = this.userService.userRole;
                if (role == "PATIENT") {
                  // this.router.navigate([
                  //   this.routeConfig.patientPaymentPath, this.appointmentId, this.doctorId
                  // ]);
                  this.router.navigate([
                    this.routeConfig.patientAddInfoFormPath, this.appointmentId, this.doctorId
                  ]);
                } else if (role == "INSURANCE") {
                  // this.router.navigate([
                  //   this.routeConfig.insurancePaymentPath, this.appointmentId, this.doctorId
                  // ]);
                  this.router.navigate([
                    this.routeConfig.insurancePatientInfoFormPath, this.appointmentId, this.doctorId
                  ]);
                } else if (role == "MEDICALLEGAL") {
                  // this.router.navigate([
                  //   this.routeConfig.medicalLegalPaymentPath, this.appointmentId, this.doctorId
                  // ]);
                  this.router.navigate([
                    this.routeConfig.medicalLegalPatientInfoFormPath, this.appointmentId, this.doctorId
                  ]);
                }
              }
            }
          } catch (e) {
            console.log("Success Exception AppointmentBookModelComponent getUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AppointmentBookModelComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AppointmentBookModelComponent getUser " + e);
          }
        }
      );
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["title"] = this.validationService.getValue(this.validationService.setValue(this.appointmentBookForm.title).required(ValidationMessages.requiredField).obj);
    //vO["agenda"] = this.validationService.getValue(this.validationService.setValue(this.appointmentBookForm.agenda).required(ValidationMessages.requiredField).obj);

    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }
}
