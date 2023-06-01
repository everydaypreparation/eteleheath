import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortable } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { MatDialog } from '@angular/material/dialog';
import { saveAs } from 'file-saver';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-view-mail',
  templateUrl: './view-mail.component.html',
  styleUrls: ['./view-mail.component.scss']
})
export class ViewMailComponent implements OnInit {

  user: any;
  userEmails: any;
  loginUserId: string = null;
  mailId: string = null;
  page: string = null;
  receiversArray: Array<any> = [];


  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private userService: UserService,
    private changeDetectorRef: ChangeDetectorRef,
    private activatedRoute: ActivatedRoute,
    private helperService: HelperService) {
    this.userService.setNav("messaging");
  }


  viewAttachment = {
    "AttachmentId": ""
  };

  restoreBody = {
    "userId": "",
    "messageId": ""
  };

  ngOnInit(): void {
    this.mailId = this.activatedRoute.snapshot.params['id'];
    this.page = this.activatedRoute.snapshot.params['page'];
    this.user = this.userService.getUser();
    if (this.user) {
      this.loginUserId = this.user.id;
      this.getAllUserEmailByUserId(this.loginUserId);
    }
  }

  getAllUserEmailByUserId(loginUserId: any): void {
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.getEmailById + loginUserId + '&MessageId=' + this.mailId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.userEmails = res.result;
              this.receiversArray = this.userEmails.receiverUserIds.split(',');
              this.changeDetectorRef.detectChanges();
            } else {
              //this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception ViewMailComponent getAllUserEmailsByUserId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewMailComponent getAllUserEmailsByUserId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewMailComponent getAllUserEmailsByUserId " + e);
          }
        }
      );
  }

  downloadAttachment(attachmentId: any, attachmentName: any) {
    this.apiService
      .getWithBearer(this.apiConfig.downloadAttachment + attachmentId)
      .subscribe(
        (res: any) => {

          try {
            if (res.result.statusCode == 200) {
              var blob = this.convertBase64ToBlobData(res.result.filedate, res.result.mimeType);
              saveAs(blob, attachmentName);
            }
          } catch (e) {
            console.log("Success Exception ViewMailComponent downloadAttachment " + e);
          }
        },
        (err: any) => {
          try {
            console.log("Error ViewMailComponent downloadAttachment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewMailComponent downloadAttachment " + e);
          }
        }
      );
  }

  convertBase64ToBlobData(base64Data: string, contentType: string, sliceSize = 512) {
    const byteCharacters = atob(base64Data);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
      const slice = byteCharacters.slice(offset, offset + sliceSize);

      const byteNumbers = new Array(slice.length);
      for (let i = 0; i < slice.length; i++) {
        byteNumbers[i] = slice.charCodeAt(i);
      }

      const byteArray = new Uint8Array(byteNumbers);

      byteArrays.push(byteArray);
    }
    const blob = new Blob(byteArrays, { type: contentType });
    return blob;

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
    this.router.navigate([this.routeConfig.messagingComposePath, 'replay']);
  }

  restoreTrashMailByMailId(mailId) {
      this.restoreBody.userId = this.loginUserId;
      this.restoreBody.messageId =  mailId;
      this.apiService.postWithBearer(this.apiConfig.restore , this.restoreBody)
        .subscribe(
          (res: any) => {
            try {
              this.router.navigate([this.routeConfig.messagingTrashPath]);
            } catch (e) {
              console.log("Success Exception HelperService restoreTrashMailByMailId " + e);
            }
          },
          (err: any) => {
            try {
              console.log("Error ViewMailComponent restoreTrashMailByMailId " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception ViewMailComponent restoreTrashMailByMailId " + e);
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
      }else if (role == "CONSULTANT") {
        this.router.navigate([this.routeConfig.consultantDashboardPath]);
      }
    }
  }
}
