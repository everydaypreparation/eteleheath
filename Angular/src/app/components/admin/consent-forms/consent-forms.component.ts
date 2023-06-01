import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { RouteConfig } from 'src/app/configs/route.config';
import { Router } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-consent-forms',
  templateUrl: './consent-forms.component.html',
  styleUrls: ['./consent-forms.component.scss']
})
export class ConsentFormsComponent implements OnInit {

  consentForms: any[];
  user: any;

  displayedColumns: string[] = ['#', 'title', 'subTitle', 'shortDescription', 'description', 'action'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private userService: UserService,
    private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private dialog: MatDialog,
    private helperService: HelperService) { 
    this.userService.setNav("consentforms");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.getAllConsentForms();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getAllConsentForms(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllConsentForms)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.consentForms = res.result.items;
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.consentForms);
              this.dataSource.paginator = this.paginator;
              this.sort.sort(({ id: 'title', start: 'asc'}) as MatSortable);
              this.dataSource.sort = this.sort;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ConsentFormsComponent getAllConsentForms " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ConsentFormsComponent getAllConsentForms " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ConsentFormsComponent getAllConsentForms " + e);
          }
        }
      );
  }

  updateConsentForm(action: string, consentFormId: string) {
    if (action == 'add') {
      this.router.navigate([
        this.routeConfig.updateConsentFormPath
      ]);
    } else if (action == 'edit') {
      this.router.navigate([
        this.routeConfig.updateConsentFormPath, consentFormId
      ]);
    }
  }

  deleteConsentForm(consentFormId: string) {
    const message = ValidationMessages.deleteConsentFormConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteConsentFormById + consentFormId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAllConsentForms();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception ConsentFormsComponent deleteConsentForm " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error ConsentFormsComponent deleteConsentForm " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception ConsentFormsComponent deleteConsentForm " + e);
              }
            });
      }
    });
  }

  homeClick(): void {
    if (this.userService.userRole) {
      const role = this.userService.userRole;
      if (role == "PATIENT") {
        this.router.navigate([this.routeConfig.patientDashboardPath]);
      } else if (role == "INSURANCE") {
        this.helperService.navigateInsuranceUser(this.user);
      } else if (role == "MEDICALLEGAL") {
        this.helperService.navigateMedicalLegalUser(this.user);
      } else if (role == "ADMIN") {
        this.router.navigate([this.routeConfig.adminDashboardPath]);
      } else if (role == "FAMILYDOCTOR") {
        this.router.navigate([this.routeConfig.familyDoctorDashboardPath]);
      } else if (role == "DIAGNOSTIC") {
        this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
      } else if (role == "CONSULTANT") {
        this.router.navigate([this.routeConfig.consultantDashboardPath]);
      }
    }
  }
}
