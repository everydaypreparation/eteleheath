import { Component, OnInit, Input, ChangeDetectorRef, ViewChild } from '@angular/core';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { UserService } from 'src/app/services/user.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { MatDialog } from '@angular/material/dialog';
import { ViewSurveyFeedbackModelComponent } from '../../view-survey-feedback-model/view-survey-feedback-model.component';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-survey-feedbacks',
  templateUrl: './survey-feedbacks.component.html',
  styleUrls: ['./survey-feedbacks.component.scss']
})
export class SurveyFeedbacksComponent implements OnInit {

  displayedColumns: string[] = ['id', 'userName', 'appointmentId', 'appointmentDateTime', 'formSubmissionDate', 'action'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  feedbacks: any = [];
  user: any;
  timezones: any[] = [];
  userTimezone: any = {};

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private propConfig: PropConfig,
    private dialog: MatDialog,) {
    this.userService.setNav("surveyfeedbacks");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.getUserTimezoneOffset();
    this.getSurveyFeedbacks();
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
            console.log("Success Exception SurveyFeedbacksComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SurveyFeedbacksComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SurveyFeedbacksComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  getSurveyFeedbacks(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getSurveys)
      .subscribe(
        (res: any) => {
          try {
            this.feedbacks = [];
            if (res.result.statusCode == 200) {
              this.feedbacks = res.result.items;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
            this.prepareTable();
          } catch (e) {
            console.log("Success Exception SurveyFeedbacksComponent getSurveyFeedbacks " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SurveyFeedbacksComponent getSurveyFeedbacks " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SurveyFeedbacksComponent getSurveyFeedbacks " + e);
          }
        }
      );
  }

  prepareTable(): void {
    this.dataSource = new MatTableDataSource(this.feedbacks);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.changeDetectorRef.detectChanges();
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  homeClick(): void {
    this.router.navigate([this.routeConfig.adminDashboardPath]);
  }

  viewFeedback(feedback: any): void {
    const dialogRef = this.dialog.open(ViewSurveyFeedbackModelComponent, {
      maxWidth: "75%",
      height: "100vh",
      autoFocus: false,
      data: feedback
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });
  }

  deleteFeedback(feedback: any): void {
    const message = ValidationMessages.deleteFeedbackConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteSurvey + feedback.userId + "&AppointmentId=" + feedback.appointmentId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getSurveyFeedbacks();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception SurveyFeedbacksComponent deleteFeedback " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error SurveyFeedbacksComponent deleteFeedback " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception SurveyFeedbacksComponent deleteFeedback " + e);
              }
            });
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}
