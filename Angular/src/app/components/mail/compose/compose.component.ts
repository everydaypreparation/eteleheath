import { Component, OnInit, Renderer2, ElementRef, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { RouteConfig } from 'src/app/configs/route.config';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpResponse, HttpEventType } from '@angular/common/http';
import { HelperService } from 'src/app/services/helper.service';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../../confirm-modal/confirm-modal.component';
import { MatDialog } from '@angular/material/dialog';
declare const $: any;

@Component({
  selector: 'app-compose',
  templateUrl: './compose.component.html',
  styleUrls: ['./compose.component.scss']
})
export class ComposeComponent implements OnInit {

  @ViewChild('attachFile')
  myInputVariable: ElementRef;

  public Editor = ClassicEditor;

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

  eR: any = null;
  dropdownEmailsSettings: any;
  user: any;
  userEmails: any[];
  selectedEmailItems: Array<any> = [];
  loginUserId: any;

  selectedFiles: FileList;
  progressInfos = [];
  message = '';
  j: any = 0;
  selectAttachId: any;
  currentIndex: any;
  //fileInfos: Observable<any>;

  currentMailId: any;
  replay: any;
  isCompleted: any;
  status: any;

  composeBody = {
    "subject": "",
    "messagesText": "",
    "senderUserIds": "",
    "receiverUserIds": "",
    "parentId": "",
    "messageId": "",
    "appointmentId": ""
  };

  draftBody = {
    "subject": "",
    "messagesText": "",
    "senderUserIds": "",
    "receiverUserIds": "",
    "parentId": ""
  };

  onBoardUserBody = {
    "id": "",
    "isCompleted": true
  };

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private _Activatedroute: ActivatedRoute,
    private envAndUrlService: EnvAndUrlService,
    private helperService: HelperService,
    private dialog: MatDialog) { }

  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.replay = this._Activatedroute.snapshot.params['replay'];
    this.isCompleted = this._Activatedroute.snapshot.params['isCompleted'];
    this.status = this._Activatedroute.snapshot.params['status'];
    if (this.user) {
      this.loginUserId = this.user.id;
    }
    this.saveNewUserEmail();
    //this.fileInfos = this.apiService.getFiles();

    this.dropdownEmailsSettings = {
      singleSelection: false,
      idField: 'id',
      textField: 'fullName',
      selectAllText: 'All',
      unSelectAllText: 'All',
      itemsShowLimit: 4,
      allowSearchFilter: true,
    };
    this.getAllUserEmails();
  }

  saveNewUserEmail() {
    this.draftBody.senderUserIds = this.loginUserId;
    this.draftBody.parentId = this.loginUserId;
    this.apiService
      .postWithBearer(this.apiConfig.draftMessages, this.draftBody)
      .subscribe(
        (res: any) => {
          try {
            this.currentMailId = res.result.messageId;
          } catch (e) {
            console.log("Success Exception ComposeComponent saveNewUserEmail " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ComposeComponent saveNewUserEmail " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ComposeComponent saveNewUserEmail " + e);
          }
        }
      );
  }


  getAllUserEmails(): void {
    this.apiService.getWithBearer(this.apiConfig.getUserEmails + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userEmails = res.result.items;
              if (this.replay == 'replay') {
                let replyMailId = sessionStorage.getItem('replyMailId');
                let replySubject = sessionStorage.getItem('replySubject');
                if (replyMailId) {
                  //this.selectedEmailItems = replyMailId.split(',');
                  this.selectedEmailItems = this.userEmails.filter(i => replyMailId.split(',').includes(i.emailId));
                  if (this.selectedEmailItems.length == 0) {
                    this.selectedEmailItems = this.userEmails.filter(i => replyMailId.split(',').includes(i.id));
                  }
                }
                if (replySubject) {
                  //$('#userSubject').val('Re: ' + replySubject);
                  this.composeBody.subject = replySubject.replace('(no subject)', '');
                }
              } else {
                this.selectedEmailItems = [];
                $('#userSubject').val('');
              }
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ComposeComponent getAllUserEmails " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ComposeComponent getAllUserEmails" + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ComposeComponent getAllUserEmails" + e);
          }
        }
      );
  }

  submitFormClick() {
    let selectedIds = this.selectedEmailItems.map(r => r.id);
    this.composeBody.receiverUserIds = selectedIds.toString();
    this.composeBody.senderUserIds = this.loginUserId;
    this.composeBody.messageId = this.currentMailId;
    this.composeBody.parentId = this.loginUserId;
    this.sendMessage();
  }

  sendMessage(): void {
    // Close the dialog, return true
    let flag;
    if (this.selectedEmailItems && this.selectedEmailItems.length > 0) {
      if (!this.composeBody.subject && !this.composeBody.messagesText) {
        flag = confirm("Are you sure you want send this mail without subject and message?");
      } else {
        flag = true;
      }
      if (flag) {
        this.stickyBarService.showLoader("");
        let newSubject;
        if (this.composeBody && this.composeBody.subject) {
          newSubject = this.composeBody.subject.replace('Re:', '');
        } else {
          newSubject = this.composeBody.subject;
        }
        let appointmentId;
        if (sessionStorage.getItem('appointmentId')) {
          appointmentId = sessionStorage.getItem('appointmentId');
        } else {
          appointmentId = this.envAndUrlService.UUID;
        }

        this.composeBody.subject = newSubject;
        this.composeBody.appointmentId = appointmentId;
        this.apiService
          .postWithBearer(this.apiConfig.sendMessages, this.composeBody)
          .subscribe(
            (res: any) => {
              try {
                this.stickyBarService.hideLoader("");
                if (res.result.statusCode == 200) {
                  // this.myInputVariable.nativeElement.value = "";
                  // this.selectedEmailItems = [];
                  if (this.isCompleted) {
                    this.onBoardUserStatus(this.isCompleted);
                  } else {
                    this.stickyBarService.showSuccessSticky(res.result.message);
                    this.router.navigate([this.routeConfig.messagingInboxPath]);
                  }
                } else {
                  this.stickyBarService.showErrorSticky(res.result.message);
                }
              } catch (e) {
                console.log("Success Exception ComposeComponent sendMessage " + e);
              }
            },
            (err: any) => {
              try {
                this.stickyBarService.hideLoader("");
                console.log("Error ComposeComponent sendMessage " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception ComposeComponent sendMessage " + e);
              }
            }
          );
      }
    } else {
      $('.errorEmailMessage').text('Please select atleast one email Id');
    }
  }

  selectFiles(event): void {
    //this.progressInfos = [];
    this.selectedFiles = event.target.files;
    this.uploadFiles();
  }

  uploadFiles(): void {
    this.message = '';

    for (let i = 0; i < this.selectedFiles.length; i++) {
      const fsize = this.selectedFiles[i].size;
      const file = Math.round((fsize / 1024));
      let fName = this.selectedFiles[i].name;
      let extension = fName.substr((fName.lastIndexOf('.') + 1));
      let preExtention = "pdf,xml,csv,dat,tar,docx,odt,rtf,txt,xlsx,xls,zip";
      if (preExtention.includes(extension)) {
        if (file < (10 * 1024)) {
          this.upload(this.j, this.selectedFiles[i], fsize);
        } else {
          $('.fileSizeErrorMessage').show();
          setTimeout(function () {
            $('.fileSizeErrorMessage').hide();
          }, 5000);
        }
      } else {
        $('.extentionMessage').show();
        setTimeout(function () {
          $('.extentionMessage').hide();
        }, 5000);
      }
    }
  }

  upload(idx, file, fsize): void {

    this.progressInfos[idx] = { value: 0, fileName: file.name, attachmentId: 0 };
    this.apiService.uploadAttachment(this.apiConfig.uploadAttachment, fsize, file, this.currentMailId)
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress) {
          this.progressInfos[idx].value = Math.round(100 * event.loaded / event.total);
        }

        if (event instanceof HttpResponse) {
          this.progressInfos[idx].attachmentId = event.body.result.attachmentId;
        }
      },
        err => {
          this.progressInfos[idx].value = 0;
          this.progressInfos[idx].attachmentId = 0;
          this.message = 'Could not upload the file:' + file.name;
        });
    this.j = this.j + 1;

  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  deleteAttachment(attachmentId: any, index: any) {
    // Close the dialog, return true
    this.stickyBarService.showLoader("");
    this.myInputVariable.nativeElement.value = "";
    this.apiService
      .deleteWithBearer(this.apiConfig.deleteAttachment + attachmentId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              $('.perSpan' + attachmentId).hide();
              //this.stickyBarService.showSuccessSticky(res.result.message);
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ComposeComponent deleteAttachment " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ComposeComponent deleteAttachment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ComposeComponent deleteAttachment " + e);
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

  onBoardUserStatus(requestId: any) {
    if (this.status == 'Completed') {
      this.onBoardUserBody.isCompleted = false;
    } else {
      this.onBoardUserBody.isCompleted = true;
    }
    this.onBoardUserBody.id = requestId;
    this.stickyBarService.showLoader("");
    this.apiService
      .putWithBearer(this.apiConfig.updateUserOnboardRequest, this.onBoardUserBody)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            this.router.navigate([this.routeConfig.adminRequestDoctorsPath]);
          } catch (e) {
            console.log("Success Exception ComposeComponent onBoardUserStatus " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ComposeComponent onBoardUserStatus " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ComposeComponent onBoardUserStatus " + e);
          }
          this.stickyBarService.hideLoader("");
        }
      );
  }

  onSelectAll(items: any) {
    if (this.userService.userRole) {
      const role = this.userService.userRole;
      if (role == "ADMIN") {
        const message = ValidationMessages.selectAllUsers;
        const dialogData = new ConfirmModel("", message);
        const dialogRef = this.dialog.open(ConfirmModalComponent, {
          maxWidth: "800px",
          data: dialogData
        });

        dialogRef.afterClosed().subscribe(dialogResult => {
          if (dialogResult == true) {
          } else {
            this.selectedEmailItems = [];
          }
        });
      }
    }
  }

  }
