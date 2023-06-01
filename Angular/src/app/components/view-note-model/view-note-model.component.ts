import { Component, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';

export interface ConfirmDialogModel {
  noteId: any;
}

@Component({
  selector: 'app-view-note-model',
  templateUrl: './view-note-model.component.html',
  styleUrls: ['./view-note-model.component.scss']
})
export class ViewNoteModelComponent implements OnInit {

  noteId: string;
  viewNotes: any;
  message: any;

  constructor(public dialogRef: MatDialogRef<ViewNoteModelComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,
    private apiService: ApiService, private apiConfig: ApiConfig, private stickyBarService: StickyBarService, private changeDetectorRef: ChangeDetectorRef) {
    // Update view with given values
    this.noteId = data.noteId;
  }

  ngOnInit(): void {
    this.message = "Please wait.";
    this.getNoteByNoteId(this.noteId);
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  getNoteByNoteId(noteId: any): void {
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.getNoteById + noteId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.viewNotes = res.result;
              this.message = "";
              this.changeDetectorRef.detectChanges();
            }
          } catch (e) {
            console.log("Success Exception ViewNoteComponent getNoteByNoteId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error ViewNoteComponent getNoteByNoteId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception ViewNoteComponent getNoteByNoteId " + e);
          }
        }
      );
  }

}

/**
 * Class to represent confirm dialog model.
 *
 * It has been kept here to keep it as part of shared component.
 */
export class ConfirmDialogModel {

  constructor(public title: string, public message: string) {
  }

}
