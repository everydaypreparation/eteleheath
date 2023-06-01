import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RouteConfig } from 'src/app/configs/route.config';
import { StickyBarService } from 'src/app/services/sticky.bar.service';
import { ApiService } from 'src/app/services/api.service';
import { ApiConfig } from 'src/app/configs/api.config';
import { HelperService } from 'src/app/services/helper.service';
import { UserService } from 'src/app/services/user.service';
import { PropConfig } from 'src/app/configs/prop.config';

@Component({
  selector: 'app-payment-details',
  templateUrl: './payment-details.component.html',
  styleUrls: ['./payment-details.component.scss']
})
export class PaymentDetailsComponent implements OnInit {

  constructor(private router: Router,
    private routeConfig: RouteConfig,
    private activatedRoute: ActivatedRoute,
    private stickyBarService: StickyBarService,
    private apiService: ApiService,
    private apiConfig: ApiConfig,
    private userService: UserService,
    private propConfig: PropConfig,
    private helperService: HelperService) { }

  user: any;
  appointmentId: any;
  paymentId: any;
  doctorId: any;
  paymentDetails: any;
  timezones: any[] = [];
  userTimezone: any = {};
  verify: any;

  ngOnInit(): void {
    this.appointmentId = this.activatedRoute.snapshot.params['appointmentId'];
    this.paymentId = this.activatedRoute.snapshot.params['paymentId'];
    this.doctorId = this.activatedRoute.snapshot.params['doctorId'];
    this.verify = this.activatedRoute.snapshot.params['verify'];
    this.user = this.userService.getUser();
    this.getUserTimezoneOffset();
    this.getPaymentDetails();
  }

  getPaymentDetails(): void {
    const body = {
      "paymentId": this.paymentId
    }
    if (Number(this.verify) == 0) {
    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getPaymentDetailsByPaypal, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.paymentDetails = res.result;
            }
          } catch (e) {
            console.log("Success Exception PaymentDetailsComponent getPaymentDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentDetailsComponent getPaymentDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentDetailsComponent getPaymentDetails " + e);
          }
        }
      );

    }else{

    this.stickyBarService.showLoader("");
    this.apiService.postWithBearer(this.apiConfig.getPaymentDetails, body)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.paymentDetails = res.result;
            }
          } catch (e) {
            console.log("Success Exception PaymentDetailsComponent getPaymentDetails " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentDetailsComponent getPaymentDetails " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentDetailsComponent getPaymentDetails " + e);
          }
        }
      );
    }
  }

  getUserTimezoneOffset() {
    this.stickyBarService.showLoader("");
    this.apiService
      .getWithBearer(this.apiConfig.getTimeZoneMaster)
      .subscribe(
        (res: any) => {
          try {
            if (res.result.statusCode == 200) {
              this.timezones = res.result.items;
              let userTimezone = this.timezones.filter(t => t.timeZoneId==this.user.timezone)[0];
              if(!userTimezone) {
                userTimezone = this.timezones.filter(t => t.timeZoneId==this.propConfig.defaultTimezoneId)[0];
              }
              this.userTimezone.abbr = userTimezone.abbr;
              this.userTimezone.offset = userTimezone.utcOffset.substring(1, userTimezone.utcOffset.indexOf(')'))
                                                                .replace("UTC", "").replace(":", "");
            }
          } catch (e) {
            console.log("Success Exception PaymentDetailsComponent getUserTimezoneOffset " + e);
          }
          this.stickyBarService.hideLoader("");
        },
        (err: any) => {
          try {
            this.stickyBarService.hideLoader("");
            console.log("Error PaymentDetailsComponent getUserTimezoneOffset " + JSON.stringify(err));
            this.apiService.catchError(err);
          } catch (e) {
            console.log("Error Exception PaymentDetailsComponent getUserTimezoneOffset " + e);
          }
        }
      );
  }

  formatDateTimeToUTC(dateTime: any) {
    return dateTime ? dateTime.replace("Z", "") + "+00:00" : dateTime;
  }

  printPaymentDetails(): void {
    window.print();
  }

  homeClick(): void {
    this.helperService.navigateMedicalLegalUser(this.user);
  }

  paymentClick(): void {
    // this.router.navigate([
    //   this.routeConfig.medicalLegalPatientInfoFormPath, this.appointmentId, this.doctorId
    // ]);
    this.router.navigate([this.routeConfig.medicalLegalDashboardPath]);
  }
}
