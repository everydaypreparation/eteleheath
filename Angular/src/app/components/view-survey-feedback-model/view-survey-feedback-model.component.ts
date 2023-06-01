import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { ApiService } from 'src/app/services/api.service';

export interface ConfirmDialogModel {
}

@Component({
  selector: 'app-view-survey-feedback-model',
  templateUrl: './view-survey-feedback-model.component.html',
  styleUrls: ['./view-survey-feedback-model.component.scss']
})
export class ViewSurveyFeedbackModelComponent implements OnInit {

  feedback: any = null;
  questions: any = [];

  constructor(public dialogRef: MatDialogRef<ViewSurveyFeedbackModelComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel,) {

    this.feedback = data;
    this.questions = this.feedback.questions;
    this.questions.forEach((question: any, index: number) => {
      question.index = index;
      question.options = [];
      question.answers = question.response;
      if (question.questionType == "SINGLE_CHOICE" || question.questionType == "MULTI_CHOICE") {
        const responses = question.response.split(",").map(function (item: any) {
          return item.trim();
        });
        question.options = question.optionSet.split(",").map(function (item: any) {
          const title = item.trim();
          const obj: any = {};
          obj.title = title;
          obj.isChecked = false;
          if (responses.includes(title)) {
            obj.isChecked = true;
          }
          return obj;
        });
      }
    });
  }

  ngOnInit(): void {
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    this.dialogRef.close(false);
  }
}

/**
 * Class to represent confirm dialog model.
 *
 * It has been kept here to keep it as part of shared component.
 */
export class ConfirmDialogModel {
  constructor() {
  }
}
