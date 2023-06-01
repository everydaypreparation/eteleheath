import { Component, OnInit, OnDestroy, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { UserService } from 'src/app/services/user.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { ApiService } from 'src/app/services/api.service';
import { HelperService } from 'src/app/services/helper.service';
import { RescheduleAppointmentModalComponent } from 'src/app/components/reschedule-appointment-modal/reschedule-appointment-modal.component';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { UploadDocumentModalComponent } from '../../upload-document-modal/upload-document-modal.component';
import { PropConfig } from 'src/app/configs/prop.config';
import { RequestDocModalComponent } from '../../request-doc-modal/request-doc-modal.component';
import { CancelAppointmentModalComponent } from '../../cancel-appointment-modal/cancel-appointment-modal.component';
import { DomSanitizer } from '@angular/platform-browser';
import { NotesModelComponent } from '../../notes-model/notes-model.component';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { ViewDocModalComponent } from '../../view-doc-modal/view-doc-modal.component';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit , OnDestroy{

  @ViewChild('patientRecordsUl', { read: ElementRef }) public patientRecordsUl: ElementRef<any>;
  @ViewChild('notesUl', { read: ElementRef }) public notesUl: ElementRef<any>;
  @ViewChild('messagesUl', { read: ElementRef }) public messagesUl: ElementRef<any>;
  @ViewChild('consultantReportUl', { read: ElementRef }) public consultantReportUl: ElementRef<any>;
  @ViewChild('missedAppointmentUl', { read: ElementRef }) public missedAppointmentUl: ElementRef<any>;


  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private dialog: MatDialog,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private helperService: HelperService,
    private changeDetectorRef: ChangeDetectorRef,
    private propConfig: PropConfig,
    private envAndUrlService: EnvAndUrlService,
    private sanitizer: DomSanitizer) { }

  userId: any = null;
  user: any;
  subscribeUser: any;
  loginUserId: any = null;
  upcomingAppointment: any;
  userEmails: any[] = [];
  notes: any[] = [];
  consultantReports: any[] = [];
  timezones: any[] = [];
  userTimezone: any = {};
  missedAppointments: any[] = [];
  selectedTab: string = "Messages";
  selectedConsultantReports: any[];
  // inboxBody = {
  //   "userId": ""
  // };

  patientRecords: any[];
  doctorProfileUrl: any;
  familyDoctorProfileUrl: any;

  apisPages: any = {
    patientRecords: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    notes: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    userMessages: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    consultantReport: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    },
    missedAppointment: {
      page: 1,
      limit: 10,
      count: 0,
      more: true,
    }
  }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    else {
      this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
        this.user = this.userService.getUser();
        this.loginUserId = this.user.id;
      });
    }

    this.getUserTimezoneOffset();
    this.getNextAppointment();
    this.getMissingAppointment(false);
    this.getAllUserEmailsByUserId(this.loginUserId, false);
    this.getDocumentListByUserId(false);
    this.getConsultantReports(false);
    //this.getSelectedConsultantReportsByPatientId();
  }

  getMissingAppointment(isConcat: boolean): void {
    let pageLimit = this.apisPages.missedAppointment.limit;
    let pages = (isConcat ? this.apisPages.missedAppointment.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getMissedAppointments + this.loginUserId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // console.log(res);
              //this.missedAppointments = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.missedAppointment);
                this.missedAppointmentUl.nativeElement.scrollTop = 0;
                this.missedAppointments = [];
              }
              this.missedAppointments = [...this.missedAppointments, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.missedAppointment);
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getMissingAppointment " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getMissingAppointment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getMissingAppointment " + e);
          }
        }
      );
  }

  getNextAppointment(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getPatientAppointmentDetails +this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.upcomingAppointment = res.result;
              this.getAllNotesByUserId(res.result.appointmentId, false);
              this.changeDetectorRef.detectChanges();
              if (this.upcomingAppointment.doctorProfileUrl) {
                if(this.upcomingAppointment.doctorProfileUrl.includes(this.apiConfig.matchesUrl)){
                  this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.upcomingAppointment.doctorProfileUrl}`);
                 }else{
                  this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.upcomingAppointment.doctorProfileUrl}`);
                 }
                //this.doctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.upcomingAppointment.doctorProfileUrl}`);
              }
              if (this.upcomingAppointment.familyDoctorProfileUrl) {
                if(this.upcomingAppointment.familyDoctorProfileUrl.includes(this.apiConfig.matchesUrl)){
                  this.familyDoctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.upcomingAppointment.familyDoctorProfileUrl}`);
                 }else{
                  this.familyDoctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.upcomingAppointment.familyDoctorProfileUrl}`);
                 }
                //this.familyDoctorProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.upcomingAppointment.familyDoctorProfileUrl}`);
              }
              // this.upcomingAppointment = res.result.items[0];
              // this.getAllNotesByUserId(res.result.items[0].appointmentId);
              
            }else {
              this.upcomingAppointment = [];
              this.getUser();
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getNextAppointment " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getNextAppointment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getNextAppointment " + e);
          }
        }
      );
  }

  cancelClick(appointmentId: any) {

    const message = ValidationMessages.confirmCancelAppointment;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(CancelAppointmentModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult && dialogResult[0] == true) {
        this.cancelAppointment(appointmentId, dialogResult[1]);
      }
    });
  }

  cancelAppointment(appointmentId: any, reason: any) {
    const body = {
      "Id": appointmentId,
      "UserId": this.loginUserId,
      "Reason": reason
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.cancelAppointment, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.getNextAppointment();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent cancelClick " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent cancelClick " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent cancelClick " + e);
          }
        }
      );
  }

  rescheduleClick(appointmentId: any, doctorId: any) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: {"appointmentId": appointmentId, "doctorId": doctorId}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getNextAppointment();
        this.getUser();
        this.getMissingAppointment(false);
      }
    });
  }
  
  getDocumentListByUserId(isConcat: boolean): void {
    let pageLimit = this.apisPages.patientRecords.limit;
    let pages = (isConcat ? this.apisPages.patientRecords.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDocumentListByUserId +this.loginUserId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.patientRecords = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.patientRecords);
                this.patientRecordsUl.nativeElement.scrollTop = 0;
                this.patientRecords = [];
              }
              for (let record of res.result.items) {
                // let documentDate = record.documentDate.split(" ");
                // record.date = documentDate[0];
                // record.month = documentDate[1].substr(0, 3);
                record.documentName = record.documentName.split(".")[0];
                if (record.documentExtension == ".pdf") {
                  record.documentExtension = "assets/images/ico-file-2.svg";
                } else if (record.documentExtension == ".doc" || record.documentExtension == ".docx") {
                  record.documentExtension = "assets/images/ico-file-1.svg";
                } else if (record.documentExtension == ".xlsx" || record.documentExtension == ".xls") {
                  record.documentExtension = "assets/images/ico-file-4.svg";
                } else {
                  record.documentExtension = "assets/images/ico-file.svg";
                }
              }
              this.patientRecords = [...this.patientRecords, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.patientRecords);              
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getDocumentListByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getDocumentListByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getDocumentListByUserId " + e);
          }
        }
      );
  }

  getSelectedConsultantReportsByPatientId(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getIntakeConsultantReportsByPatientId +this.loginUserId+"&AppointmentId="+this.envAndUrlService.UUID+"&DoctorId="+this.envAndUrlService.UUID)
      .subscribe(
        (res: any) => {
          console.log('--RRR---');
          console.log(res.result.items);
          try {
            if (res.result.statusCode == 200) {
              this.selectedConsultantReports = res.result.items;
              for(let record of this.selectedConsultantReports) {
                let documentDate = record.documentDate.split(" ");
                record.date = documentDate[0];
                record.month = documentDate[1].substr(0, 3);
                record.documentName = record.documentName.split(".")[0];
                record.documentExtension = "assets/images/report-2.png";
              }
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getSelectedConsultantReportsByPatientId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getSelectedConsultantReportsByPatientId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getSelectedConsultantReportsByPatientId " + e);
          }
        }
      );
  }

  patientRecordUploadClick() {
    const dialogRef = this.dialog.open(UploadDocumentModalComponent, {
      data: {"loginUserId": this.loginUserId}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getDocumentListByUserId(false);
      }
    });
  }

  editPatientRecordClick(documentId) {
    const dialogRef = this.dialog.open(UploadDocumentModalComponent, {
      data: {"loginUserId": this.loginUserId, "documentId": documentId}
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getDocumentListByUserId(false);
      }
    });
  }

  deletePatientRecordClick(documentId) {

    const message = ValidationMessages.confirmDocumentDelete;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.deletePatientRecord(documentId);
      }
    });
  }

  deletePatientRecord(documentId) {
    this.stickyBarService.showLoader("");
    this.apiService
      .deleteWithBearer(this.apiConfig.deleteDocumentById + documentId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.getDocumentListByUserId(false);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent deletePatientRecord " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent deletePatientRecord " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent deletePatientRecord " + e);
          }
        }
      );
  }

  downloadDocument(documentId) {
    this.helperService.downloadDocumentById(documentId);
  }

  getAllUserEmailsByUserId(loginUserId: any, isConcat: boolean): void {
    // this.inboxBody.userId = loginUserId;
    const inboxBody = {
      "userId": loginUserId,
      "limit": this.apisPages.userMessages.limit,
      "page": isConcat ? this.apisPages.userMessages.page : 1
    };
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getAllUserEmailsByUserId, inboxBody)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.userEmails = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.userMessages);
                this.messagesUl.nativeElement.scrollTop = 0;
                this.userEmails = [];
              }
              this.userEmails = [...this.userEmails, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.userMessages);
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAllUserEmailsByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAllUserEmailsByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAllUserEmailsByUserId " + e);
          }
        }
      );
  }

  setReplyCurrentMailId(replyMailId: any, replySubject: any) {
    if (replyMailId) {
      sessionStorage.setItem('replyMailId', replyMailId);
    } else {
      sessionStorage.setItem('replyMailId', '');
    }
    if (replySubject) {
      sessionStorage.setItem('replySubject', replySubject);
    } else {
      sessionStorage.setItem('replySubject', '');
    }
    sessionStorage.setItem("appointmentId", this.upcomingAppointment ? this.upcomingAppointment.appointmentId : "");
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  composeMessage() {
    sessionStorage.setItem("appointmentId", "");
    this.router.navigate([this.routeConfig.messagingComposePath]);
  }

  getAllNotesByUserId(appintmentId: any,isConcat: boolean): void {
    let pageLimit = this.apisPages.notes.limit;
    let pages = (isConcat ? this.apisPages.notes.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllNotesByUserId + this.loginUserId + "&AppointmentId=" + this.envAndUrlService.UUID+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.notes = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.notes);
                this.notesUl.nativeElement.scrollTop = 0;
                this.notes = [];
              }
              this.notes = [...this.notes, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.notes);
            }
          } catch (e) {
            console.log("Success Exception DashboardComponent getAllNotesByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error DashboardComponent getAllNotesByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception DashboardComponent getAllNotesByUserId " + e);
          }
        }
      );
  }

  addNote() {
    // this.router.navigate([this.routeConfig.notesPath]);

    const dialogRef = this.dialog.open(NotesModelComponent, {
      data: { "loginUserId": this.loginUserId, "noteId": "", "note": "", "appointmentId": this.envAndUrlService.UUID }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotesByUserId(this.upcomingAppointment.appointmentId, false);
      }
    });

  }

  onTabChange(event: MatTabChangeEvent) {
    this.selectedTab = event.tab.textLabel
  }
  
  getConsultantReports(isConcat: boolean): void {
    let pageLimit = this.apisPages.consultantReport.limit;
    let pages = (isConcat ? this.apisPages.consultantReport.page : 1);
    // let body = {
    //   "UserId": this.loginUserId,
    //   "DoctorId": "",
    //   "PatientId": "",
    //   "RoleName": this.user.roleNames[0],
    //   "limit":this.apisPages.consultantReport.limit,
    //   "page":(isConcat ? this.apisPages.consultantReport.page : 1)
    // }
    this.stickyBarService.showLoader("");
    this.apiService
    .getWithBearer(this.apiConfig.getConsultantReportForAllRole+this.loginUserId+"&DoctorId="+this.envAndUrlService.UUID+"&PatientId="+this.envAndUrlService.UUID+"&RoleName="+this.user.roleNames[0]+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.consultantReports = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.consultantReport);
                this.consultantReportUl.nativeElement.scrollTop = 0;
                this.consultantReports = [];
              }
              this.consultantReports = [...this.consultantReports, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.consultantReport);
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getConsultantReports " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getConsultantReports " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getConsultantReports " + e);
          }
        }
      );
  }

  downloadReport(consultId) {
    this.helperService.downloadReportByConsultId(consultId);
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
              let userTimezone = this.timezones.filter(t => t.timeZoneId==this.user.timezone)[0];
              if(!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId==this.propConfig.defaultTimezoneId)[0];
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

  formatTime24HourTo12Hour(time: any) {
    return this.helperService.formatTime24HourTo12Hour(time);
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  ngOnDestroy(){
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
  }

  gotoSamvaad(appointmentId, meetingID, Title, patientId, roleName) {
    if (Title === null || Title === undefined || Title === '') {
      Title = "Video Conference";
    }

    if (appointmentId === null || appointmentId === undefined || appointmentId === '') {
      appointmentId = this.envAndUrlService.UUID;
    }

    if (patientId === null || patientId === undefined || patientId === '') {
      patientId = this.envAndUrlService.UUID;
    }

    // this.router.navigate([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    const url = this.router.createUrlTree([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    window.open(window.location.origin + "/#" + url.toString(), '_blank');
  }

  requestDoc(): void {
    const dialogRef = this.dialog.open(RequestDocModalComponent, {
      data: {userId: this.loginUserId , loginUserId: this.loginUserId, availabilitySlotId: null},
      autoFocus: false
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });
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

patientRecordOpenDocClick(documentId: any) {
  const dialogRef = this.dialog.open(ViewDocModalComponent, {
    width: "100%",
    height: "100%",
    data: {"documentId": documentId}
  });

  dialogRef.afterClosed().subscribe(dialogResult => {
    if (dialogResult == true) {
    }
  });
}

findConsultant(){
  this.router.navigate([this.routeConfig.findingConsultantPath]);
}

getUser(): void {
  const userId = localStorage.getItem("userId");
  this.stickyBarService.showLoader("");
  this.apiService
    .getWithBearer(this.apiConfig.getUser + userId)
    .subscribe(
      (res: any) => {
        try {
          this.stickyBarService.hideLoader("");
          if (res.result.statusCode == 200) {
            this.userService.setUser(res.result);
            this.user = this.userService.getUser();
            if(!this.user.isPayment && !this.user.isIntake && !this.user.isAppointment && this.user.isAllowtoNewBooking && !this.user.isMissedAppointment){
              this.router.navigate([this.routeConfig.findingConsultantPath]);
            }
          }
        } catch (e) {
          console.log("Success Exception DashboardComponent getUser " + e);
        }
      },
      (err: any) => {
        try {
          this.stickyBarService.hideLoader("");
          console.log("Error DashboardComponent getUser " + JSON.stringify(err));
          this.apiService.catchError(err);
        } catch (e) {
          console.log("Error Exception DashboardComponent getUser " + e);
        }
      }
    );
}

resetApisPage(pageObj: any) {
  pageObj.page = 1;
  pageObj.count = 0;
  pageObj.more = true;
}

paginationHandler(result: any, pageObj: any): void {
  if (!result.items) {
    pageObj.more = false;
  }
  pageObj.count += result.items.length;
  if (result.count == 0 || (result.count == pageObj.count)) {
    pageObj.more = false;
  }
}

scrollHandler(e: any, type: string): void {
  if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight) {
    if (type == "patientRecords" && this.apisPages.patientRecords.more) {
      this.apisPages.patientRecords.page += 1;
      this.getDocumentListByUserId(true);
    }else if (type == "notes" && this.apisPages.notes.more) {
      this.apisPages.notes.page += 1;
      this.getAllNotesByUserId(this.upcomingAppointment.appointmentId, true);
    }else if (type == "messages" && this.apisPages.userMessages.more) {
      this.apisPages.userMessages.page += 1;
      this.getAllUserEmailsByUserId(this.loginUserId, true);
    }else if (type == "consultantReport" && this.apisPages.consultantReport.more) {
      this.apisPages.consultantReport.page += 1;
      this.getConsultantReports(true);
    }else if (type == "missed" && this.apisPages.missedAppointment.more) {
      this.apisPages.missedAppointment.page += 1;
      this.getMissingAppointment(true);
    }
  }
}

}
