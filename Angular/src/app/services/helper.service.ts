import { Injectable } from "@angular/core";
import { StickyBarService } from './sticky.bar.service';
import { ApiService } from './api.service';
import { ApiConfig } from '../configs/api.config';
import { saveAs } from 'file-saver';
import { Router } from '@angular/router';
import { RouteConfig } from '../configs/route.config';
import { HttpClient } from "@angular/common/http";
import { EnvAndUrlService } from "./env-and-url.service";

@Injectable()
export class HelperService {

  constructor(
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private router: Router,
    private http: HttpClient,
    private envAndUrlService: EnvAndUrlService,
    private routeConfig: RouteConfig) {
  }

  getTimezone(date: any): any {
    const offset = new Date(date).getTimezoneOffset();
    if (!isNaN(offset)) {
      const o = Math.abs(offset);
      date = (offset < 0 ? "+" : "-") + ("00" + Math.floor(o / 60)).slice(-2) + ":" + ("00" + (o % 60)).slice(-2);
    }
    return date;
  }

  formatTime24HourTo12Hour(time: any) {

    let formatedTime = "";
    if (time) {
      let timeArr = time.split(":");
      let hr = Number(timeArr[0]);
      let min = timeArr[1];
      let isPm = false;
      let finalHr;
      if (hr == 12) {
        isPm = true;
        finalHr = hr;
      } else if (timeArr[0].trim() == '00') {
        isPm = false;
        finalHr = 12;
      } else if (hr > 12 && timeArr[0].trim() != '00') {
        isPm = true;
        finalHr = hr - 12;
      }
      let hours = '';
      let finalHours = '';
      if (hr == 0) {
        hours = '12';
      } else {
        hours = hr + '';
      }

      finalHours = finalHr + '';

      formatedTime = `${isPm ? finalHours : hours}:${min}` + `${isPm ? ' PM' : ' AM'}`;
    }

    return formatedTime;
  }

  downloadDocumentById(documentId) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.downloadDocumentById + documentId)
      .subscribe(
        (res: any) => {
          // console.log(res.result);
          try {
            if (res.result.statusCode == 200) {
              //var blob = this.convertBase64ToBlobData(res.result.filedata, res.result.mimeType);
              //saveAs(blob, res.result.documentName);
              if (res.result.filedata) {
                // console.log(res.result.filedata);
                if (res.result.filedata.includes(this.apiConfig.matchesUrl)) {
                  // saveAs(res.result.filedata);
                  this.downloadFromURLWithCustomName(res.result.filedata, res.result.documentName);
                } else {
                  var blob = this.convertBase64ToBlobData(res.result.filedata, res.result.mimeType);
                  saveAs(blob, res.result.documentName);
                }
              }
            }
          } catch (e) {
            console.log("Success Exception HelperService downloadDocumentById " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HelperService downloadDocumentById " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HelperService downloadDocumentById " + e);
          }
        }
      );
  }

  downloadReportByConsultId(consultId) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.downloadReportByConsultId + consultId)
      .subscribe(
        (res: any) => {
          console.log(res);
          try {
            if (res.result.statusCode == 200) {
              // var blob = this.convertBase64ToBlobData(res.result.report, res.result.mimeType);
              // saveAs(blob, res.result.reoportName);
              if (res.result.report) {
                if (res.result.report.includes(this.apiConfig.matchesUrl)) {
                  // saveAs(res.result.report);
                  this.downloadFromURLWithCustomName(res.result.report, res.result.reoportName);
                } else {
                  var blob = this.convertBase64ToBlobData(res.result.report, res.result.mimeType);
                  saveAs(blob, res.result.reoportName);
                }
              }
            }
          } catch (e) {
            console.log("Success Exception HelperService downloadReportByConsultId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HelperService downloadReportByConsultId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HelperService downloadReportByConsultId " + e);
          }
        }
      );
  }

  downloadSelectedConsulationReportByConsultId(consultId, docName: any) {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.downloadReportByConsultId + consultId)
      .subscribe(
        (res: any) => {
          console.log(res);
          try {
            if (res.result.statusCode == 200) {
              // var blob = this.convertBase64ToBlobData(res.result.report, res.result.mimeType);
              // saveAs(blob, res.result.reoportName);
              if (res.result.report) {
                if (res.result.report.includes(this.apiConfig.matchesUrl)) {
                  // saveAs(res.result.report);
                  this.downloadFromURLWithCustomName(res.result.report, "Consultation Report by "+docName);
                } else {
                  var blob = this.convertBase64ToBlobData(res.result.report, res.result.mimeType);
                  saveAs(blob, "Consultation Report by "+docName);
                }
              }
            }
          } catch (e) {
            console.log("Success Exception HelperService downloadReportByConsultId " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error HelperService downloadReportByConsultId " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception HelperService downloadReportByConsultId " + e);
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

  getAge(dob: any): number {
    let today = new Date();
    let birthDate = new Date(dob);
    let age = today.getFullYear() - birthDate.getFullYear();
    let m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }

  navigateInsuranceUser(user: any) {
    if (user.name && user.surname && user.emailAddress && user.timezone) {
      let appId = user.appoinmentId.includes(this.envAndUrlService.UUID);
      let docId = user.doctorId.includes(this.envAndUrlService.UUID);
      if (!user.isPayment && !user.isAppointment && !user.isIntake && appId && docId
      ) {
        this.router.navigate([this.routeConfig.insuranceEmptyDashboardPath]);
      }
      else if (!user.isPayment && !user.isAppointment && !user.isIntake && docId == false && appId == false) {
        this.router.navigate([this.routeConfig.insuranceDashboardPath]);
      }
      else if (!user.isPayment && user.isIntake && !user.isAppointment) {
        this.router.navigate([this.routeConfig.insurancePaymentPath, user.appoinmentId, user.doctorId]);
        // if (user.isBookLater) {
        //   this.router.navigate([this.routeConfig.insuranceDashboardPath]);
        // } else {
        //   this.router.navigate([this.routeConfig.insurancePaymentPath, user.appoinmentId, user.doctorId]);
        // }
      }
      else if (user.isPayment && user.isAppointment && user.isIntake) {
        this.router.navigate([this.routeConfig.insuranceDashboardPath]);
      }
    } else {
      this.router.navigate([this.routeConfig.insuranceProfilePath]);
    }
  }

  navigateMedicalLegalUser(user: any) {
    if (user.name && user.surname && user.emailAddress && user.timezone) {
      let appId = user.appoinmentId.includes(this.envAndUrlService.UUID);
      let docId = user.doctorId.includes(this.envAndUrlService.UUID);
      if (!user.isPayment && !user.isAppointment && !user.isIntake && appId && docId
      ) {
        this.router.navigate([this.routeConfig.medicalLegalEmptyDashboardPath]);
      }
      else if (!user.isPayment && !user.isAppointment && !user.isIntake && docId == false && appId == false) {
        this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
      }
      else if (!user.isPayment && user.isIntake && !user.isAppointment) {
        this.router.navigate([this.routeConfig.medicalLegalPaymentPath, user.appoinmentId, user.doctorId]);
        // if (user.isBookLater) {
        //   this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
        // } else {
        //   this.router.navigate([this.routeConfig.medicalLegalPaymentPath, user.appoinmentId, user.doctorId]);
        // }
      }
      else if (user.isPayment && user.isAppointment && user.isIntake) {
        this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
      }
    } else {
      this.router.navigate([this.routeConfig.medicalLegalProfilePath]);
    }
  }

  navigateFamilyDoctorUser(user: any) {
    if (!user.isCase) {
      this.router.navigate([this.routeConfig.familyDoctorsEmptyDashboardPath]);
    } else {
      this.router.navigate([this.routeConfig.familyDoctorDashboardPath]);
    }
  }

  navigateDiagnosticUser(user: any) {
    if (!user.isAppointment) {
      this.router.navigate([this.routeConfig.diagnosticDashboardPath]);
    } else {
      this.router.navigate([this.routeConfig.diagnosticDashboardDetailsPath]);
    }
  }

  navigatePatient(user: any) {
    // if(user.name && user.surname && user.emailAddress && user.timezone) {
    //   let appId = user.appoinmentId.includes(this.envAndUrlService.UUID);
    //   let docId = user.doctorId.includes(this.envAndUrlService.UUID);
    //   if(!user.isPayment && !user.isAppointment && !user.isIntake && appId && docId) {
    //     this.router.navigate([this.routeConfig.findingConsultantPath]);
    //   } else if(!user.isPayment && !user.isAppointment && !user.isIntake 
    //     && !appId && !docId) {
    //       this.router.navigate([this.routeConfig.findingConsultantPath]);
    //   }
    //   else if(!user.isPayment && user.isIntake && !user.isAppointment) {
    //     this.router.navigate([this.routeConfig.patientPaymentPath, user.appoinmentId, user.doctorId]);
    //   } else if(user.isPayment && user.isAppointment && user.isIntake) {
    //     this.router.navigate([this.routeConfig.findingConsultantPath]);
    //   }
    // } else {
    //   this.router.navigate([this.routeConfig.patientProfilePath]);
    // }

    if (user.name && user.surname && user.emailAddress && user.timezone) {
    
      if (!user.isPayment && user.isIntake && !user.isAppointment) {
        this.router.navigate([this.routeConfig.patientPaymentPath, user.appoinmentId, user.doctorId]);
      } else {
        if (user.isAllowtoNewBooking) {
          if(user.isMissedAppointment){
          this.router.navigate([this.routeConfig.patientDashboardPath]);
          }else{
            this.router.navigate([this.routeConfig.findingConsultantPath]);
          }
        } else {
          this.router.navigate([this.routeConfig.patientDashboardPath]);
        }
      }
    } else {
      this.router.navigate([this.routeConfig.patientDashboardPath]);
    }
  }

  downloadFromURLWithCustomName(url: string, name: string): void {
    this.stickyBarService.showLoader("");
    this.http.get(url, { responseType: 'blob', observe: 'response' })
      .subscribe((respone) => {
        const type = respone.body.type;
        const mimeTypes = {
          "application/pdf": "pdf",
          "application/vnd.ms-excel": "xls",
          "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "xlsx",
          "application/msword": "doc",
          "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "docx",
          "image/bmp": "bmp",
          "image/gif": "gif",
          "image/vnd.microsoft.icon": "ico",
          "image/jpeg": "jpeg",
          "image/jpg": "jpg",
          "image/png": "png",
          "image/svg+xml": "svg",
          "image/tiff": "tiff",
          "image/tif": "tif",
          "image/webp": "webp",
        }
        if (!name) {
          name = Math.floor(100000 + Math.random() * 900000) + ".";
          if (type) {
            const ext = mimeTypes[type];
            if (ext) {
              name += mimeTypes[type];
            } else {
              this.stickyBarService.hideLoader("");
              saveAs(url);
              return;
            }
          } else {
            this.stickyBarService.hideLoader("");
            saveAs(url);
            return;
          }
        }
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(respone.body);
        link.download = name;
        link.click();
        this.stickyBarService.hideLoader("");
      });
  }
}
