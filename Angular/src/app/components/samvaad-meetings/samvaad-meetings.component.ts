import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { SamvaadmeetingsService } from '../../services/samvaadmeetings.service';
import { takeUntil, debounce, skip } from 'rxjs/operators';
import { ApiConfig } from 'src/app/configs/api.config';
import { Subject, timer, fromEvent } from 'rxjs';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { SignalRService } from '../../services/signal-r.service';
import { CKEditorComponent, CKEditor5 } from '@ckeditor/ckeditor5-angular';
import { Router, ActivatedRoute } from '@angular/router';
import { UserService } from 'src/app/services/user.service';
import { ApiService } from 'src/app/services/api.service';
import { UploadDocumentModalComponent } from '../upload-document-modal/upload-document-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { HelperService } from 'src/app/services/helper.service';
import { ViewDocModalComponent } from '../view-doc-modal/view-doc-modal.component';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { RouteConfig } from 'src/app/configs/route.config';
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-samvaad-meetings',
  templateUrl: './samvaad-meetings.component.html',
  styleUrls: ['./samvaad-meetings.component.scss']
})
export class SamvaadMeetingsComponent implements OnInit {

  @ViewChild('iframe') iframe: ElementRef;
  @ViewChild('ckeditor', { static: false }) public ckeditorComponent: CKEditorComponent;

  public Editor = ClassicEditor;
  meetingID: string = '';
  roleName: string = '';
  fullName: string = '';
  joinMeetingURL: SafeResourceUrl;

  noteId: string = '';
  pageTitle: string = '';
  isLoadingResults = false;
  apiRunning = false;

  user: any = null;
  subscribeUser: any;
  chat: string = "";
  patientId: string = "";
  patientRecords: any[];
  isShowSamvaadNav: boolean = false;
  subscribeSamvaadNav: any = null;
  selectedMobileTabIndex: number = 0;
  selectedTabIndex: number = 0;
  timezones: any[] = [];
  userTimezone: any = {};
  notesText: string = "";

  noteBody = {
    "notesId": "",
    "notes": "",
    "userId": "",
    "appointmentId": ""
    // "notes": "",
    // "slotId": "",
    // "userId": this.loginUserId
  };

  meetingLogBody = {
    "userId": "",
    "appointmentId": "",
    "meetingId": ""
  };

  defaultConfig = {
    toolbar: {
      items: [
        'heading',
        '|',
        'bold',
        'italic',
        '|',
        'bulletedList',
        'numberedList',
        '|',
        'undo',
        'redo',
        'link'
      ]
    },
    table: {
      contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
    },
    language: 'en'
  };

  constructor(
    private stickyBarService: StickyBarService,
    private SamvaadService: SamvaadmeetingsService,
    private sanitizer: DomSanitizer,
    public signalRService: SignalRService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private apiService: ApiService,
    private helperService: HelperService,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private propConfig: PropConfig,
    private routeConfig: RouteConfig,
  ) {
    this.user = this.userService.getUser();
    this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {
      this.user = this.userService.getUser();
    });
    this.subscribeSamvaadNav = this.userService.toggleSamvaadNav.subscribe((data: any) => {
      this.selectedMobileTabIndex = 0;
      this.isShowSamvaadNav = !this.isShowSamvaadNav;
    });
  }

  ngOnInit(): void {

    localStorage.removeItem("samvaadMeeting");
    //console.log(ClassicEditor.builtinPlugins.map(plugin => plugin.pluginName));

    this.meetingID = this.route.snapshot.params['id'];
    this.fullName = this.user.name + " " + this.user.surname;
    this.pageTitle = decodeURI(this.route.snapshot.params['title']);
    this.patientId = this.route.snapshot.params['patientId'];
    this.roleName = this.route.snapshot.params['roleName'];

    //signalr
    this.signalRService.connect(this.meetingID);
    this.getUserTimezoneOffset();
    //get patient document list

    if (this.roleName != "Admin") {
      this.getDocumentListByAppointmentId();

      //Get Notes
      this.apiService
        .getWithBearer(this.apiConfig.getNoteByUserIdAndAppointmentId + this.user.id + "&AppointmentId=" + this.route.snapshot.params['appointmentId'])
        .subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.noteId = res.result.notesId;
                this.notesText = res.result.notes;
              }
            } catch (e) {
              console.log("Success Exception DashboardComponent getNoteByUserIdAndAppointmentId " + e);
            }

            this.onReady(this.Editor);
          },
          (err: any) => {
            try {
              console.log("Error DashboardComponent getNoteByUserIdAndAppointmentId " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception DashboardComponent getNoteByUserIdAndAppointmentId " + e);
            }

            this.onReady(this.Editor);
          }
        );
    }

    //samvaad
    this.SamvaadService.joinMeeting(this.apiConfig.JoinSamvaadMeeting, this.meetingID, this.fullName)
      .subscribe(
        data => {

          if (data["result"] == "Meeting Over") {
            this.stickyBarService.showErrorSticky(data["result"]);
            this.router.navigate([this.routeConfig.signInPath]);
          }
          else {
            this.joinMeetingURL = this.sanitizer.bypassSecurityTrustResourceUrl(data["result"]);
          }

          /**
           * Here we need to write code for create meeting statistics
           */

          this.meetingLogBody.userId = this.user.id;
          this.meetingLogBody.appointmentId = this.route.snapshot.params['appointmentId'];
          this.meetingLogBody.meetingId = this.meetingID;

          this.apiService
            .postWithBearer(this.apiConfig.createMeetingLog, this.meetingLogBody)
            .subscribe(
              data => {

              },
              error => {
                console.log('error : ' + error);
              });

        },
        error => {
          console.log('error : ' + error);
        });
  }

  ngAfterViewInit(): void {
    fromEvent(this.iframe.nativeElement, 'load')
      // Skip the initial load event and only capture subsequent events.
      .pipe(skip(1))
      .subscribe((event: Event) => {
        console.log(event.target);
        //this.isLoadingResults = true;
      });
    setTimeout(() => {
      this.userService.toggleSamvaadNavIconClick({ isShow: true });
    }, 1000);
  }

  sendChatMessage(): void {

    this.signalRService.sendChatMessageToHub(this.chat).subscribe({
      next: _ => this.chat = '',
      error: (err) => console.error(err)
    });
  }

  saveData(data) {

    if (this.apiRunning == false) {
      if (this.noteId == '') {
        this.apiRunning = true;
        this.noteBody.notes = data;
        this.noteBody.userId = this.user.id;

        this.noteBody.appointmentId = this.route.snapshot.params['appointmentId'];

        this.apiService
          .postWithBearer(this.apiConfig.saveOrUpdateNote, this.noteBody)
          .subscribe(
            data => {
              this.apiRunning = false;
              this.noteId = data["result"].notesId;
            },
            error => {
              this.apiRunning = false;
              console.log('error : ' + error);
            });
      }
      else {
        this.apiRunning = true;
        this.noteBody.notesId = this.noteId;
        this.noteBody.notes = data;
        this.noteBody.userId = this.user.id;
        this.noteBody.appointmentId = this.route.snapshot.params['appointmentId'];

        this.apiService
          .putWithBearer(this.apiConfig.updateNote, this.noteBody)
          .subscribe(
            data => {
              this.apiRunning = false;
              //console.log(data["result"]);
            },
            error => {
              this.apiRunning = false;
              console.log('error : ' + error);
            });
      }
    }
    //console.log(data);
    // Here you can save the data to the backend and return a promise to that action.
  }

  onReady(editor: CKEditor5.Editor) {
    this.ckeditorComponent.change
      .pipe(debounce(() => timer(5000)))
      .subscribe(() => this.saveData(this.notesText));
  }

  recordUploadClick() {

    if (this.roleName == "Patient") {
      const dialogRef = this.dialog.open(UploadDocumentModalComponent, {
        data: { "loginUserId": this.patientId, "appointmentId": this.route.snapshot.params['appointmentId'] }
      });

      dialogRef.afterClosed().subscribe(dialogResult => {
        // Not needed now, changes will be made if some logic change in future
        if (dialogResult == true) {
          this.getDocumentListByAppointmentId();
        }
      });
    }
    else {
      const dialogRef = this.dialog.open(UploadDocumentModalComponent, {
        data: { "loginUserId": this.user.id, "appointmentId": this.route.snapshot.params['appointmentId'] }
      });

      dialogRef.afterClosed().subscribe(dialogResult => {
        // Not needed now, changes will be made if some logic change in future
        if (dialogResult == true) {
          this.getDocumentListByAppointmentId();
        }
      });
    }
  }

  getDocumentListByAppointmentId(): void {

    if (this.roleName == "Patient") {
      this.stickyBarService.showLoader("");
      this.apiService
        .getWithBearer(this.apiConfig.getDocumentListByUserId + this.patientId)
        .subscribe(
          (res: any) => {
            //console.log(res);
            try {
              if (res.result.statusCode == 200) {
                this.patientRecords = res.result.items;
                for (let record of this.patientRecords) {
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
              }
            } catch (e) {
              console.log("Success Exception SamvaadMeetingsComponent getDocumentListByAppointmentId " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error SamvaadMeetingsComponent getDocumentListByAppointmentId " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception SamvaadMeetingsComponent getDocumentListByAppointmentId " + e);
            }
          }
        );
    }
    else {
      this.stickyBarService.showLoader("");
      this.apiService
        .getWithBearer(this.apiConfig.getDocumentListByAppointmentId + this.route.snapshot.params['appointmentId'])
        .subscribe(
          (res: any) => {
            //console.log(res);
            try {
              if (res.result.statusCode == 200) {
                this.patientRecords = res.result.items;
                for (let record of this.patientRecords) {
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
              }
            } catch (e) {
              console.log("Success Exception SamvaadMeetingsComponent getDocumentListByAppointmentId " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error SamvaadMeetingsComponent getDocumentListByAppointmentId " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception SamvaadMeetingsComponent getDocumentListByAppointmentId " + e);
            }
          }
        );
    }
  }

  downloadDocument(documentId) {
    this.helperService.downloadDocumentById(documentId);
  }

  patientRecordOpenDocClick(documentId: any) {
    const dialogRef = this.dialog.open(ViewDocModalComponent, {
      width: "100%",
      height: "100%",
      data: { "documentId": documentId }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult == true) {
      }
    });
  }

  onTabChange(event: MatTabChangeEvent) {
    if (event.index == 3) {
      this.getDocumentListByAppointmentId();
    }
  }

  onMobileTabChange(event: MatTabChangeEvent) {
    if (event.index == 4) {
      this.getDocumentListByAppointmentId();
    }
  }

  onResize(event: any) {
    if (event.target.innerWidth <= 768) {
      // this.selectedMobileTabIndex = 0;
    } else {
      this.selectedMobileTabIndex = 0;
      this.isShowSamvaadNav = false;
      // this.selectedTabIndex = 0;
    }
  }

  ngOnDestroy(): void {
    if (this.subscribeSamvaadNav != null) {
      this.subscribeSamvaadNav.unsubscribe();
    }
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
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
}
