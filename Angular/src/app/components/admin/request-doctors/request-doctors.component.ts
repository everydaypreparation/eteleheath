import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ViewRequestDoctorModelComponent } from '../../view-request-doctor-model/view-request-doctor-model.component';
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-request-doctors',
  templateUrl: './request-doctors.component.html',
  styleUrls: ['./request-doctors.component.scss']
})
export class RequestDoctorsComponent implements OnInit {

  requestDoctors: any[] = [];
  navigationSubscription;
  loginUserId: any;
  timezones: any[] = [];
  userTimezone: any = {};
  user: any;
  onBoardPatient: any = 'progress';

  displayedColumns: string[] = ['id', 'doctorFirstName', 'hospital', 'firstName', 'roleName', 'createdOn', 'patientOnBoard', 'action'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  onBoardUserBody = {
    "id": "",
    "isCompleted": true
  };

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private dialog: MatDialog, private userService: UserService, private propConfig: PropConfig) {
    this.userService.setNav("requestDoctor");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.getUserTimezoneOffset();
    this.getRequestDoctorsList();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getRequestDoctorsList(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getRequestDoctors)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.requestDoctors = res.result.items;
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.requestDoctors);
              this.dataSource.paginator = this.paginator;
              this.dataSource.sort = this.sort;
            }
          } catch (e) {
            console.log("Success Exception RequestDoctorsComponent getRequestDoctorsList " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RequestDoctorsComponent getRequestDoctorsList " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RequestDoctorsComponent getRequestDoctorsList " + e);
          }
        }
      );
  }


  deleteRequestDoctorsDetail(requestId: string) {
    const message = ValidationMessages.deleteRequestDoctorConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteRequestDoctorDetail + requestId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getRequestDoctorsList();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception RequestDoctorsComponent deleteRequestDoctorDetail " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error RequestDoctorsComponent deleteUserDetail " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception RequestDoctorsComponent deleteUserDetail " + e);
              }
            });
      }
    });
  }

  composeMessage(emailAddress: any, requestId: any, status: any) {
    sessionStorage.setItem('replyMailId', emailAddress);
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay', requestId, status]);
  }


  viewRequestDoctorsDetail(requestId: string) {
    const dialogRef = this.dialog.open(ViewRequestDoctorModelComponent, {
      data: { "loginUserId": this.loginUserId, "requestId": requestId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
      }
    });

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
            console.log("Success Exception RequestDoctorsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error RequestDoctorsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception RequestDoctorsComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  onBoardUserStatus(requestId: any, status: any) {
    let message;
    if (status == 'Completed') {
      this.onBoardUserBody.isCompleted = false;
      message = ValidationMessages.deOnboardUserConfirmation;
    } else {
      this.onBoardUserBody.isCompleted = true;
      message = ValidationMessages.onboardUserConfirmation;
    }

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
          this.stickyBarService.showLoader("");
          this.onBoardUserBody.id = requestId;
          this.apiService
            .putWithBearer(this.apiConfig.updateUserOnboardRequest, this.onBoardUserBody)
            .subscribe(
              (res: any) => {
                try {
                  if (res.result.statusCode == 200) {
                    this.getRequestDoctorsList();
                  }
                } catch (e) {
                  console.log("Success Exception RequestDoctorsComponent onBoardUserStatus " + e);
                }
                this.stickyBarService.hideLoader("");
              },
              (err: any) => {
                try {
                  console.log("Error RequestDoctorsComponent onBoardUserStatus " + JSON.stringify(err));
                  this.apiService.catchError(err);
                } catch (e) {
                  console.log("Error Exception RequestDoctorsComponent onBoardUserStatus " + e);
                }
                this.stickyBarService.hideLoader("");
              }
            );
      }
    });
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  homeClick(): void {
    this.router.navigate([this.routeConfig.adminDashboardPath]);
  }
}
