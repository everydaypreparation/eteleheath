import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { RouteConfig } from 'src/app/configs/route.config';
import { Router, ActivatedRoute } from '@angular/router';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { UserService } from 'src/app/services/user.service';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from 'src/app/services/helper.service';
import { UploadDocumentModalComponent } from '../../upload-document-modal/upload-document-modal.component';
import { RescheduleAppointmentModalComponent } from '../../reschedule-appointment-modal/reschedule-appointment-modal.component';
import { ConfirmModalComponent, ConfirmModel } from '../../confirm-modal/confirm-modal.component';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { PropConfig } from 'src/app/configs/prop.config';
import { CancelAppointmentModalComponent } from '../../cancel-appointment-modal/cancel-appointment-modal.component';
import { DomSanitizer } from '@angular/platform-browser';

import { NotesModelComponent } from '../../notes-model/notes-model.component';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { ViewDocModalComponent } from '../../view-doc-modal/view-doc-modal.component';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-patient-details',
  templateUrl: './patient-details.component.html',
  styleUrls: ['./patient-details.component.scss']
})
export class PatientDetailsComponent implements OnInit {
  @ViewChild('patientRecordsUl', { read: ElementRef }) public patientRecordsUl: ElementRef<any>;
  @ViewChild('notesUl', { read: ElementRef }) public notesUl: ElementRef<any>;
  @ViewChild('messagesUl', { read: ElementRef }) public messagesUl: ElementRef<any>;
  @ViewChild('consultantReportUl', { read: ElementRef }) public consultantReportUl: ElementRef<any>;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private userService: UserService,
    private stickyBarService: StickyBarService,
    private apiConfig: ApiConfig,
    private apiService: ApiService,
    private activatedRoute: ActivatedRoute,
    private dialog: MatDialog,
    private helperService: HelperService,
    private propConfig: PropConfig,
    private envAndUrlService: EnvAndUrlService,
    private sanitizer: DomSanitizer) { }

  userId: any = null;
  user: any;
  subscribeUser: any;
  loginUserId: any = null;
  nextAppointment: any;
  appointmentId: any;
  patientId: any;
  patientRecords: any[];
  appointmentDetails: any;
  notes: any[] = [];
  consultantReports: any[] = [];
  timezones: any[] = [];
  userTimezone: any = {};

  selectedTab: string = "Messages";

  userEmails: any[] = [];
  consultantProfileUrl: any;
  profileUrl: any;

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
    }
  }

  ngOnInit(): void {
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
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
    this.getAppoinmentDetailsById();
    this.getAllNotesByUserId(false);
  
  }

  getAllUpcomingAppointment(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllUpcomingAppointmentById + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let upcomingAppointments = res.result.items;
              this.nextAppointment = upcomingAppointments.filter(a => a.appointmentId==this.appointmentId)[0];
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  rescheduleClick(appointmentId: any, doctorId: any) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": appointmentId, "doctorId": doctorId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getAllUpcomingAppointment();
      }
    });
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
              this.getAllUpcomingAppointment();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent cancelClick " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent cancelClick " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent cancelClick " + e);
          }
        }
      );
  }

  getAppoinmentDetailsById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAppoinmentDetailsById + this.appointmentId + "&UserId=" + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.appointmentDetails = res.result;
              console.log(this.appointmentDetails);
              this.patientId = this.appointmentDetails.patientId;
              this.getAllUserEmailsByUserId(this.loginUserId, this.appointmentDetails.patientId, false);
              if (this.appointmentDetails.dateOfBirth) {
                this.appointmentDetails.age = this.helperService.getAge(this.appointmentDetails.dateOfBirth);
              }
              if (this.appointmentDetails.doctorProfileUrl) {
                if(this.appointmentDetails.doctorProfileUrl.includes(this.apiConfig.matchesUrl)){
                  this.consultantProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.appointmentDetails.doctorProfileUrl}`);
                 }else{
                  this.consultantProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.appointmentDetails.doctorProfileUrl}`);
                 }
                //this.consultantProfileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.appointmentDetails.doctorProfileUrl}`);
              }
              if (this.appointmentDetails.profileUrl) {
                if(this.appointmentDetails.profileUrl.includes(this.apiConfig.matchesUrl)){
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${this.appointmentDetails.profileUrl}`);
                 }else{
                  this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.appointmentDetails.profileUrl}`);
                 }
                //this.profileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, ${this.appointmentDetails.profileUrl}`);
              }

              this.getDocumentListByAppointmentId(false);
              this.getConsultantReports(this.appointmentDetails.doctorId, false);

            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  getDocumentListByAppointmentId(isConcat: boolean): void {
    let pageLimit = this.apisPages.patientRecords.limit; 
    let pages = (isConcat ? this.apisPages.patientRecords.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getDocumentListByUserId + this.patientId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              // this.patientRecords = res.result.items;
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
            console.log("Success Exception PatientDetailsComponent getDocumentListByAppointmentId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getDocumentListByAppointmentId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getDocumentListByAppointmentId " + e);
          }
        }
      );
  }

  patientRecordUploadClick() {
    const dialogRef = this.dialog.open(UploadDocumentModalComponent, {
      data: { "loginUserId": this.patientId, "appointmentId": this.appointmentId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.getDocumentListByAppointmentId(false);
      }
    });
  }

  downloadDocument(documentId) {
    this.helperService.downloadDocumentById(documentId);
  }

  composeMessage() {
    if (this.patientId) {
      sessionStorage.setItem('replyMailId', this.patientId);
      sessionStorage.setItem('replySubject', '');
    }
    sessionStorage.setItem("appointmentId", this.appointmentId);
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  getAllUserEmailsByUserId(loginUserId: any, patientId: any, isConcat: boolean): void {
    let pageLimit = this.apisPages.userMessages.limit; 
    let pages = (isConcat ? this.apisPages.userMessages.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.getUserMessages + loginUserId + '&ToUserId=' + patientId + '&appointmentId=' + this.appointmentId+"&limit="+pageLimit+"&page="+pages)
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
    sessionStorage.setItem("appointmentId", this.appointmentId);
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  getConsultantReports(doctorId: any,isConcat: boolean): void {
    // let body = {
    //   "appointmentId": this.appointmentId,
    //   "roleName": this.user.roleNames[0]
    // }
    let pageLimit = this.apisPages.consultantReport.limit; 
    let pages = (isConcat ? this.apisPages.consultantReport.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
    .getWithBearer(this.apiConfig.getConsultantReportForAllRole+this.envAndUrlService.UUID+"&DoctorId="+doctorId+"&PatientId="+this.patientId+"&RoleName="+this.user.roleNames[0]+"&limit="+pageLimit+"&page="+pages)
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

  homeClick(): void {
    this.router.navigate([
      this.routeConfig.familyDoctorDashboardPath
    ]);
  }

  getAllNotesByUserId(isConcat: boolean): void {
    let pageLimit = this.apisPages.notes.limit; 
    let pages = (isConcat ? this.apisPages.notes.page : 1);
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllNotesByUserId + this.loginUserId + "&AppointmentId=" + this.appointmentId+"&limit="+pageLimit+"&page="+pages)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              //this.notes = res.result.items;
              if (!isConcat) {
                this.resetApisPage(this.apisPages.notes);
                this.notesUl.nativeElement.scrollTop = 0;
                this.notes = [];
              }
              this.notes = [...this.notes, ...res.result.items];
              this.paginationHandler(res.result, this.apisPages.notes);
            }
          } catch (e) {
            console.log("Success Exception PatientDetailsComponent getAllNotesByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getAllNotesByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getAllNotesByUserId " + e);
          }
        }
      );
  }

  addNote() {

    const dialogRef = this.dialog.open(NotesModelComponent, {
      data: { "loginUserId": this.loginUserId, "noteId": "", "note": "", "appointmentId": this.nextAppointment ? this.nextAppointment.appointmentId : this.envAndUrlService.UUID }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      // this.result = dialogResult;
      if (dialogResult == true) {
        this.getAllNotesByUserId(false);
      }
    });

  }

  onTabChange(event: MatTabChangeEvent) {
    this.selectedTab = event.tab.textLabel
  }

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          this.getAllUpcomingAppointment();
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
            console.log("Success Exception PatientDetailsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          this.getAllUpcomingAppointment();
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PatientDetailsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PatientDetailsComponent getUserTimezoneOffset " + e);
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

  deleteDocumentById(docId: any){
    const message = ValidationMessages.confirmDocumentDelete;

    const dialogData = new ConfirmModel("", message);

    const dialogRef = this.dialog.open(ConfirmModalComponent, {
      maxWidth: "800px",
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
        this.stickyBarService.showLoader("");
        this.apiService
          .deleteWithBearer(this.apiConfig.deleteDocumentById + docId)
          .subscribe(
            (res: any) => {
              try {
                if (res.result.statusCode == 200) {
                  this.getDocumentListByAppointmentId(false);
                  this.stickyBarService.showSuccessSticky(res.result.message);
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception PatientDetailsComponent deleteNote " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error PatientDetailsComponent deleteNote " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception PatientDetailsComponent deleteNote " + e);
              }
            });
      }
    });
  }

  gotoSamvaad(appointmentId, meetingID, Title, patientId, roleName){
    if(Title === null || Title === undefined || Title === ''){
      Title = "Video Conference";
    }

    if(appointmentId === null || appointmentId === undefined || appointmentId === ''){
      appointmentId = this.envAndUrlService.UUID;
    }

    if(patientId === null || patientId === undefined || patientId === ''){
      patientId = this.envAndUrlService.UUID;
    }

    // this.router.navigate([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    const url = this.router.createUrlTree([this.routeConfig.samvaad + "/" + Title + "/" + appointmentId + "/" + meetingID + "/" + patientId + "/" + roleName]);
    window.open(window.location.origin + "/#" + url.toString(), '_blank');
  }

  setEmailReply(replyMailId: any) {
    if (replyMailId) {
      sessionStorage.setItem('replyMailId', replyMailId);
      sessionStorage.setItem('replySubject', '');
    } else {
      sessionStorage.setItem('replyMailId', '');
      sessionStorage.setItem('replySubject', '');
    }
    sessionStorage.setItem("appointmentId", this.appointmentId);
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
        this.getDocumentListByAppointmentId(true);
      }else if (type == "notes" && this.apisPages.notes.more) {
        this.apisPages.notes.page += 1;
        this.getAllNotesByUserId(true);
      }else if (type == "messages" && this.apisPages.userMessages.more) {
        this.apisPages.userMessages.page += 1;
        this.getAllUserEmailsByUserId(this.loginUserId, this.appointmentDetails.patientId, true);
      }else if (type == "consultantReport" && this.apisPages.consultantReport.more) {
        this.apisPages.consultantReport.page += 1;
        this.getConsultantReports(this.appointmentDetails.doctorId, true);
      }
    }
  }
}