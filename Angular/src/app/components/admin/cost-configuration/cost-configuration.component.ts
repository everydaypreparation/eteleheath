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
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { HelperService } from 'src/app/services/helper.service';
import { AddCostModalComponent } from '../../add-cost-modal/add-cost-modal.component';

@Component({
  selector: 'app-cost-configuration',
  templateUrl: './cost-configuration.component.html',
  styleUrls: ['./cost-configuration.component.scss']
})
export class CostConfigurationComponent implements OnInit {

  costs: any[] = [];
  user: any;
  displayedColumns: string[] = ['id' , 'keyName', 'value', 'action'];
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
      this.userService.setNav("costConfiguration");
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      //this.loginUserId = this.user.id;
      this.getAllCost();
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getAllCost(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllCost)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.costs = res.result.items;
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.costs);
              this.dataSource.paginator = this.paginator;
              this.dataSource.sort = this.sort;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception CostConfigurationComponent getAllCost " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error CostConfigurationComponent getAllCost " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception CostConfigurationComponent getAllCost " + e);
          }
        }
      );
  }

  addCost() {
    const dialogRef = this.dialog.open(AddCostModalComponent, {
      data: { "costId": ""}
    });
    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllCost();
      }
    });
  }

  updateCost(costId: any) {
    const dialogRef = this.dialog.open(AddCostModalComponent, {
      data: { "costId": costId}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllCost();
      }
    });
  }


  deleteCost(costId: string) {
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
          .deleteWithBearer(this.apiConfig.deleteCost + costId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAllCost();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception CostConfigurationComponent deleteCost " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error CostConfigurationComponent deleteCost " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception CostConfigurationComponent deleteCost " + e);
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
