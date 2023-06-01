import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { UserService } from 'src/app/services/user.service';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ValidationService } from 'src/app/services/validation.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from 'src/app/services/helper.service';

interface Food {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-finding-consultant',
  templateUrl: './finding-consultant.component.html',
  styleUrls: ['./finding-consultant.component.scss']
})
export class FindingConsultantComponent implements OnInit {

  specialties: any[];
  selectedSpecialtyMdl: string = "";
  selectedSearchCriteriaMdl: string = "";
  consultantOrHospitalNameMdl: string = "";

  IsImpersonating: boolean = false;
  rememberMe: boolean = false;

  user: any = null;
  roleName: any;

  eR: any = null;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private validationService: ValidationService,
    private dialog: MatDialog,
    private helperService: HelperService
) {

  this.IsImpersonating = localStorage.getItem("IsImpersonating") ? localStorage.getItem("IsImpersonating").toLowerCase() == "true" : false;
  this.rememberMe = localStorage.getItem("rememberMe") == "true" ? true : false;
 }

  ngOnInit(): void {
    let user = this.userService.getUser();
    if (user) {
      this.roleName = user.roleNames[0];
    }
    this.getSpecialties();
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
            console.log("Success Exception FindingConsultantComponent getSpecialties " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error FindingConsultantComponent getSpecialties " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception FindingConsultantComponent getSpecialties " + e);
          }
        }
      );
  }

  searchClick(): void {
    if (this.isValidated()) {
      this.searchConsultant();
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    if (this.consultantOrHospitalNameMdl) {
      vO["selectedSearchCriteria"] = this.validationService.getValue(this.validationService.setValue(this.selectedSearchCriteriaMdl).required(ValidationMessages.selectSearchCriteria).obj);
    }
    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  searchConsultant(): void {
    let user = this.userService.getUser();
    let basePath = "";
    if (this.userService.userRole) {
      const role = this.userService.userRole;
      if (role == "PATIENT") {
        basePath = this.routeConfig.patientDoctorListPath;
      } else if (role == "INSURANCE") {
        basePath = this.routeConfig.insuranceConsultantsListPath;
      } else if (role == "MEDICALLEGAL") {
        basePath = this.routeConfig.medicalLegalConsultantsListPath;
      }
    }

    if (basePath) {
      const queryParamsObj: any = {};
      if (this.selectedSpecialtyMdl) {
        queryParamsObj.specialtyName = this.selectedSpecialtyMdl;
      }
      if (this.selectedSearchCriteriaMdl && this.selectedSearchCriteriaMdl == 'consultant' && this.consultantOrHospitalNameMdl) {
        queryParamsObj.consultantName = this.consultantOrHospitalNameMdl;
      } else if (this.selectedSearchCriteriaMdl && this.selectedSearchCriteriaMdl == 'hospital' && this.consultantOrHospitalNameMdl) {
        queryParamsObj.hospitalName = this.consultantOrHospitalNameMdl;
      }
      if (Object.keys(queryParamsObj).length > 0) {
        this.router.navigate([basePath], { queryParams: queryParamsObj });
      } else {
        this.router.navigate([basePath]);
      }
    }
  }

  validatorPress(e: any, type: any): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
    const charCode = e.which ? e.which : e.keyCode;
    if (charCode == 13) {
      this.searchClick();
    }
  }

  clearFields(): void {
    this.selectedSpecialtyMdl = "";
    this.selectedSearchCriteriaMdl = "";
    this.consultantOrHospitalNameMdl = "";
  }

  logoutClick(): void {

    const message = ValidationMessages.confirmLogout;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        if (this.userService.clearUserData()) {
          this.router.navigate([this.routeConfig.signInPath]);
        }
      }
    });
  }

  stopImpersonation() {

    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.StopImpersonation + this.rememberMe).subscribe(
      (res: any) => {
        try {
          this.stickyBarService.hideLoader("");
          if (res.result.statusCode == 200) {

            localStorage.setItem("app-tn", res.result.accessToken);
            localStorage.IsImpersonating = false;
            localStorage.setItem("userId", localStorage.getItem("Impersonator"));
            this.router.navigate([this.routeConfig.signInPath]);

            this.stickyBarService.showSuccessSticky(res.result.message);
          } else {
            this.stickyBarService.showErrorSticky(res.result.message);
          }
        } catch (e) {
          console.log("Success Exception FindingConsultantComponent stopImpersonation " + e);
        }
      },
      (err: any) => {
        try {
          this.stickyBarService.hideLoader("");
          console.log("Error FindingConsultantComponent stopImpersonation " + JSON.stringify(err));
          this.apiService.catchError(err);
        } catch (e) {
          console.log("Error Exception FindingConsultantComponent stopImpersonation " + e);
        }
      });
  }
}
