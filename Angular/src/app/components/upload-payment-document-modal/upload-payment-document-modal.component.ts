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
  selector: 'app-upload-payment-document-modal',
  templateUrl: './upload-payment-document-modal.component.html',
  styleUrls: ['./upload-payment-document-modal.component.scss']
})
export class UploadPaymentDocumentModalComponent implements OnInit {

  documentCategories = ["Health Record", "Diagnostic Images-Scans", "Diagnostic Reports", "Medical Report", "Others"];
  reportDocumentPaths: any[] = [];
  selectedDocumentCategory: string = "";
  errMessage: string = "";
  fileUpload: any;

  constructor(public dialogRef: MatDialogRef<UploadPaymentDocumentModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private dialog: MatDialog) {
  }

  ngOnInit(): void {
  }

  selectFile(event) {
    if (event.target.files.length > 0) {
      this.reportDocumentPaths = [];
      //let file = event.target.files[0];
      this.fileUpload = event;
      this.reportDocumentPaths.push(this.fileUpload);
      this.reportDocumentPaths.push(this.selectedDocumentCategory);
    }
  }

  onDismiss(): void {
    this.reportDocumentPaths = [];
    this.dialogRef.close(this.reportDocumentPaths);
  }

  uploadDocument(): void {
    if (this.selectedDocumentCategory && this.fileUpload) {
      this.dialogRef.close(this.reportDocumentPaths);
    } else {
      this.reportDocumentPaths = [];
      this.errMessage = ValidationMessages.allFieldsRequired;
    }
  }

  categoryDoc(value) {
    this.reportDocumentPaths = [];
    this.selectedDocumentCategory = value;
    this.reportDocumentPaths.push(this.fileUpload);
    this.reportDocumentPaths.push(this.selectedDocumentCategory);
  }
}
