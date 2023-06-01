import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ConfirmModel, ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';
import { ApiService } from 'src/app/services/api.service';

export interface DialogData {
  loginUserId: number;
  documentId: number;
  appointmentId: number;
}

@Component({
  selector: 'app-upload-document-modal',
  templateUrl: './upload-document-modal.component.html',
  styleUrls: ['./upload-document-modal.component.scss']
})
export class UploadDocumentModalComponent implements OnInit {

  documentCategories = ["Health Record", "Diagnostic Images-Scans", "Diagnostic Reports", "Medical Report", "Others"];
  loginUserId: any;
  documentId: any;
  appointmentId: any;
  reportDocumentPaths: any[] = [];
  selectedDocumentCategory: string = "";
  errMessage: string = "";

  constructor(public dialogRef: MatDialogRef<UploadDocumentModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private dialog: MatDialog) {

      this.loginUserId = data.loginUserId;
      this.documentId = data.documentId;
      this.appointmentId = data.appointmentId;
    }

  ngOnInit(): void {
  }

  selectFile(event) {
    this.errMessage = "";
    if(event.target.files.length > 0) {
      this.reportDocumentPaths = [];
      let file = event.target.files[0];
      this.reportDocumentPaths.push(file);
    }
  }

  editDocument(): void {
    if(this.reportDocumentPaths.length > 0) {

      const message = ValidationMessages.confirmDocumentEdit;

      const dialogData = new ConfirmModel("", message);

      const dialogRef = this.dialog.open(ConfirmModalComponent, {
        maxWidth: "800px",
        data: dialogData
      });

      dialogRef.afterClosed().subscribe(dialogResult => {
        if (dialogResult == true) {
          const formdata: FormData = new FormData();
          formdata.append('DocumentId', this.documentId);
          formdata.append('UserId', this.loginUserId);
          for(let reportDoc of this.reportDocumentPaths) {
            formdata.append('ReportDocumentPaths', reportDoc, this.selectedDocumentCategory+'.'+reportDoc.name.split('.')[1]);
          }

          this.stickyBarService.showLoader("");
          this.apiService
            .postFormDataWithBearer(this.apiConfig.editDocumentById, formdata)
            .subscribe(
              (res: any) => {
                this.dialogRef.close(true);
                try {
                  if (res.result.statusCode == 200) {
                    this.stickyBarService.showSuccessSticky(res.result.message);
                  } else {
                    this.stickyBarService.showErrorSticky(res.result.message);
                  }
                } catch (e) {
                  console.log("Success Exception UploadDocumentModalComponent editDocument " + e);
                }
                this.stickyBarService.hideLoader("");
              },
              (err: any) => {
                this.dialogRef.close(true);
                try {
                  this.stickyBarService.hideLoader("");
                  console.log("Error UploadDocumentModalComponent editDocument " + JSON.stringify(err));
                  this.apiService.catchError(err);
                } catch (e) {
                  console.log("Error Exception UploadDocumentModalComponent editDocument " + e);
                }
              }
            );
        } else {
          this.dialogRef.close(true);
        }
      });
    } else {
      this.errMessage = ValidationMessages.allFieldsRequired;
    }
  }

  uploadDocument() {
    if(this.reportDocumentPaths.length > 0 && this.selectedDocumentCategory) {
      const formdata: FormData = new FormData();
      formdata.append('UserId', this.loginUserId);
      formdata.append('Category', this.selectedDocumentCategory);
      if(this.appointmentId) {
        formdata.append('AppointmentId', this.appointmentId);
      }
      for(let reportDoc of this.reportDocumentPaths) {
        console.log(this.selectedDocumentCategory);
        formdata.append('ReportDocumentPaths', reportDoc, this.selectedDocumentCategory+'.'+reportDoc.name.split('.')[1]);
      }

      this.stickyBarService.showLoader("");
      this.apiService
        .postFormDataWithBearer(this.apiConfig.uploadDocument, formdata)
        .subscribe(
          (res: any) => {
            this.dialogRef.close(true);
            try {
              if (res.result.statusCode == 200) {
                this.stickyBarService.showSuccessSticky(res.result.message);
              } else {
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception UploadDocumentModalComponent uploadDocument " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            this.dialogRef.close(true);
            try {
              this.stickyBarService.hideLoader("");
              console.log("Error UploadDocumentModalComponent uploadDocument " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception UploadDocumentModalComponent uploadDocument " + e);
            }
          }
        );
    } else {
      this.errMessage = ValidationMessages.allFieldsRequired;
    }
  }

  categoryDoc(value) {
    this.selectedDocumentCategory = value;
  }

  onDismiss(): void {
    this.dialogRef.close(false);
  }
}
