import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
} from '@angular/core';
import { StickyBarService } from '../services/sticky.bar.service';


@Component({
  selector: 'app-common',
  templateUrl: './common.component.html',
  styleUrls: ['./common.component.scss']
})
export class CommonComponent implements OnDestroy {
  subscribeStickyEvent: any;
  cR: number = 0;
  constructor(private stickyBarService: StickyBarService) {
    this.subscribeStickyEvent = this.stickyBarService.stickyEvent.subscribe((data: any) => {
      if (data.type == "loadershow") {
        this.cR = this.cR + 1;
        // console.log("show " + this.cR);
        this.showLoader(data.message);
      } else if (data.type == "loaderhide") {
        this.cR = this.cR - 1;
        // console.log("hide " + this.cR);
        this.hideLoader();
      } else if (data.type == "loaderreset") {
        this.cR = 0;
        // console.log("reset " + this.cR);
        this.hideLoader();
      } else if (data.type == "successstickyshow") {
        this.showSuccessSticky(data.message);
      } else if (data.type == "successstickyhide") {
        this.hideSuccessSticky();
      } else if (data.type == "errorstickyshow") {
        this.showErrorSticky(data.message);
      } else if (data.type == "errorstickyhide") {
        this.hideErrorSticky();
      }
    });
  }

  loader = {
    state: false,
    message: ""
  };

  successSticky: any = {
    state: false,
    message: ""
  };

  errorSticky: any = {
    state: false,
    message: ""
  };

  showLoader(message: any): void {
    this.loader.state = true;
    this.loader.message = message;
  }

  hideLoader(): void {
    setTimeout(() => {
      // console.log("hideLoader " + this.cR);
      if (this.cR < 1) {
        this.cR = 0;
        this.loader.state = false;
      }
    }, 800);
  }

  showSuccessSticky(message: any): void {
    this.successSticky.state = true;
    this.successSticky.message = message;
    setTimeout(() => {
      this.hideSuccessSticky();
    }, 5000);
  }

  hideSuccessSticky(): void {
    this.successSticky.state = false;
    this.successSticky.message = "";
  }

  showErrorSticky(message: any): void {
    this.errorSticky.state = true;
    this.errorSticky.message = message;
    setTimeout(() => {
      this.hideErrorSticky();
    }, 5000);
  }

  hideErrorSticky(): void {
    this.errorSticky.state = false;
    this.errorSticky.message = "";
  }

  ngOnDestroy(): void {
    if (this.subscribeStickyEvent != null) {
      this.subscribeStickyEvent.unsubscribe();
    }
  }
}
