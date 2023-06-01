import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-cancel-appointment-modal',
  templateUrl: './cancel-appointment-modal.component.html',
  styleUrls: ['./cancel-appointment-modal.component.scss']
})
export class CancelAppointmentModalComponent implements OnInit {

  title: string;
  message: string;
  reason: any;
  cancelArr: any[] = [];

  constructor(public dialogRef: MatDialogRef<CancelAppointmentModalComponent>, @Inject(MAT_DIALOG_DATA) public data: ConfirmModel) {
    // Update view with given values
    this.title = data.title;
    this.message = data.message;
  }

  ngOnInit(): void {
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.cancelArr.push(true);
    this.cancelArr.push(this.reason);
    this.dialogRef.close(this.cancelArr);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.cancelArr.push(false);
    this.cancelArr.push(this.reason);
    this.dialogRef.close(this.cancelArr);
  }
}

/**
 * Class to represent confirm dialog model.
 *
 * It has been kept here to keep it as part of shared component.
 */
export class ConfirmModel {

  constructor(public title: string, public message: string) {
  }

}
