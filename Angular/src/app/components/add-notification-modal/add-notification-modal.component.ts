import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';
import { UserService } from 'src/app/services/user.service';
import { ValidationService } from 'src/app/services/validation.service';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
declare const $: any;

export interface ConfirmDialogModel {
  loginUserId: number;
  note: any;
  notificationId: any;
}

@Component({
  selector: 'app-add-notification-modal',
  templateUrl: './add-notification-modal.component.html',
  styleUrls: ['./add-notification-modal.component.scss']
})
export class AddNotificationModalComponent implements OnInit {

  notificationId: string;
  message: string;
  roles: any[] = [];
  loginUserId: any;
  eR: any = null;
  editFlag = false;
  dropdownRoleSettings: any;
  selectedRoleItems: Array<any> = [];


  notificationBody = {
    "roleName": [],
    "content": "",
    "title": ""
  };

  notificationUpdateBody = {
    "id":"",
    "roleName": [],
    "content": "",
    "title": ""
  };

  constructor(public dialogRef: MatDialogRef<AddNotificationModalComponent>, private stickyBarService: StickyBarService
    , @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel, private userService: UserService,
    private apiConfig: ApiConfig, private apiService: ApiService, private validationService: ValidationService) {
    // Update view with given value
    this.notificationId = data.notificationId;
    this.loginUserId = data.loginUserId;
  }

  ngOnInit(): void {

    this.dropdownRoleSettings = {
      singleSelection: false,
      idField: 'normalizedName',
      textField: 'displayName',
      selectAllText: 'All',
      unSelectAllText: 'All',
      itemsShowLimit: 3,
      allowSearchFilter: true
    };
    this.getAllRoles();
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }

  getAllRoles(): void {
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.getAllRoles)
      .subscribe(
        (res: any) => {
          try {
            this.roles = res.result.items;
            this.roles = this.roles.filter(x => x.name.toLowerCase() !== "patient");
            if(this.notificationId){
              this.getNotificationById();
            }
          } catch (e) {
            console.log("Success Exception AddNotificationModalComponent getAllRoles " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddNotificationModalComponent getAllRoles " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddNotificationModalComponent getAllRoles " + e);
          }
        }
      );
  }

  getNotificationById(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getNotificationsById+this.notificationId)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.selectedRoleItems = this.roles.filter(i => res.result.roleName.toString().split(',').includes(i.normalizedName));
              this.notificationBody.roleName = this.selectedRoleItems;
              this.notificationBody = res.result;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception AddNotificationModalComponent getNotificationById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error AddNotificationModalComponent getNotificationById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AddNotificationModalComponent getNotificationById " + e);
          }
        }
      );
  }

  saveNotification() {
    //console.log(this.noteBody, 'table-border');
    if (this.selectedRoleItems && this.selectedRoleItems.length > 0) {
      if (!this.isValidated()) {
      } else {
        this.stickyBarService.showLoader("");
        this.notificationBody.roleName = this.selectedRoleItems.map(ue => ue.normalizedName);
        // this.noteBody.appointmentId = this.appointmentId;
        this.apiService.postWithBearer(this.apiConfig.sendNotification, this.notificationBody)
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
                console.log("Success Exception AddNotificationModalComponent saveNotification " + e);
              }
              this.stickyBarService.hideLoader("");
            },
            (err: any) => {
              try {
                console.log("Error AddNotificationModalComponent saveNotification " + JSON.stringify(err));
                this.apiService.catchError(err);
              } catch (e) {
                console.log("Error Exception AddNotificationModalComponent saveNotification " + e);
              }
              this.stickyBarService.hideLoader("");
            }
          );
      }
    } else {
      $('.errorRoleMessage').text(ValidationMessages.roleConfirmation);
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    vO["title"] = this.validationService.getValue(this.validationService.setValue(this.notificationBody.title).required(ValidationMessages.requiredField).obj);
    vO["content"] = this.validationService.getValue(this.validationService.setValue(this.notificationBody.content).required(ValidationMessages.requiredField).obj);

    this.eR = this.validationService.findError(vO);
    if (this.eR) {
      return false;
    } else {
      return true;
    }
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  updateNotification() {
    if (this.selectedRoleItems && this.selectedRoleItems.length > 0) {
    if (!this.isValidated()) {
    } else {
      this.stickyBarService.showLoader("");
      this.notificationUpdateBody.roleName = this.selectedRoleItems.map(ue => ue.normalizedName);
      this.notificationUpdateBody.id = this.notificationId;
      this.notificationUpdateBody.title = this.notificationBody.title;
      this.notificationUpdateBody.content = this.notificationBody.content;
      console.log(this.notificationUpdateBody);
      this.apiService
        .putWithBearer(this.apiConfig.updateNotification, this.notificationUpdateBody)
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
              console.log("Success Exception AddNotificationModalComponent updateNotification " + e);
            }
            this.stickyBarService.hideLoader("");
          },
          (err: any) => {
            try {
              console.log("Error AddNotificationModalComponent updateNotification " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception AddNotificationModalComponent updateNotification " + e);
            }
            this.stickyBarService.hideLoader("");
          }
        );
    }
  } else {
    $('.errorRoleMessage').text(ValidationMessages.roleConfirmation);
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
