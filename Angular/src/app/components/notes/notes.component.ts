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
import { ViewNoteModelComponent } from '../view-note-model/view-note-model.component';
import { NotesModelComponent } from '../notes-model/notes-model.component';
import { HelperService } from 'src/app/services/helper.service';
import { PropConfig } from 'src/app/configs/prop.config';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-notes',
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss']
})
export class NotesComponent implements OnInit {

  user: any;
  notes: any[] = [];
  loginUserId: string = null;
  appointmentId: any;
  timezones: any[] = [];
  userTimezone: any = {};

  displayedColumns: string[] = ['notes', 'noteDate', 'action'];
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
    private envAndUrlService: EnvAndUrlService,
    private helperService: HelperService, private propConfig: PropConfig) {
  }

  ngOnInit(): void {
    let appointmentId = this.activatedRoute.snapshot.params["appointmentId"];
    if(!appointmentId){
     this.appointmentId = this.envAndUrlService.UUID;
    }else{
      this.appointmentId = appointmentId;
    }
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
      this.getAllNotesByUserId();
      this.getUserTimezoneOffset();
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getAllNotesByUserId(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllNotesByUserId + this.loginUserId+'&AppointmentId='+this.envAndUrlService.UUID)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.notes = res.result.items;
              this.changeDetectorRef.detectChanges();
              this.dataSource = new MatTableDataSource(this.notes);
              this.dataSource.paginator = this.paginator;
              this.dataSource.sort = this.sort;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception NotesComponent getAllNotesByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error NotesComponent getAllNotesByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception NotesComponent getAllNotesByUserId " + e);
          }
        }
      );
  }

  addNotes() {
    const dialogRef = this.dialog.open(NotesModelComponent, {
      data: { "loginUserId": this.loginUserId, "noteId": "", "note": "" ,"appointmentId":this.envAndUrlService.UUID}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotesByUserId();
      }
    });
  }

  updateNote(noteId: any, note: any, appointmentId:any) {
    const dialogRef = this.dialog.open(NotesModelComponent, {
      data: { "loginUserId": this.loginUserId, "noteId": noteId, "note": note, "appointmentId": appointmentId ? appointmentId : this.envAndUrlService.UUID}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotesByUserId();
      }
    });
  }

  viewNote(noteId: string) {
    const dialogRef = this.dialog.open(ViewNoteModelComponent, {
      data: { "noteId": noteId }
    });
  }

  deleteNote(noteId: string) {
    const message = ValidationMessages.deleteNoteConfirmation;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteNoteById + noteId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getAllNotesByUserId();
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception NotesComponent deleteNote " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error NotesComponent deleteNote " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception NotesComponent deleteNote " + e);
              }
            });
      }
    });
  }

  getNotes(message: string) {
    return (message && message.length > 50 ? message.replace('&nbsp;', '').replace(/<(?:.|\n)*?>/gm, ' ').trim().slice(0, 50) + '...' : message.replace('&nbsp;', '').replace(/<(?:.|\n)*?>/gm, ' ').trim());
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
            console.log("Success Exception NotesComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error NotesComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception NotesComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }
}
