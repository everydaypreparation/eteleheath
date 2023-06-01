import { Component, OnInit, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-audit-report',
  templateUrl: './audit-report.component.html',
  styleUrls: ['./audit-report.component.scss']
})
export class AuditReportComponent implements OnInit {

  subscribeUser: any;
  user: any = null;

  searchInput = "";
  itemCount: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;
  pageSizeOptions: number[] = [5, 10, 25, 100];

  displayedColumns: string[] = ['id', 'userName', 'component', 'operation', 'action', 'executionTime', 'impersonatorUserName'];
  dataSource: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(
    private router: Router,
    private routeConfig: RouteConfig,
    private helperService: HelperService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private userService: UserService
  ) {
    this.user = this.userService.getUser();
    this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
      this.user = this.userService.getUser();
    });
  }

  ngOnInit(): void {
    this.getAuditReports(this.pageSize, this.pageNumber);
  }

  nextPage(event: PageEvent) {

    if (this.pageNumber != event.pageIndex + 1) {
      this.pageNumber = event.pageIndex + 1;
    }
    if (this.pageSize != event.pageSize) {
      this.pageSize = event.pageSize;
    }

    this.getAuditReports(this.pageSize, this.pageNumber);
  }

  getAuditReports(limit: number = 0, page: number = 0, searchKeyword: string = "") {

    //To make pagination work
    if (this.searchInput != "") {
      searchKeyword = this.searchInput
    }

    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAuditReports + "?limit=" + limit + "&page=" + page + "&searchKeyword=" + searchKeyword)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.dataSource = new MatTableDataSource(res.result.items);        
              //this.dataSource.paginator = this.paginator;
              this.itemCount = res.result.count;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AuditReportComponent getAuditReports " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AuditReportComponent getAuditReports " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AuditReportComponent getAuditReports " + e);
          }
        }
      );
  }

  applyFilter(event: KeyboardEvent) {

    this.searchInput = (event.target as HTMLInputElement).value;
    if (event.key.toLowerCase() == "enter" || this.searchInput == "") {
      this.getAuditReports(this.pageSize, this.pageNumber, this.searchInput);
      this.paginator.pageIndex = 0;
    }
  }

  applyFilterButton(searchKeyword) {

    this.searchInput = searchKeyword;
    this.getAuditReports(this.pageSize, this.pageNumber, searchKeyword);
    this.paginator.pageIndex = 0;
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
        let usr =  this.userService.getUser();
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
