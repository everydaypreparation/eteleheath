import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from 'src/app/services/user.service';
import { RouteConfig } from 'src/app/configs/route.config';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-survey-form',
  templateUrl: './survey-form.component.html',
  styleUrls: ['./survey-form.component.scss']
})
export class SurveyFormComponent implements OnInit {

  user: any = null;
  appointmentId: any = null;
  showContent: boolean = false;
  formBody: any = [];
  optionalQuestion: any = null;
  showThanks: boolean = false;
  errObj: any = {
    isError: false,
    errMsg: "Please answer the question(s)"
  }
  questions: any = [];

  constructor(private apiService: ApiService,
    private router: Router,
    private routeConfig: RouteConfig,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private helperService: HelperService,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,) { }


  ngOnInit(): void {
    this.user = this.userService.getUser();
    this.stickyBarService.hideLoader("");
    localStorage.removeItem("survay-form");
    this.appointmentId = this.activatedRoute.snapshot.params['id'];
    if (this.appointmentId) {
      this.getUserIdByAppointmentId();
    }
  }

  getUserIdByAppointmentId(): void {
    const body = {
      "AppointmentId": this.appointmentId
    };
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.surveyAuthenticateUser, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (res.result.userId == this.user.id && !res.result.isSurveySubmitted) {
                this.getSurveyForm();
              } else {
                this.router.navigate([
                  this.routeConfig.notFoundPath
                ]);
              }
            } else {
              this.router.navigate([
                this.routeConfig.notFoundPath
              ]);
            }
          } catch (e) {
            console.log("Success Exception SurveyFormComponent getUserIdByAppointment " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SurveyFormComponent getUserIdByAppointment " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SurveyFormComponent getUserIdByAppointment " + e);
          }
        }
      );
  }

  getSurveyForm(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getSurveyForm)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.questions = res.result.items;
              this.questions.forEach((question: any, index: number) => {
                question.index = index;
                question.options = [];
                question.answers = "";
                if (question.questionType == "SINGLE_CHOICE" || question.questionType == "MULTI_CHOICE") {
                  question.options = question.optionSet.split(",").map(function (item: any) {
                    return item.trim();
                  });
                }
              });
              this.optionalQuestion = this.questions.splice(1, 1)[0];
              this.showContent = true;
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception SurveyFormComponent getSurveyForm " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error SurveyFormComponent getSurveyForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SurveyFormComponent getSurveyForm " + e);
          }
        }
      );
  }

  onChanged(e: any, obj: any, answer: string): void {
    this.validatorPress(e, obj, answer);
    if (obj.questionType == "SINGLE_CHOICE") {
      obj.answers = answer;
      if (obj.index == 0) {
        if (answer == "Poor" || answer == "Satisfactory" || answer == "Good") {
          if (!this.questions.find((item: any) => item.index == 1)) {
            /*add*/
            this.questions.splice(1, 0, this.optionalQuestion);
          }
        } else {
          if (this.questions.find((item: any) => item.index == 1)) {
            /*remove*/
            this.optionalQuestion = this.questions.splice(1, 1)[0];
            this.optionalQuestion.answers = "";
          }
        }
      }
    } else if (obj.questionType == "MULTI_CHOICE") {
      if (e.target.checked) {
        if (obj.answers) {
          obj.answers.push(answer)
        } else {
          obj.answers = [answer];
        }
      } else {
        obj.answers = obj.answers.filter((item: any) => item !== answer);
      }
    }
  }

  isValidated(): boolean {
    return !Object.values(this.formBody).every((item: any) => item.response == "");
  }

  submitClk(): void {
    this.formBody = [];
    if (!this.questions.find((item: any) => item.index == 1)) {
      const obj: any = {};
      obj.questionId = this.optionalQuestion.id;
      obj.response = String(this.optionalQuestion.answers);
      obj.appointmentId = this.appointmentId;
      obj.caseId = "";
      this.formBody.push(obj);
    }
    this.questions.forEach((question: any, index: number) => {
      const obj: any = {};
      obj.questionId = question.id;
      obj.response = String(question.answers);
      obj.appointmentId = this.appointmentId;
      obj.caseId = "";
      this.formBody.push(obj);
    });
    if (this.isValidated()) {
      this.submitSurveyForm();
    } else {
      this.errObj.isError = true;
    }
  }

  submitSurveyForm(): void {
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.submitSurveyForm, this.formBody)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.showThanks = true;
              setTimeout(() => {
                if (this.userService.userRole) {
                  const role = this.userService.userRole;
                  if (role == "PATIENT") {
                    this.helperService.navigatePatient(this.user);
                  } else if (role == "INSURANCE") {
                    this.helperService.navigateInsuranceUser(this.user);
                  } else if (role == "MEDICALLEGAL") {
                    this.helperService.navigateMedicalLegalUser(this.user);
                  }
                }
              }, 1000);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception SurveyFormComponent submitSurveyForm " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            console.log("Error SurveyFormComponent submitSurveyForm " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception SurveyFormComponent submitSurveyForm " + e);
          }
          this.stickyBarService.hideLoader("");
        }
      );
  }

  validatorPress(e: any, obj: any, answer: string): void {
    this.errObj.isError = false;
  }
}
