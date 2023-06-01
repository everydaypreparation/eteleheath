import { Component, OnInit, Input, ChangeDetectorRef, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { MatDialog } from '@angular/material/dialog';
import { AppComponent } from 'src/app/app.component';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { v4 as uuid } from 'uuid';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

interface Food {
  value: string;
  viewValue: string;
}

@Component({
  selector: 'app-medical-legals',
  templateUrl: './medical-legals.component.html',
  styleUrls: ['./medical-legals.component.scss']
})
export class MedicalLegalsComponent implements OnInit {

  users: any[] = [];
  roleName: string = 'medicallegal';
  navigationSubscription;

  foods: Food[] = [
    { value: 'Option-1', viewValue: 'Option-1' },
    { value: 'Option-2', viewValue: 'Option-1' },
    { value: 'Option-3', viewValue: 'Option-1' }
  ];

  displayedColumns: string[] = ['id', 'fullName', 'cityId', 'stateId', 'countryId', 'phoneNumber', 'action'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private envAndUrlService: EnvAndUrlService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private dialog: MatDialog,) {
    this.userService.setNav("medicallegals");
  }

  ngOnInit(): void {
    this.getUserListByRoleName(false);
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getUserListByRoleName(initialized: boolean): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUserByRoles + this.roleName)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.users = res.result.getUserDetailsListOutputs;
              if(!initialized) {
                this.dataSource = new MatTableDataSource(this.users);
                this.dataSource.sort = this.sort;
                this.dataSource.sortingDataAccessor = (item, sortHeaderId) => {
                  switch(sortHeaderId) {
                    case 'fullName': return (item.name + " " + item.surname).toLocaleLowerCase();
                    default:  if (typeof item[sortHeaderId] === 'string') {
                                return item[sortHeaderId].toLocaleLowerCase();
                              }
                              return item[sortHeaderId];
                  }
                };
                this.dataSource.paginator = this.paginator;
                this.sort.sort(({ id: 'fullName', start: 'asc'}) as MatSortable);
              } else {
                this.dataSource.data = this.users;
              }
              this.changeDetectorRef.detectChanges();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AdminMedicalComponent getUserListByRoleName " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AdminMedicalComponent getUserListByRoleName " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AdminMedicalComponent getUserListByRoleName " + e);
          }
        }
      );
  }

  updateUser(action: string, userId: string) {
    if (action == 'add') {
      this.router.navigate([
        this.routeConfig.medicalRegistrationPath
      ]);
    } else if (action == 'edit') {
      this.router.navigate([
        this.routeConfig.medicalRegistrationPath, userId
      ]);
    }
  }

  viewUserDetail(userId: string) {
    this.router.navigate([
      this.routeConfig.adminViewUserPath, userId, this.roleName
    ]);
  }

  deleteUserDetail(userId: string) {
    const message = ValidationMessages.deleteUserConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteUserDetailsById + userId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getUserListByRoleName(true);
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception MedicalLegalsComponent deleteUserDetail " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error MedicalLegalsComponent deleteUserDetail " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception MedicalLegalsComponent deleteUserDetail " + e);
              }
            });
      }
    });
  }

  impersonateUser(userId: string, id: string) {

    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.ImpersonateUser + id).subscribe(
      (res: any) => {
        try {
          this.stickyBarService.hideLoader("");
          if (res.result.statusCode == 200) {

            localStorage.setItem("app-tn", res.result.accessToken);
            localStorage.IsImpersonating = true;
            localStorage.setItem("userId", userId);
            this.router.navigate([this.routeConfig.signInPath]);

            this.stickyBarService.showSuccessSticky(res.result.message);
          } else {
            this.stickyBarService.showErrorSticky(res.result.message);
          }
        } catch (e) {
          console.log("Success Exception PatientsComponent impersonateUser " + e);
        }
      },
      (err: any) => {
        try {
          this.stickyBarService.hideLoader("");
          console.log("Error PatientsComponent impersonateUser " + JSON.stringify(err));
          this.apiService.catchError(err);
        } catch (e) {
          console.log("Error Exception PatientsComponent impersonateUser " + e);
        }
      });
  }

  homeClick(): void {
    this.router.navigate([this.routeConfig.adminDashboardPath]);
  }

  setEmailReply(replyMailId: any) {
    if (replyMailId) {
      sessionStorage.setItem('replyMailId', replyMailId);
      sessionStorage.setItem('replySubject', '');
    } else {
      sessionStorage.setItem('replyMailId', '');
      sessionStorage.setItem('replySubject', '');
    }
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  gotoSamvaad(emailId: string, fullName: string) {

    const selectedEmailItems: Array<{ emailId: string, fullName: string }> = [
      { emailId: emailId, fullName: fullName },
    ];;

    if (selectedEmailItems.length > 0) {
      const appointmentId = this.envAndUrlService.UUID;
      const patientId = this.envAndUrlService.UUID;
      const meetingID = uuid();
      this.stickyBarService.showLoader("");

      // this.router.navigate([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
      const url = this.router.createUrlTree([this.routeConfig.samvaad + "/Video Conference/" + appointmentId + "/" + meetingID + "/" + patientId + "/Admin"]);

      this.apiService
        .postWithBearer(this.apiConfig.SendSamvaadEmail + url.toString(), selectedEmailItems)
        .subscribe(
          (res: any) => {
            try {
              this.stickyBarService.hideLoader("");
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
                window.open(window.location.origin + "/#" + url.toString(), '_blank');
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception DashboardComponent gotoSamvaad " + e);
            }
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error DashboardComponent gotoSamvaad " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception DashboardComponent gotoSamvaad " + e);
            }
          }
        );
    }
    else {
      this.stickyBarService.showErrorSticky("Please select at least one user for meeting.");
    }
  }
}
