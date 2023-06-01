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
import { HelperService } from 'src/app/services/helper.service';

interface Food {
  value: string;
  viewValue: string;
}
@Component({
  selector: 'app-emro-admins',
  templateUrl: './emro-admins.component.html',
  styleUrls: ['./emro-admins.component.scss']
})
export class EmroAdminsComponent implements OnInit {
  users: any[] = [];
  roleName: string = 'admin';
  navigationSubscription;
  user: any;

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
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private changeDetectorRef: ChangeDetectorRef,
    private helperService: HelperService) {
    this.userService.setNav("emroadmins");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
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
                this.dataSource.paginator = this.paginator;
                this.dataSource.sortingDataAccessor = (item, sortHeaderId) => {
                  switch(sortHeaderId) {
                    case 'fullName': return (item.name + " " + item.surname).toLocaleLowerCase();
                    default:  if (typeof item[sortHeaderId] === 'string') {
                                return item[sortHeaderId].toLocaleLowerCase();
                              }
                              return item[sortHeaderId];
                  }
                };
                this.sort.sort(({ id: 'fullName', start: 'asc'}) as MatSortable);
                this.dataSource.sort = this.sort;
              } else {
                this.dataSource.data = this.users;
              }
              this.changeDetectorRef.detectChanges();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AdminEmroComponent getUserListByRoleName " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AdminEmroComponent getUserListByRoleName " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AdminEmroComponent getUserListByRoleName " + e);
          }
        }
      );
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

}
