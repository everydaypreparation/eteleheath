import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { ValidationMessages } from 'src/app/shared/validation-messages.enum';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ValidationService } from 'src/app/services/validation.service';
import { UserService } from 'src/app/services/user.service';
import { RescheduleAppointmentModalComponent } from '../../reschedule-appointment-modal/reschedule-appointment-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from 'src/app/services/helper.service';
import { IPayPalConfig, ICreateOrderRequest } from 'ngx-paypal';
import { EnvAndUrlService } from 'src/app/services/env-and-url.service';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {

  public payPalConfig?: IPayPalConfig;

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private stickyBarService: StickyBarService,
    private activatedRoute: ActivatedRoute,
    private validationService: ValidationService,
    private userService: UserService,
    private dialog: MatDialog,
    private envAndUrlService: EnvAndUrlService,
    private helperService: HelperService) { }

  user: any;
  appointmentId: any;
  doctorId: any;
  paymentId: any;
  expirationMonthYear: string = "";
  couponCodes: any[] = [];
  originalAmount = 0;
  amount = 0;
  showSuccess: any;
  showPaypalButton: any = false;
  paymentForm = {
    "nameOnCard": "",
    "cardNumber": "",
    "cvc": "",
    "expirationMonth": 0,
    "expirationYear": 0,
    "cardType": "",
    "originalPayAmount": this.originalAmount,
    "payAmount": this.amount,
    "couponId": "",
    "appointmentId": "",
    "userId": "",
    "description": "",
    "isPatient": false
  }

  paymentPaypalForm = {
    "payeeEmailAddress": "",
    "payeeMerchantId": "",
    "payerFullName": "",
    "payerEmailAddress": "",
    "payerId": "",
    "originalPayAmount": "",
    "paymentCurrencyCode": "",
    "paymentCreateTime": "",
    "paymentUpdateTime": "",
    "paymentOrderId": "",
    "paymentStatus": "",
    "payerAddressLine": "",
    "payerAdminArea2": "",
    "payerAdminArea1": "",
    "payerPostalCode": "",
    "payerCountryCode": "",
    "couponId": "",
    "appointmentId": "",
    "userId": "",
    "isPatient": false
  };

  paymentFormWallet = {
    "originalPayAmount": 0,
    "payAmount": 0,
    "couponId": "",
    "appointmentId": "",
    "userId": "",
    "description": "",
    "isPatient": false
  };


  eR: any = null;
  currentMonth = new Date().getMonth() + 1;
  currentYear = new Date().getFullYear();
  remainingBalance: any = 0;
  applicableDiscount: any = 0;
  paymentMessage: any;

  ngOnInit(): void {
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
    this.doctorId = this.activatedRoute.snapshot.params['doctorId'];
    this.user = this.userService.getUser();
    this.getCouponCode();
    if(this.user.isBookLater == false){
    this.checkSlotAvailability(false);
    }
    this.initConfig();
    //this.getAllWalletCost();
    this.getAllCost();
  }

  showPaypalBtn() {
    if (Number(this.amount) > 0) {
      if(this.user.isBookLater == false){
      this.checkSlotAvailabilityForPaypal(true);
      }else{
        this.showPaypalButton = true;
      }
    }
  }

  payFromWallet() {
    if(this.user.isBookLater == false){
    this.checkSlotAvailabilityForWallet(true);
    }else{
    this.saveWalletPaymentDetails();
    }
  }

  getCouponCode(): any {
    this.stickyBarService.showLoader("");
    this.apiService.getWithBearer(this.apiConfig.getCouponCodes)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.couponCodes = res.result.items;
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent getCouponCode " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent getCouponCode " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent getCouponCode " + e);
          }
        }
      );
  }

  payClick(): void {
    if (this.isValidated()) {
      if(this.user.isBookLater == false){
      this.checkSlotAvailability(true);
      }else{
      if(this.paymentForm.couponId) {
        this.validateCoupon(true);
      } else {
        this.payAmount();
      }}
    }
  }

  isValidated(): boolean {
    const vO: any = {};
    this.paymentForm.nameOnCard = this.paymentForm.nameOnCard.trim();
    let message = this.checkCouponCodeValidity();
    if (message) {
      this.eR = {};
      this.eR.key = 'couponId';
      this.eR.message = message;
    }
    if (!this.eR) {
      vO["nameOnCard"] = this.validationService.getValue(this.validationService.setValue(this.paymentForm.nameOnCard).required(ValidationMessages.enterNameOnCard).alphabets(ValidationMessages.nameOnCardNotString).rangeLength([2, 80], ValidationMessages.nameOnCardLength).obj);
      vO["cardNumber"] = this.validationService.getValue(this.validationService.setValue(this.paymentForm.cardNumber).required(ValidationMessages.enterCardNumber).minLength(13, ValidationMessages.enterValidCardNumber).obj);
      vO["cvv"] = this.validationService.getValue(this.validationService.setValue(this.paymentForm.cvc).required(ValidationMessages.enterCVV).minLength(3, ValidationMessages.enterValidCVV).obj);
      vO["expirationMonthYear"] = this.validationService.getValue(this.validationService.setValue(this.expirationMonthYear).required(ValidationMessages.enterCardExpirationDetails).minLength(6, ValidationMessages.enterCardValidExpirationDetails).regex("^(0?[1-9]|1[012])\\d{4}$", ValidationMessages.enterCardValidExpirationDetails).obj);
      this.eR = this.validationService.findError(vO);
      if (!this.eR && this.expirationMonthYear) {
        let expirationMonth = Number(this.expirationMonthYear.substr(0, 2));
        let expirationYear = Number(this.expirationMonthYear.substr(2, 4));
        if (expirationYear < this.currentYear || (expirationMonth < this.currentMonth && expirationYear <= this.currentYear)) {
          this.eR = {};
          this.eR.key = 'expirationMonthYear';
          this.eR.message = ValidationMessages.enterCardValidExpirationDetails;
        }
      }
    }
    if (this.eR) {
      return false;
    } else {
      this.paymentForm.appointmentId = this.appointmentId;
      this.paymentForm.userId = this.user.id;
      this.paymentForm.expirationMonth = Number(this.expirationMonthYear.substr(0, 2));
      this.paymentForm.expirationYear = Number(this.expirationMonthYear.substr(2, 4));
      return true;
    }
  }

  checkCouponCodeValidity(): string {
    if (this.paymentForm.couponId) {
      if (this.couponCodes && this.couponCodes.length > 0) {
        let matchedCode = this.couponCodes.filter(c => c.discountCode == this.paymentForm.couponId);
        if (matchedCode && matchedCode.length > 0) {
          let couponExpiryDate = matchedCode[0].couponExpire;
          if (new Date(this.formatDateTimeToUTC(couponExpiryDate)).getTime() > new Date().getTime()) {
            return null;
          }
          return ValidationMessages.couponCodeExpired;
        }
      }
      return ValidationMessages.enterValidCouponCode;
    }
    return null;
  }

  couponCodeFocusOut() {
    if (!this.paymentForm.couponId) {
      this.amount = this.originalAmount;
    }

    if (this.user.depositAmount && Number(this.user.depositAmount) >= Number(this.amount) && Number(this.amount) > 0) {
      this.remainingBalance = Number(this.user.depositAmount) - Number(this.amount);
      if (this.remainingBalance < 0) {
        this.remainingBalance = 0;
      }
      //this.applicableDiscount = 0;
    }
  }

  validateCoupon(isSubmit: boolean): void {
    let message = "";
    if (this.paymentForm.couponId) {
      message = this.checkCouponCodeValidity();
    } else {
      message = "";
      return;
    }
    if (message) {
      this.eR = {};
      this.eR.key = 'couponId';
      this.eR.message = message;
      return;
    }
    const body = {
      "coupon": this.paymentForm.couponId,
      "originalPayAmount": this.originalAmount,
      "userId": this.user.id
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.validateCoupon, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.amount = res.result.amount;
              if (res.result.is100PercentAmount) {
                if(this.user.isBookLater == false){
                this.checkSlotAvailabilityForAppointmentBooked(true);
                }else{
                this.appoinmentBooked();
                }
              }
              if (this.user.depositAmount && Number(this.user.depositAmount) >= Number(this.amount) && Number(this.amount) > 0) {
                this.remainingBalance = Number(this.user.depositAmount) - Number(this.amount);
                if (this.remainingBalance < 0) {
                  this.remainingBalance = 0;
                }
              }
              this.applicableDiscount = Number(res.result.discountAmount);
              this.stickyBarService.showSuccessSticky(this.paymentForm.couponId + ValidationMessages.coupanMessage);
              if (isSubmit) {
                this.payAmount();
              }
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent validateCoupon " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent validateCoupon " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent validateCoupon " + e);
          }
        }
      );
  }

  payAmount(): void {
    this.paymentForm.payAmount = this.amount;
    this.paymentForm.originalPayAmount = this.originalAmount;
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.appointmentPayment, this.paymentForm)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.paymentId = res.result.paymentId;
              this.getUser(1);
              this.clearFields();
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent payAmount " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent payAmount " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent payAmount " + e);
          }
        }
      );
  }

  getUser(verify: any): void {
    const userId = localStorage.getItem("userId");
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getUser + userId)
      .subscribe(
        (res: any) => {
          try {
            this.stickyBarService.hideLoader("");
            if (res.result.statusCode == 200) {
              this.userService.setUser(res.result);
              this.router.navigate([
                this.routeConfig.medicalLegalPaymentDetailsPath, this.appointmentId, this.paymentId, this.doctorId, verify
              ]);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent getUser " + e);
          }
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent getUser " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent getUser " + e);
          }
        }
      );
  }

  checkSlotAvailability(isSubmit: boolean): void {
    let body = {
      "AppointmentId": this.appointmentId
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.IsSlotAvailable, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!res.result.isAvailable) {
                this.showAvailableSlotsToReschedule(isSubmit);
              } else {
                if (isSubmit) {
                  if (this.paymentForm.couponId) {
                    this.validateCoupon(true);
                  } else {
                    this.payAmount();
                  }
                }
              }
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent checkSlotAvailability " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent checkSlotAvailability " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent checkSlotAvailability " + e);
          }
        }
      );
  }

  checkSlotAvailabilityForPaypal(isSubmit: boolean): void {
    let body = {
      "AppointmentId": this.appointmentId
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.IsSlotAvailable, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!res.result.isAvailable) {
                this.showAvailableSlotsToRescheduleForPaypal(isSubmit);
              } else {
                this.showPaypalButton = true;
              }
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent checkSlotAvailabilityForPaypal " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent checkSlotAvailabilityForPaypal " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent checkSlotAvailabilityForPaypal " + e);
          }
        }
      );
  }

  checkSlotAvailabilityForWallet(isSubmit: boolean): void {
    let body = {
      "AppointmentId": this.appointmentId
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.IsSlotAvailable, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!res.result.isAvailable) {
                this.showAvailableSlotsToRescheduleForWallet(isSubmit);
              } else {
                this.saveWalletPaymentDetails();
              }
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent checkSlotAvailabilityForPaypal " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent checkSlotAvailabilityForPaypal " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent checkSlotAvailabilityForPaypal " + e);
          }
        }
      );
  }

  showAvailableSlotsToReschedule(isSubmit: boolean) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": this.appointmentId, "doctorId": this.doctorId, "slotNotAvailableMessage": ValidationMessages.slotNotAvailable }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult) {
        if (isSubmit) {
          if (this.paymentForm.couponId) {
            this.validateCoupon(true);
          } else {
            this.payAmount();
          }
        }
      }
    });
  }

  showAvailableSlotsToRescheduleForPaypal(isSubmit: boolean) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": this.appointmentId, "doctorId": this.doctorId, "slotNotAvailableMessage": ValidationMessages.slotNotAvailable }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult) {
        this.showPaypalButton = true;
      } else {
        this.showPaypalButton = false;
      }
    });
  }

  showAvailableSlotsToRescheduleForWallet(isSubmit: boolean) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": this.appointmentId, "doctorId": this.doctorId, "slotNotAvailableMessage": ValidationMessages.slotNotAvailable }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult) {
        this.saveWalletPaymentDetails();
      }
    });
  }

  showAvailableSlotsToRescheduleForAppointmentBooked(isSubmit: boolean) {
    const dialogRef = this.dialog.open(RescheduleAppointmentModalComponent, {
      maxWidth: "800px",
      data: { "appointmentId": this.appointmentId, "doctorId": this.doctorId, "slotNotAvailableMessage": ValidationMessages.slotNotAvailable }
    });

    dialogRef.afterClosed().subscribe(dialogResult => {
      if (dialogResult) {
        this.appoinmentBooked();
      }
    });
  }

  validatorPress(e: any, type: string): void {
    if (this.eR && this.eR.key == type) {
      this.eR = null;
    }
  }

  clearFields(): void {
    for (let key of Object.keys(this.paymentForm)) {
      this.paymentForm[key] = "";
    }
    this.paymentForm.expirationMonth = 0;
    this.paymentForm.expirationYear = 0;
    this.paymentForm.originalPayAmount = this.originalAmount;
    this.paymentForm.payAmount = this.amount;
    this.paymentForm.appointmentId = this.appointmentId;
    this.paymentForm.userId = this.user.id;
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  homeClick(): void {
    //this.helperService.navigateMedicalLegalUser(this.user);
    //this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
    if (!this.user.isCase) {
      this.router.navigate([this.routeConfig.medicalLegalEmptyDashboardPath]);
    } else {
      this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
    }
  }

  tabClick(tab) {
    if (tab.index == 1 || tab.index == 2) {
      //this.getAllCost();
    } else if (tab.index == 0) {
      //this.getAllWalletCost();
    }

    if (tab.index == 2) {
      if (Number(this.amount) > 0) {
        if(this.user.isBookLater == false){
        this.checkSlotAvailabilityForPaypal(true);
        }else{
          this.showPaypalButton = true;
        }
      }
      this.initConfig();
    }
    // if (tab.index == 1) {
    //   this.getAllCost();
    //   this.initConfig();
    // } else if (tab.index == 2) {
    //   this.getAllCost();
    // } else if (tab.index == 0) {
    //   this.getAllWalletCost();
    // }
  }

  initConfig(): void {
    this.payPalConfig = {
      currency: 'USD',
      clientId: this.envAndUrlService.SELLER_ID,
      createOrderOnClient: (data) => <ICreateOrderRequest>{
        intent: 'CAPTURE',
        purchase_units: [
          {
            amount: {
              currency_code: 'USD',
              value: this.amount + ''
            }
          }
        ]
      },
      advanced: {
        commit: 'true'
      },
      style: {
        label: 'paypal',
        layout: 'vertical'
      },
      onApprove: (data, actions) => {
        //console.log('onApprove - transaction was approved, but not authorized', data, actions);
        actions.order.get().then(details => {
          //console.log('onApprove - you can get full order details inside onApprove: ', details);
        });
      },
      onClientAuthorization: (data) => {
        console.log('onClientAuthorization - you should probably inform your server about completed transaction at this point');
        this.showSuccess = true;
        this.stickyBarService.showLoader("");
        if (data) {
          this.savePaymentDetails(data);
        }
      },
      onCancel: (data, actions) => {
        this.stickyBarService.hideLoader("");
       // console.log('OnCancel', data, actions);
      },
      onError: err => {
        this.stickyBarService.hideLoader("");
        //console.log('OnError', err);
      },
      onClick: (data, actions) => {
        //this.stickyBarService.showLoader("");
        //console.log('onClick', data, actions);
      },
    };
  }


  savePaymentDetails(data): void {
    this.paymentPaypalForm.payeeEmailAddress = this.envAndUrlService.SELLER_ID;
    this.paymentPaypalForm.payeeMerchantId = data.purchase_units[0].payee.merchant_id;
    this.paymentPaypalForm.payerFullName = data.purchase_units[0].shipping.name.full_name;
    this.paymentPaypalForm.payerEmailAddress = data.payer.email_address;
    this.paymentPaypalForm.payerId = data.payer.payer_id;
    this.paymentPaypalForm.originalPayAmount = data.purchase_units[0].amount.value;
    this.paymentPaypalForm.paymentCurrencyCode = data.purchase_units[0].amount.currency_code;
    this.paymentPaypalForm.paymentCreateTime = data.create_time;
    this.paymentPaypalForm.paymentUpdateTime = data.update_time;
    this.paymentPaypalForm.paymentOrderId = data.id;
    this.paymentPaypalForm.paymentStatus = data.status;
    this.paymentPaypalForm.payerAddressLine = data.purchase_units[0].shipping.address.address_line_1;
    this.paymentPaypalForm.payerAdminArea2 = data.purchase_units[0].shipping.address.admin_area_2;
    this.paymentPaypalForm.payerAdminArea1 = data.purchase_units[0].shipping.address.admin_area_1;
    this.paymentPaypalForm.payerPostalCode = data.purchase_units[0].shipping.address.postal_code;
    this.paymentPaypalForm.payerCountryCode = data.purchase_units[0].shipping.address.country_code;
    this.paymentPaypalForm.couponId = "";
    this.paymentPaypalForm.appointmentId = this.appointmentId;
    this.paymentPaypalForm.userId = this.user.id;
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.paymentByPaypal, this.paymentPaypalForm)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.paymentId = res.result.paymentId;
              this.getUser(0);
            } else {
              // this.paymentId = res.result.paymentId;
              // this.getUser(0);
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent savePaymentDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent savePaymentDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent savePaymentDetails " + e);
          }
        }
      );
  }

  getAllWalletCost(): void {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getAllCost)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let baseRate = Number(res.result.items.filter(ue => ue != null && ue.keyName == 'Base Rate')[0].value);
              let uptoPages = Number(res.result.items.filter(ue1 => ue1 != null && ue1.keyName == 'Upto Pages')[0].value);
              let perPageRate = Number(res.result.items.filter(ue2 => ue2 != null && ue2.keyName == 'Per Page Rate')[0].value);
              let pages = Number(this.user.numberofPages);
              if (this.user.depositAmount && Number(this.user.depositAmount) > 0) {
                if (res.result.items[0] && baseRate > 0 && perPageRate > 0 && uptoPages > 0) {
                  if (pages <= uptoPages) {
                    this.originalAmount = baseRate;
                    this.amount = baseRate;
                  } else {
                    let remainingPageCount = pages - uptoPages;
                    let amt = remainingPageCount * perPageRate;
                    let finalAmt = amt + baseRate;
                    this.originalAmount = finalAmt;
                    this.amount = finalAmt;
                  }
                  this.remainingBalance = Number(this.user.depositAmount) - Number(this.amount);
                  if (this.remainingBalance < 0) {
                    this.remainingBalance = 0;
                  }
                } else {
                  this.originalAmount = 0;
                  this.amount = 0;
                  this.applicableDiscount = 0;
                  this.remainingBalance = 0;
                }
              } else {
                if (res.result.items[0] && baseRate > 0 && perPageRate > 0 && uptoPages > 0) {
                if (pages <= uptoPages) {
                  this.amount = baseRate;
                } else {
                  let remainingPageCount = pages - uptoPages;
                  let amt = remainingPageCount * perPageRate;
                  let finalAmt = amt + baseRate;
                  this.amount = finalAmt;
                }
                this.originalAmount = 0;
                // this.amount = 20;
                this.applicableDiscount = 0;
                this.remainingBalance = 0;
              }else{
                this.originalAmount = 0;
                this.amount = 0;
                this.applicableDiscount = 0;
                this.remainingBalance = 0;
              }
              }
            } else {
              this.originalAmount = 0;
              this.amount = 0;
              this.applicableDiscount = 0;
              this.remainingBalance = 0;
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent getAllCost " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent getAllCost " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent getAllCost " + e);
          }
        }
      );
  }



  getAllCost(): void {
    this.stickyBarService.showLoader("");
    //this.paymentForm.couponId = "";
    this.apiService
      .getWithBearer(this.apiConfig.getAllCost)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              let baseRate = Number(res.result.items.filter(ue => ue != null && ue.keyName == 'Base Rate')[0].value);
              let uptoPages = Number(res.result.items.filter(ue1 => ue1 != null && ue1.keyName == 'Upto Pages')[0].value);
              let perPageRate = Number(res.result.items.filter(ue2 => ue2 != null && ue2.keyName == 'Per Page Rate')[0].value);
              let pages = Number(this.user.numberofPages);
              if (res.result.items[0] && baseRate > 0 && perPageRate > 0 && uptoPages > 0) {
                if (pages <= uptoPages) {
                  this.originalAmount = baseRate;
                  this.amount = baseRate;
                } else {
                  let remainingPageCount = pages - uptoPages;
                  let amt = remainingPageCount * perPageRate;
                  let finalAmt = amt + baseRate;
                  this.originalAmount = finalAmt;
                  this.amount = finalAmt;
                }
                this.remainingBalance = Number(this.user.depositAmount) - Number(this.amount);
              } else {
                this.originalAmount = 0;
                this.amount = 0;
              }
            } else {
              this.originalAmount = 0;
              this.amount = 0;
              this.stickyBarService.showErrorSticky(res.result.message);
            }
            this.paymentMessage = ValidationMessages.paymentMessage;
          } catch (e) {
            console.log("Success Exception PaymentComponent getAllCost " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent getAllCost " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent getAllCost " + e);
          }
        }
      );
  }

  saveWalletPaymentDetails(): void {
    this.paymentFormWallet.userId = this.user.id;
    this.paymentFormWallet.payAmount = this.amount;
    this.paymentFormWallet.originalPayAmount = this.originalAmount;
    this.paymentFormWallet.couponId = this.paymentForm.couponId;
    this.paymentFormWallet.appointmentId = this.appointmentId;
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.payByDepositeAmount, this.paymentFormWallet)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.paymentId = res.result.paymentId;
              this.getUser(1);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent saveWalletPaymentDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent saveWalletPaymentDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent saveWalletPaymentDetails " + e);
          }
        }
      );
  }

  appoinmentBooked(): void {
    this.paymentMessage = "";
    this.paymentFormWallet.userId = this.user.id;
    this.paymentFormWallet.payAmount = this.amount;
    this.paymentFormWallet.originalPayAmount = this.originalAmount;
    this.paymentFormWallet.couponId = this.paymentForm.couponId;
    this.paymentFormWallet.appointmentId = this.appointmentId;
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.payByDepositeAmount, this.paymentFormWallet)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.stickyBarService.showSuccessSticky(res.result.message);
              this.paymentId = res.result.paymentId;
              this.getUser(1);
            } else {
              this.stickyBarService.showErrorSticky(res.result.message);
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent appoinmentBooked " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent appoinmentBooked " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent appoinmentBooked " + e);
          }
        }
      );
  }

  checkSlotAvailabilityForAppointmentBooked(isSubmit: boolean): void {
    let body = {
      "AppointmentId": this.appointmentId
    }
    this.stickyBarService.showLoader("");
    this.apiService
      .postWithBearer(this.apiConfig.IsSlotAvailable, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              if (!res.result.isAvailable) {
                this.showAvailableSlotsToRescheduleForAppointmentBooked(isSubmit);
              } else {
                this.appoinmentBooked();
              }
            }
          } catch (e) {
            console.log("Success Exception PaymentComponent checkSlotAvailabilityForAppointmentBooked " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentComponent checkSlotAvailabilityForAppointmentBooked " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentComponent checkSlotAvailabilityForAppointmentBooked " + e);
          }
        }
      );
  }

  moveIntackForm(){
    this.router.navigate([this.routeConfig.medicalLegalPatientInfoFormPath, this.appointmentId, this.doctorId]);
  }
}