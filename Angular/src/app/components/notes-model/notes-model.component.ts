import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AppointmentSlotService } from 'src/app/services/appointment-slot.service';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';

export interface ConfirmDialogModel {
  loginUserId: number;
  noteId: any;
  note: any;
  appointmentId: any;
}

@Component({
  selector: 'app-notes-model',
  templateUrl: './notes-model.component.html',
  styleUrls: ['./notes-model.component.scss']
})

export class NotesModelComponent implements OnInit {

  public Editor = ClassicEditor;

  noteId: string;
  message: string;
  availabilitySlots: any[] = [];
  loginUserId: any;
  eR: any = null;
  editFlag = false;
  appointmentId: any;

  noteBody = {
    "notesId": "",
    "notes": "",
    "userId": "",
    "appointmentId": ""
    // "notes": "",
    // "slotId": "",
    // "userId": this.loginUserId
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

  constructor(public dialogRef: MatDialogRef<NotesModelComponent>, private stickyBarService: StickyBarService
    , @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel, private userService: UserService,
    private appointmentSlotService: AppointmentSlotService, private apiConfig: ApiConfig, private apiService: ApiService, private validationService: ValidationService) {
    // Update view with given value
    this.loginUserId = data.loginUserId;
    this.noteId = data.noteId;
    this.noteBody.notes = data.note;
    this.appointmentId = data.appointmentId;
  }

  ngOnInit(): void {
    //this.getAvailabilitySlotById();
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  getAvailabilitySlotById(): void {
    this.stickyBarService.showLoader("");
    this.appointmentSlotService
      .getAvailabilitySlotWithBearer(this.apiConfig.getAllAppointmentSlotbyDoctorId + this.loginUserId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.availabilitySlots = res.result.items;
            }
          } catch (e) {
            console.log("Success Exception NotesModelComponent getAvailabilitySlotById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error NotesModelComponent getAvailabilitySlotById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception NotesModelComponent getAvailabilitySlotById " + e);
          }
        }
      );
  }

  saveNotes() {
    //console.log(this.noteBody, 'table-border');
    if (!this.isValidated()) {
    } else {
      this.stickyBarService.showLoader("");
      this.noteBody.userId = this.loginUserId;
      this.noteBody.appointmentId = this.appointmentId;
      this.apiService
        .postWithBearer(this.apiConfig.saveNote, this.noteBody)
        .subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.dialogRef.close(true);
                this.stickyBarService.showSuccessSticky(res.result.message);
              } else {
                this.dialogRef.close(true);
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception NotesModelComponent saveNotes " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              console.log("Error NotesModelComponent saveNotes " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception NotesModelComponent saveNotes " + e);
            }
            this.stickyBarService.hideLoader("");
          }
        );
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["notes"] = this.validationService.getValue(this.validationService.setValue(this.noteBody.notes).required(ValidationMessages.requiredField).obj);

    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  updateNotes() {
    if (!this.isValidated()) {
    } else {
      this.stickyBarService.showLoader("");
      this.noteBody.notesId = this.noteId;
      this.noteBody.userId = this.loginUserId;
      this.noteBody.appointmentId = this.appointmentId;
      this.apiService
        .putWithBearer(this.apiConfig.updateNote, this.noteBody)
        .subscribe(
          (res: any) => {
            try {
              if (res.result.statusCode == 200) {
                this.dialogRef.close(true);
                this.stickyBarService.showSuccessSticky(res.result.message);
              } else {
                this.dialogRef.close(true);
                this.stickyBarService.showErrorSticky(res.result.message);
              }
            } catch (e) {
              console.log("Success Exception NotesModelComponent updateNotes " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              console.log("Error NotesModelComponent updateNotes " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception NotesModelComponent updateNotes " + e);
            }
            this.stickyBarService.hideLoader("");
          }
        );
    }
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
