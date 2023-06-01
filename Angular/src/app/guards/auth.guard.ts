import { Injectable } from "@angular/core";
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, NavigationStart, } from "@angular/router";
import { Observable, Subscriber } from "rxjs";
import { of } from "rxjs";
import { UserService } from "../services/user.service";
import { RouteConfig } from "../configs/route.config";
import { ApiConfig } from "../configs/api.config";
import { ApiService } from "../services/api.service";
import { StickyBarService } from '../services/sticky.bar.service';
import { ValidationMessages } from '../shared/validation-messages.enum';
import { EnvAndUrlService } from '../services/env-and-url.service';

@Injectable()
export class AuthGuard implements CanActivate {

  apiGetUserCallCount = 0;

  constructor(
    private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private stickyService: StickyBarService,
    private envAndUrlService: EnvAndUrlService
  ) {
    this.router.events.subscribe((res) => {
      router.events.subscribe((event: any) => {
        if (event instanceof NavigationStart) {
        }
      });
    });
  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    console.log("--- canActivate ---");
    this.userService.toggleSamvaadNavIconClick({ isShow: false });
    const roles = route.data.roles;
    console.log("roles " + roles + " " + this.userService.isUserAuthenticated());
    if (this.userService.isUserAuthenticated()) {
      if (this.userService.userRole) {
        if (this.userService.hasRole(roles)) {
          if (this.userService.userRole == "INSURANCE") {
            let currentUrl = state.url;
            return this.navigateInsurance(currentUrl);
          } else
            if (this.userService.userRole == "MEDICALLEGAL") {
              let currentUrl = state.url;
              return this.navigateMedicalLegal(currentUrl);
            } else
              if (this.userService.userRole == "PATIENT") {
                let currentUrl = state.url;
                return this.navigatePatient(currentUrl);
              }
              else
                if (this.userService.userRole == "CONSULTANT") {
                  let currentUrl = state.url;
                  return this.navigateConsultant(currentUrl);
                } else {
                  return new Observable<boolean>((observer) => {
                    this.next(observer);
                  });
                }
        } else {
          return this.moveToNotFound();
        }
      } else {
        let currentUrl = state.url;
        return new Observable<boolean>((observer) => {
          this.getUser(roles, currentUrl, observer);
        });
      }
    } else {
      if (state.url && state.url.includes('/survey-form')) {
        localStorage.setItem('survay-form', state.url);
      }
      else if (state.url && state.url.includes('/meeting/join/')) {
        localStorage.setItem('samvaadMeeting', state.url);
      }
      return this.moveToSignIn();
    }
  }

  canLoad(route: any, state: any): Observable<boolean> | Promise<boolean> | boolean {
    const roles = route.data.roles;
    if (this.userService.isUserAuthenticated()) {
      let currentUrl = "/etelehealth";
      state.forEach(u => {
        currentUrl += "/" + u.path;
      });
      if (this.userService.userRole) {
        if (this.userService.hasRole(roles)) {
          if (this.userService.userRole == "INSURANCE") {
            return this.navigateInsurance(currentUrl);
          } else
            if (this.userService.userRole == "MEDICALLEGAL") {
              return this.navigateMedicalLegal(currentUrl);
            } else
              if (this.userService.userRole == "PATIENT") {
                return this.navigatePatient(currentUrl);
              } else if (this.userService.userRole == "CONSULTANT") {
                return this.navigateConsultant(currentUrl);
              }
              else {
                return new Observable<boolean>((observer) => {
                  this.next(observer);
                });
              }
        } else {
          return this.moveToNotFound();
        }
      } else {
        return new Observable<boolean>((observer) => {
          this.getUser(roles, currentUrl, observer);
        });
      }
    } else {
      return this.moveToSignIn();
    }
  }

  getUser(roles: any, currentUrl: any, observer: Subscriber<boolean>) {
    const userId = localStorage.getItem("userId");
    if (!userId) {
      return this.moveToSignIn();
    }
    if (this.apiGetUserCallCount == 0) {
      this.apiGetUserCallCount++;
      this.apiService
        .getWithBearer(this.apiConfig.getUser + userId)
        .subscribe(
          (res: any) => {
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
              this.apiGetUserCallCount--;
              if (this.userService.hasRole(roles) && currentUrl) {
                if (this.userService.userRole == "INSURANCE") {
                  if (this.navigateInsurance(currentUrl)) {
                    this.next(observer);
                  }
                } else
                  if (this.userService.userRole == "MEDICALLEGAL") {
                    if (this.navigateMedicalLegal(currentUrl)) {
                      this.next(observer);
                    }
                  } else
                    if (this.userService.userRole == "PATIENT") {
                      if (this.navigatePatient(currentUrl)) {
                        this.next(observer);
                      }
                    }
                    else
                      if (this.userService.userRole == "CONSULTANT") {
                        if (this.navigateConsultant(currentUrl)) {
                          this.next(observer);
                        }
                      }
                      else {
                        this.next(observer);
                      }
              } else {
                return this.moveToNotFound();
              }
            } else {
              return this.moveToSignIn();
            }
          },
          (err: any) => {
            try {
              console.log("Error AuthGuard getUser " + JSON.stringify(err));
              this.apiService.catchError(err);
            } catch (e) {
              console.log("Error Exception AuthGuard getUser " + e);
            }
            return this.moveToSignIn();
          }
        );
    }
  }

  next(observer: Subscriber<boolean>): void {
    //console.log("next ---->");
    observer.next(true);
    observer.complete();
  }

  stop(observer: Subscriber<boolean>): void {
    //console.log("stop ----|");
    observer.next(false);
    observer.complete();
  }

  moveToSignIn(): Observable<boolean> | Promise<boolean> | boolean {
    //console.log("signin ----@");
    this.router.navigate([this.routeConfig.signInPath]);
    return of(false);
  }

  moveToNotFound(): Observable<boolean> | Promise<boolean> | boolean {
    //console.log("not-found ----?");
    this.router.navigate([this.routeConfig.notFoundPath]);
    return of(false);
  }

  // navigateInsurance(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
  //   let user = this.userService.getUser();
  //   if (user.name && user.surname && user.emailAddress && user.timezone) {
  //     if (currentUrl.includes(this.routeConfig.insuranceProfilePath) ||
  //       currentUrl.includes(this.routeConfig.messagingInboxPath) ||
  //       currentUrl.includes(this.routeConfig.changePasswordPath) ||
  //       currentUrl.includes(this.routeConfig.messagingComposePath) ||
  //       currentUrl.includes(this.routeConfig.messagingSentPath) ||
  //       currentUrl.includes(this.routeConfig.messagingTrashPath) ||
  //       currentUrl.includes(this.routeConfig.messagingViewMailPath) ||
  //       currentUrl.includes(this.routeConfig.notesPath) ||
  //       currentUrl.includes(this.routeConfig.viewNotesPath)) {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     }

  //     if (!user.isPayment && !user.isAppointment && !user.isIntake) {
  //       if (currentUrl.includes(this.routeConfig.insuranceEmptyDashboardPath) ||
  //         currentUrl.includes(this.routeConfig.findingConsultantPath) ||
  //         currentUrl.includes(this.routeConfig.insuranceConsultantsListPath) ||
  //         currentUrl.includes(this.routeConfig.insuranceConsultantDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.insuranceDashboardPath) ||
  //         currentUrl.includes(this.routeConfig.insurancePatientDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.samvaad) ||
  //         // if(currentUrl.includes(this.routeConfig.insurancePaymentPath)) {
  //         //   let params = currentUrl.replace(this.routeConfig.insurancePaymentPath, "").split("/");
  //         currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
  //         if (currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
  //           let params = currentUrl.replace(this.routeConfig.insurancePatientInfoFormPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //       // } else if (user.isPayment && !user.isAppointment) {
  //     } else if (user.isIntake && !user.isPayment && !user.isAppointment) {
  //       if (currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.insurancePaymentPath)
  //         ||
  //         currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)
  //       ) {
  //         if (currentUrl.includes(this.routeConfig.insurancePaymentPath)) {
  //           let params = currentUrl.replace(this.routeConfig.insurancePaymentPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //       // } else if (user.isPayment && user.isAppointment) {
  //     } else if (user.isIntake && user.isPayment && user.isAppointment) {
  //       if (currentUrl.includes(this.routeConfig.insuranceDashboardPath) ||
  //         currentUrl.includes(this.routeConfig.insurancePatientDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.findingConsultantPath) ||
  //         currentUrl.includes(this.routeConfig.insuranceConsultantsListPath) ||
  //         currentUrl.includes(this.routeConfig.samvaad) ||
  //         currentUrl.includes(this.routeConfig.insuranceConsultantDetailsPath)
  //         ||
  //         currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath)
  //         // ||
  //         // currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)
  //       ) {
  //         if (currentUrl.includes(this.routeConfig.insurancePatientDetailsPath)) {
  //           let params = currentUrl.replace(this.routeConfig.insurancePatientDetailsPath, "").split("/");
  //           let appointmentId = params[2];
  //           return new Observable<boolean>((observer) => {
  //             this.checkUpcomingAppointments(currentUrl, this.apiConfig.getAllAppointmentById + user.id, appointmentId, observer);
  //           });
  //         }
  //         else if (currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
  //           let params = currentUrl.replace(this.routeConfig.insurancePatientInfoFormPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else if (currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath)) {
  //           let params = currentUrl.replace(this.routeConfig.insurancePaymentDetailsPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[4];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         }

  //         else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //     } else {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     }
  //   } else {
  //     if (currentUrl.includes(this.routeConfig.insuranceProfilePath)) {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     }
  //     else {
  //       this.router.navigate([this.routeConfig.insuranceProfilePath]);
  //       this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
  //       this.router.navigate([""], {
  //         queryParams: { returnUrl: currentUrl },
  //       });
  //       return of(false);
  //     }
  //   }
  // }


  navigateMedicalLegal(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
    let user = this.userService.getUser();
    if (user.name && user.surname && user.emailAddress && user.timezone) {
      if (currentUrl.includes(this.routeConfig.medicalLegalProfilePath) ||
        currentUrl.includes(this.routeConfig.auditReportPath) ||
        currentUrl.includes(this.routeConfig.messagingInboxPath) ||
        currentUrl.includes(this.routeConfig.changePasswordPath) ||
        currentUrl.includes(this.routeConfig.messagingComposePath) ||
        currentUrl.includes(this.routeConfig.messagingSentPath) ||
        currentUrl.includes(this.routeConfig.messagingTrashPath) ||
        currentUrl.includes(this.routeConfig.messagingViewMailPath) ||
        currentUrl.includes(this.routeConfig.notesPath) ||
        currentUrl.includes(this.routeConfig.surveyFormPath) ||
        currentUrl.includes(this.routeConfig.viewNotesPath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }

      if (!user.isPayment && !user.isAppointment && !user.isIntake) {
        if (currentUrl.includes(this.routeConfig.medicalLegalEmptyDashboardPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantDetailsPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalDashboardPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalPatientDetailsPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
          if (currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.medicalLegalPatientInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
      } else if (user.isIntake && !user.isPayment && !user.isAppointment) {
        if (
          currentUrl.includes(this.routeConfig.medicalLegalPaymentDetailsPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalPaymentPath) ||

          currentUrl.includes(this.routeConfig.medicalLegalDashboardPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalEmptyDashboardPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantDetailsPath) ||
          
          currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)
        ) {
          if (currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
            let params = currentUrl.replace(this.routeConfig.medicalLegalPaymentPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
        // }
        // } else if (user.isPayment && user.isAppointment) {
      } else if (user.isIntake && user.isPayment && user.isAppointment) {
        if (currentUrl.includes(this.routeConfig.medicalLegalDashboardPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalPatientDetailsPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.medicalLegalConsultantDetailsPath)
          ||
          currentUrl.includes(this.routeConfig.medicalLegalPaymentDetailsPath)
          // ||
          // currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)
        ) {
          if (currentUrl.includes(this.routeConfig.medicalLegalPatientDetailsPath)) {
            let params = currentUrl.replace(this.routeConfig.medicalLegalPatientDetailsPath, "").split("/");
            let appointmentId = params[2];
            return new Observable<boolean>((observer) => {
              this.checkUpcomingAppointments(currentUrl, this.apiConfig.getAllAppointmentById + user.id, appointmentId, observer);
            });
          }
          else if (currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.medicalLegalPatientInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else if (currentUrl.includes(this.routeConfig.medicalLegalPaymentDetailsPath)) {
            let params = currentUrl.replace(this.routeConfig.medicalLegalPaymentDetailsPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[4];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          }

          else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
      } else {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
    } else {
      if (currentUrl.includes(this.routeConfig.medicalLegalProfilePath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
      else {
        this.router.navigate([this.routeConfig.medicalLegalProfilePath]);
        this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
        // this.router.navigate([""], {
        //   queryParams: { returnUrl: currentUrl },
        // });
        return of(false);
      }
    }
  }

  navigateInsurance(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
    let user = this.userService.getUser();
    if (user.name && user.surname && user.emailAddress && user.timezone) {
      if (currentUrl.includes(this.routeConfig.insuranceProfilePath) ||
        currentUrl.includes(this.routeConfig.auditReportPath) ||
        currentUrl.includes(this.routeConfig.messagingInboxPath) ||
        currentUrl.includes(this.routeConfig.changePasswordPath) ||
        currentUrl.includes(this.routeConfig.messagingComposePath) ||
        currentUrl.includes(this.routeConfig.messagingSentPath) ||
        currentUrl.includes(this.routeConfig.messagingTrashPath) ||
        currentUrl.includes(this.routeConfig.messagingViewMailPath) ||
        currentUrl.includes(this.routeConfig.notesPath) ||
        currentUrl.includes(this.routeConfig.surveyFormPath) ||
        currentUrl.includes(this.routeConfig.viewNotesPath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }

      if (!user.isPayment && !user.isAppointment && !user.isIntake) {
        if (currentUrl.includes(this.routeConfig.insuranceEmptyDashboardPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantDetailsPath) ||
          currentUrl.includes(this.routeConfig.insuranceDashboardPath) ||
          currentUrl.includes(this.routeConfig.insurancePatientDetailsPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
          if (currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.insurancePatientInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
      } else if (user.isIntake && !user.isPayment && !user.isAppointment) {
        if (currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath) ||
          currentUrl.includes(this.routeConfig.insurancePaymentPath) ||

          currentUrl.includes(this.routeConfig.insuranceDashboardPath) ||
          currentUrl.includes(this.routeConfig.insuranceEmptyDashboardPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantDetailsPath) ||

          currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)
        ) {
          if (currentUrl.includes(this.routeConfig.insurancePaymentPath)) {
            let params = currentUrl.replace(this.routeConfig.insurancePaymentPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
        //}
        // } else if (user.isPayment && user.isAppointment) {
      } else if (user.isIntake && user.isPayment && user.isAppointment) {
        if (currentUrl.includes(this.routeConfig.insuranceDashboardPath) ||
          currentUrl.includes(this.routeConfig.insurancePatientDetailsPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantsListPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.insuranceConsultantDetailsPath)
          ||
          currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath)
          // ||
          // currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)
        ) {
          if (currentUrl.includes(this.routeConfig.insurancePatientDetailsPath)) {
            let params = currentUrl.replace(this.routeConfig.insurancePatientDetailsPath, "").split("/");
            let appointmentId = params[2];
            return new Observable<boolean>((observer) => {
              this.checkUpcomingAppointments(currentUrl, this.apiConfig.getAllAppointmentById + user.id, appointmentId, observer);
            });
          }
          else if (currentUrl.includes(this.routeConfig.insurancePatientInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.insurancePatientInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else if (currentUrl.includes(this.routeConfig.insurancePaymentDetailsPath)) {
            let params = currentUrl.replace(this.routeConfig.insurancePaymentDetailsPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[4];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          }

          else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
      } else {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
    } else {
      if (currentUrl.includes(this.routeConfig.insuranceProfilePath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
      else {
        this.router.navigate([this.routeConfig.insuranceProfilePath]);
        this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
        // this.router.navigate([""], {
        //   queryParams: { returnUrl: currentUrl },
        // });
        return of(false);
      }
    }
  }


  navigatePatient(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
    let user = this.userService.getUser();
    console.log('--------------');
    console.log(user);
    if (user.name && user.surname && user.emailAddress && user.timezone) {
      if (currentUrl.includes(this.routeConfig.patientProfilePath) ||
        currentUrl.includes(this.routeConfig.auditReportPath) ||
        currentUrl.includes(this.routeConfig.changePasswordPath) ||
        currentUrl.includes(this.routeConfig.messagingInboxPath) ||
        currentUrl.includes(this.routeConfig.messagingComposePath) ||
        currentUrl.includes(this.routeConfig.messagingSentPath) ||
        currentUrl.includes(this.routeConfig.messagingTrashPath) ||
        currentUrl.includes(this.routeConfig.messagingViewMailPath) ||
        currentUrl.includes(this.routeConfig.notesPath) ||
        currentUrl.includes(this.routeConfig.surveyFormPath) ||
        currentUrl.includes(this.routeConfig.viewNotesPath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }

      if (!user.isPayment && !user.isAppointment && !user.isIntake) {
        if (
          //  currentUrl.includes(this.routeConfig.patientDashboardPath) ||
          // currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorListPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorDetailsPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.patientAddInfoFormPath)) {
          if (currentUrl.includes(this.routeConfig.patientAddInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.patientAddInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else if (currentUrl.includes(this.routeConfig.patientDashboardPath) && user.isAllowtoNewBooking && user.isMissedAppointment) {
          return new Observable<boolean>((observer) => {
            this.next(observer);
          });
        } else if (currentUrl.includes(this.routeConfig.findingConsultantPath) && user.isAllowtoNewBooking) {
          return new Observable<boolean>((observer) => {
            this.next(observer);
          });
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowedForPatient);
          this.router.navigate([this.routeConfig.findingConsultantPath], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
        // } else if (user.isPayment && !user.isAppointment) {
      } else if (user.isIntake && !user.isPayment && !user.isAppointment) {
        // if (currentUrl.includes(this.routeConfig.patientPaymentDetailsPath) ||
        //   currentUrl.includes(this.routeConfig.patientPatientInfoFormPath)) {
        //   if (currentUrl.includes(this.routeConfig.patientPatientInfoFormPath)) {
        if (currentUrl.includes(this.routeConfig.patientPaymentDetailsPath) ||
          currentUrl.includes(this.routeConfig.patientPaymentPath) ||
          currentUrl.includes(this.routeConfig.patientDashboardPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorListPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorDetailsPath) ||
          currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.patientAddInfoFormPath)) {
          if (currentUrl.includes(this.routeConfig.patientPaymentPath)) {
            let params = currentUrl.replace(this.routeConfig.patientPaymentPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
        // } else if (user.isPayment && user.isAppointment) {
      } else if (user.isIntake && user.isPayment && user.isAppointment) {
        if (currentUrl.includes(this.routeConfig.patientDashboardPath) ||
          //currentUrl.includes(this.routeConfig.findingConsultantPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorListPath) ||
          currentUrl.includes(this.routeConfig.patientDoctorDetailsPath) ||
          currentUrl.includes(this.routeConfig.patientAddInfoFormPath) ||
          currentUrl.includes(this.routeConfig.samvaad) ||
          currentUrl.includes(this.routeConfig.patientPaymentDetailsPath)) {
          if (currentUrl.includes(this.routeConfig.patientAddInfoFormPath)) {
            let params = currentUrl.replace(this.routeConfig.patientAddInfoFormPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[3];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else if (currentUrl.includes(this.routeConfig.patientPaymentDetailsPath)) {
            let params = currentUrl.replace(this.routeConfig.patientPaymentDetailsPath, "").split("/");
            let appointmentId = params[2];
            let doctorId = params[4];
            if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
              return new Observable<boolean>((observer) => {
                this.next(observer);
              });
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          }

          else {
            return new Observable<boolean>((observer) => {
              this.next(observer);
            });
          }
        } else if (currentUrl.includes(this.routeConfig.findingConsultantPath) && user.isAllowtoNewBooking) {
          return new Observable<boolean>((observer) => {
            this.next(observer);
          });
        } else {
          this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
          this.router.navigate([""], {
            queryParams: { returnUrl: currentUrl },
          });
          return of(false);
        }
      } else {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
    } else {
      if (currentUrl.includes(this.routeConfig.patientProfilePath)) {
        return new Observable<boolean>((observer) => {
          this.next(observer);
        });
      }
      else {
        this.router.navigate([this.routeConfig.patientProfilePath]);
        this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
        // this.router.navigate([""], {
        //   queryParams: { returnUrl: currentUrl },
        // });
        return of(false);
      }
    }
  }

  navigateConsultant(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
    let user = this.userService.getUser();
    // if (user.name && user.surname && user.emailAddress && user.timezone) {
    if (currentUrl.includes(this.routeConfig.consultantPatientDetailsPath)) {
      let params = currentUrl.replace(this.routeConfig.consultantPatientDetailsPath, "").split("/");
      let appointmentId = params[2];

      return new Observable<boolean>((observer) => {
        this.checkUpcomingConsultantAppointments(currentUrl, this.apiConfig.getBookLaterAppointment + user.id, appointmentId, observer);
        // this.checkUpcomingConsultantAppointments(currentUrl, this.apiConfig.getAllUpcomingAppointmentById + user.id, appointmentId, observer);
      });
    } else {
      return new Observable<boolean>((observer) => {
        this.next(observer);
      });
    }
    // } else {
    //   console.log('ppp----');
    //   if (currentUrl.includes(this.routeConfig.consultantProfilePath)) {
    //     return new Observable<boolean>((observer) => {
    //       this.next(observer);
    //     });
    //   }
    //   else {
    //     console.log('qqqq----');
    //     this.router.navigate([this.routeConfig.consultantProfilePath]);
    //     this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
    //     return of(false);
    //   }
    // }
  }

  // navigateMedicalLegal(currentUrl: string): Observable<boolean> | Promise<boolean> | boolean {
  //   let user = this.userService.getUser();
  //   if (user.name && user.surname && user.emailAddress && user.timezone) {
  //     if (currentUrl.includes(this.routeConfig.medicalLegalProfilePath) ||
  //       currentUrl.includes(this.routeConfig.messagingInboxPath) ||
  //       currentUrl.includes(this.routeConfig.messagingComposePath) ||
  //       currentUrl.includes(this.routeConfig.messagingSentPath) ||
  //       currentUrl.includes(this.routeConfig.messagingTrashPath) ||
  //       currentUrl.includes(this.routeConfig.messagingViewMailPath) ||
  //       currentUrl.includes(this.routeConfig.notesPath) ||
  //       currentUrl.includes(this.routeConfig.viewNotesPath)) {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     }

  //     // if (!user.isPayment && !user.isAppointment) {
  //     if (!user.isPayment && !user.isAppointment && !user.isIntake) {
  //       if (currentUrl.includes(this.routeConfig.medicalLegalEmptyDashboardPath) ||
  //         currentUrl.includes(this.routeConfig.findingConsultantPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalConsultantsListPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalConsultantDetailsPath) ||
  //         // currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //         // if (currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //         //   let params = currentUrl.replace(this.routeConfig.medicalLegalPaymentPath, "").split("/");
  //         currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
  //         if (currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
  //           let params = currentUrl.replace(this.routeConfig.medicalLegalPatientInfoFormPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //       // } else if (user.isPayment && !user.isAppointment) {
  //     } else if (user.isIntake && !user.isPayment && !user.isAppointment) {
  //       if (currentUrl.includes(this.routeConfig.medicalLegalPaymentDetailsPath) ||
  //         // currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
  //         // if (currentUrl.includes(this.routeConfig.medicalLegalPatientInfoFormPath)) {
  //         //   let params = currentUrl.replace(this.routeConfig.medicalLegalPatientInfoFormPath, "").split("/");
  //         currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //         if (currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //           let params = currentUrl.replace(this.routeConfig.medicalLegalPaymentPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //       // } else if (user.isPayment && user.isAppointment) {
  //     } else if (user.isIntake && user.isPayment && user.isAppointment) {
  //       if (currentUrl.includes(this.routeConfig.medicalLegalDashboardPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalPatientDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.findingConsultantPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalConsultantsListPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalConsultantDetailsPath) ||
  //         currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //         if (currentUrl.includes(this.routeConfig.medicalLegalPatientDetailsPath)) {
  //           let params = currentUrl.replace(this.routeConfig.medicalLegalPatientDetailsPath, "").split("/");
  //           let appointmentId = params[2];
  //           return new Observable<boolean>((observer) => {
  //             this.checkUpcomingAppointments(currentUrl, this.apiConfig.getNextAppointmentById + user.id, appointmentId, observer);
  //           });
  //         } else if (currentUrl.includes(this.routeConfig.medicalLegalPaymentPath)) {
  //           let params = currentUrl.replace(this.routeConfig.medicalLegalPaymentPath, "").split("/");
  //           let appointmentId = params[2];
  //           let doctorId = params[3];
  //           if (appointmentId && appointmentId == user.appoinmentId && doctorId && doctorId == user.doctorId) {
  //             return new Observable<boolean>((observer) => {
  //               this.next(observer);
  //             });
  //           } else {
  //             this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //             this.router.navigate([""], {
  //               queryParams: { returnUrl: currentUrl },
  //             });
  //             return of(false);
  //           }
  //         } else {
  //           return new Observable<boolean>((observer) => {
  //             this.next(observer);
  //           });
  //         }
  //       } else {
  //         this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
  //         this.router.navigate([""], {
  //           queryParams: { returnUrl: currentUrl },
  //         });
  //         return of(false);
  //       }
  //     } else {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     }
  //   } else {
  //     if (currentUrl.includes(this.routeConfig.medicalLegalProfilePath)) {
  //       return new Observable<boolean>((observer) => {
  //         this.next(observer);
  //       });
  //     } else {
  //       this.router.navigate([this.routeConfig.medicalLegalProfilePath]);
  //       this.stickyService.showErrorSticky(ValidationMessages.completeProfileInfo);
  //       this.router.navigate([""], {
  //         queryParams: { returnUrl: currentUrl },
  //       });
  //       return of(false);
  //     }
  //   }
  // }

  checkUpcomingAppointments(currentUrl: any, apiUrl: string, appointmentId: string, observer: Subscriber<boolean>) {
    this.apiService
      .getWithBearer(apiUrl + '&AppointmentId=' + appointmentId)
      .subscribe(
        (res: any) => {
          if (res.result.statusCode == 200) {
            let upcomingAppointments = res.result.items;
            let upcomingAppointmentIds = upcomingAppointments.map(a => a.appointmentId);
            if (upcomingAppointmentIds.indexOf(appointmentId) > -1) {
              this.next(observer);
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            this.stickyService.showErrorSticky(res.result.message);
          }
        },
        (err: any) => {
          try {
            console.log("Error AuthGuard checkUpcomingAppointments " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AuthGuard checkUpcomingAppointments " + e);
          }
          return this.stop(observer);
        }
      );
  }

  checkUpcomingConsultantAppointments(currentUrl: any, apiUrl: string, appointmentId: string, observer: Subscriber<boolean>) {
    this.apiService
      .getWithBearer(apiUrl)
      .subscribe(
        (res: any) => {
          if (res.result.statusCode == 200) {
            let upcomingAppointments = res.result.items;
            let upcomingAppointmentIds = upcomingAppointments.map(a => a.appointmentId);
            if (upcomingAppointmentIds.indexOf(appointmentId) > -1) {
              this.next(observer);
            } else {
              this.stickyService.showErrorSticky(ValidationMessages.navigationNotAllowed);
              this.router.navigate([""], {
                queryParams: { returnUrl: currentUrl },
              });
              return of(false);
            }
          } else {
            this.stickyService.showErrorSticky(res.result.message);
          }
        },
        (err: any) => {
          try {
            console.log("Error AuthGuard checkUpcomingAppointments " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception AuthGuard checkUpcomingAppointments " + e);
          }
          return this.stop(observer);
        }
      );
  }
}
