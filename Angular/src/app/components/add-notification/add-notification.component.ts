import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';
import { AddNotificationModalComponent } from '../add-notification-modal/add-notification-modal.component';

@Component({
  selector: 'app-add-notification',
  templateUrl: './add-notification.component.html',
  styleUrls: ['./add-notification.component.scss']
})
export class AddNotificationComponent implements OnInit {

  notifications: any[] = [];
  user: any;
  displayedColumns: string[] = ['id' , 'title', 'role', 'description', 'action'];
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
    private activatedRoute: ActivatedRoute,
    private helperService: HelperService) {
      this.userService.setNav("addNotification");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.getAllNotification();
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getAllNotification(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllNotification)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.notifications = res.result.items;
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.notifications);
              this.dataSource.paginator = this.paginator;
              this.dataSource.sort = this.sort;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddNotificationComponent getAllNotification " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddNotificationComponent getAllNotification " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddNotificationComponent getAllNotification " + e);
          }
        }
      );
  }

  addNotification() {
    const dialogRef = this.dialog.open(AddNotificationModalComponent, {
      data: { "notificationId": "", "userId": this.user.id}
    });
    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotification();
      }
    });
  }

  updateNotification(notificationId: any) {
    const dialogRef = this.dialog.open(AddNotificationModalComponent, {
      data: { "notificationId": notificationId, "userId": this.user.id}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotification();
      }
    });
  }


  deleteNotification(notificationId: string) {
    const message = ValidationMessages.deleteNotificationConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteNotification + notificationId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAllNotification();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception AddNotificationComponent deleteNotification " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error AddNotificationComponent deleteNotification " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception AddNotificationComponent deleteNotification " + e);
              }
            });
      }
    });
  }

  homeClick(): void {
    this.router.navigate([this.routeConfig.adminDashboardPath]);
  }
}