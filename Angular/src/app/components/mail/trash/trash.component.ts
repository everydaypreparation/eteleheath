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
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { PropConfig } from 'src/app/configs/prop.config';
import { HelperService } from 'src/app/services/helper.service';
declare const $: any;

@Component({
  selector: 'app-trash',
  templateUrl: './trash.component.html',
  styleUrls: ['./trash.component.scss']
})
export class TrashComponent implements OnInit {

  user: any;
  userEmails: any[] = [];
  loginUserId: string = null;
  timezones: any[] = [];
  userTimezone: any = {};

  selectedMailIdsArray: Array<any> = [];
  isCheckedSelectAll: boolean;
  mailIdsArray: Array<any> = [];


  displayedColumns: string[] = ['id', 'firstName', 'subject', 'mailDateTime'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private changeDetectorRef: ChangeDetectorRef,
    private dialog: MatDialog,
    private propConfig: PropConfig,
    private helperService: HelperService) {
    this.userService.setNav("messaging");
  }

  inboxBody = {
    "userId": ""
  };

  restoreBody = {
    "userId": "",
    "messageId": ""
  };

  ngOnInit(): void {
    $('.trashButton').addClass("selectedLeftBar");
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
      this.getAllUserEmailsByUserId(this.loginUserId);
    }
    this.getUserTimezoneOffset();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getAllUserEmailsByUserId(loginUserId: any): void {
    $('.selectAll').prop('checked', false);
    this.mailIdsArray = [];
    this.userEmails = [];
    this.selectedMailIdsArray = [];
    this.inboxBody.userId = loginUserId;
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getAllTrashEmailsByUserId, this.inboxBody)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userEmails = res.result.items;
              if (this.userEmails) {
                this.mailIdsArray = this.userEmails.filter(ue => ue != null).map(ue => ue.messageId);
              } else {
                this.mailIdsArray = [];
              }
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.userEmails);
              this.dataSource.paginator = this.paginator;
              this.dataSource.sort = this.sort;
              $('.checkAll').prop('checked', false);
            } else {
              this.userEmails = [];
              this.mailIdsArray = [];
              this.selectedMailIdsArray = [];
              this.dataSource = new MatTableDataSource(this.userEmails);
              $('.checkAll').prop('checked', false);
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception sentComponent getAllUserEmailsByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error sentComponent getAllUserEmailsByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception sentComponent getAllUserEmailsByUserId " + e);
          }
        }
      );
  }

  composeMessage() {
    this.router.navigate([this.routeConfig.messagingComposePath]);
  }
  inboxMessage() {
    this.router.navigate([this.routeConfig.messagingInboxPath]);
  }
  sentMessage() {
    this.router.navigate([this.routeConfig.messagingSentPath]);
  }
  trashMessage() {
    this.router.navigate([this.routeConfig.messagingTrashPath]);
  }

  viewMail(id: any) {
    this.router.navigate([this.routeConfig.messagingViewMailPath, 'trash', id]);
  }

  getUserMessage(msg: string) {
    let message;
    if (msg) {
      message = msg.replace(/<(.|\n)*?>/g, '').trim();
    }
    return (message && message.length > 30 ? "- " + message.slice(0, 30) + '...' : "- " + message);
  }

  onInboxEventChange(emailId: string, isChecked: boolean) {
    if (isChecked && emailId == 'page') {
      this.selectedMailIdsArray = this.userEmails.filter(ue => ue != null).map(ue => ue.messageId);
      this.isCheckedSelectAll = true;
      $('.selectAll').prop('checked', true);
    } else if (!isChecked && emailId == 'page') {
      this.selectedMailIdsArray = [];
      this.isCheckedSelectAll = false;
      $('.selectAll').prop('checked', false);
    }

    if (emailId != 'page') {
      this.isCheckedSelectAll = (this.selectedMailIdsArray.length > 0 && this.mailIdsArray.length > 0 && this.mailIdsArray.every(i => this.selectedMailIdsArray.includes(i)));
      $('.selectAll').prop('checked', this.isCheckedSelectAll);
      if (isChecked) {
        this.selectedMailIdsArray.push(emailId);
      } else {
        let index = this.selectedMailIdsArray.indexOf(emailId);
        this.selectedMailIdsArray.splice(index, 1);
        $('.selectAll').prop('checked', false);
      }
    }
    let arr = this.userEmails.filter(ue => ue != null).map(ue => ue.messageId);
    if (arr.length == this.selectedMailIdsArray.length) {
      $('.selectAll').prop('checked', true);
    }
  }

  deleteMailsByMailId() {
    // Close the dialog, return true
    const message = ValidationMessages.deleteMessagesConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService.deleteWithBearer(this.apiConfig.deleteMails + this.selectedMailIdsArray.toString() + '&IsDeleteReceiver=false&IsDeleteSender=false&IsDeleteTrash=true&IsDeleteDraft=false&UserId=' + this.loginUserId).subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
                this.getAllUserEmailsByUserId(this.loginUserId);
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception InboxComponent deleteMailsByMailId " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error InboxComponent deleteMailsByMailId " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception InboxComponent deleteMailsByMailId " + e);
            }
          }
        );
      }
    });
  }

  restoreTrashMailByMailId() {
    this.restoreBody.userId = this.loginUserId;
    this.restoreBody.messageId = this.selectedMailIdsArray.toString();
    this.apiService.postWithBearer(this.apiConfig.restore, this.restoreBody)
      .subscribe(
        (res: any) => {
          try {
            this.getAllUserEmailsByUserId(this.loginUserId);
          } catch (e) {
            console.log("Success Exception HelperService restoreTrashMailByMailId " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ViewMailComponent restoreTrashMailByMailId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewMailComponent restoreTrashMailByMailId " + e);
          }
        }
      );
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
            console.log("Success Exception DashboardComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
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
        let usr = this.userService.getUser();
        if (!usr.isCase) {
          this.router.navigate([this.routeConfig.familyDoctorsEmptyDashboardPath]);
        } else {
          this.router.navigate([this.routeConfig.familyDoctorDashboardPath]);
        }
      } else if (role == "DIAGNOSTIC") {
        if (this.userService.getUser().isAppointment == false) {
          this.router.navigate([
            this.routeConfig.diagnosticDashboardPath
          ]);
        } else if (this.userService.getUser().isAppointment == true) {
          this.router.navigate([
            this.routeConfig.diagnosticDashboardDetailsPath
          ]);
        }
      } else if (role == "CONSULTANT") {
        this.router.navigate([this.routeConfig.consultantDashboardPath]);
      }
    }
  }

}
