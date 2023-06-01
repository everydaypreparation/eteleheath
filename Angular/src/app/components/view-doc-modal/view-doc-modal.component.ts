import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { HelperService } from 'src/app/services/helper.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { DomSanitizer } from '@angular/platform-browser';
declare const $: any;

export interface ConfirmDialogModel {
  documentId: any;
}

@Component({
  selector: 'app-view-doc-modal',
  templateUrl: './view-doc-modal.component.html',
  styleUrls: ['./view-doc-modal.component.scss']
})
export class ViewDocModalComponent implements OnInit {

  loginUserId: any;
  documentId: any;
  docVal: any;
  iframeVal: any;
  imgVal: any;
  message: any;
  documentName: any;

  constructor(public dialogRef: MatDialogRef<ViewDocModalComponent>, private stickyBarService: StickyBarService
    , @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel, private userService: UserService, private sanitizer: DomSanitizer,
    private apiConfig: ApiConfig, private apiService: ApiService, private validationService: ValidationService, private helperService: HelperService) {
    // Update view with given value
    this.documentId = data.documentId;
  }

  ngOnInit(): void {
    //this.getAvailabilitySlotById();
    this.showDocumentById(this.documentId);
    this.message = "Please wait...."
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  showDocumentById(documentId: any) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.downloadDocumentById + documentId)
      .subscribe(
        (res: any) => {
          console.log(res.result);
          try {
            if (res.result.statusCode == 200) {
              if (res.result.filedata) {
                if (res.result.filedata.includes(this.apiConfig.matchesUrl)) {
                  if (res.result.mimeType.includes('image/')) {
                    this.imgVal = res.result.filedata;
                    this.documentName = res.result.documentName.split('.')[0];
                  } else {
                    this.docVal = res.result.filedata;
                    this.documentName = res.result.documentName.split('.')[0];
                    // let aa = $('iframe-doc-viewer').val();
                    //  console.log(this.docVal);
                  }
                } else {
                  if (res.result.mimeType.includes('image/')) {
                    this.imgVal = "data:" + res.result.mimeType + ";base64," + res.result.filedata;
                    this.documentName = res.result.documentName.split('.')[0];
                  } else {
                    this.iframeVal = this.sanitizer.bypassSecurityTrustResourceUrl("data:" + res.result.mimeType + ";base64," + res.result.filedata);
                    this.documentName = res.result.documentName.split('.')[0];
                  }
                }
              }
              this.message = "";
            } else {
              this.message = ValidationMessages.documentNotFound;
            }
          } catch (e) {
            console.log("Success Exception ViewDocModalComponent showDocumentById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewDocModalComponent showDocumentById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewDocModalComponent showDocumentById " + e);
          }
        }
      );
  }

  downloadDocument() {
    this.helperService.downloadDocumentById(this.documentId);
  }

}
