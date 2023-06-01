import { Injectable, Output, EventEmitter } from "@angular/core";

@Injectable()
export class StickyBarService {

  @Output() stickyEvent: EventEmitter<any> = new EventEmitter<any>();

  constructor() { }

  showLoader(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "loadershow"
    });
  }

  hideLoader(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "loaderhide"
    });
  }

  resetLoader(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "loaderreset"
    });
  }

  showSuccessSticky(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "successstickyshow"
    });
  }

  hideSuccessSticky(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "successstickyhide"
    });
  }

  showErrorSticky(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "errorstickyshow"
    });
  }

  hideErrorSticky(message: string) {
    this.stickyEvent.emit({
      message: message,
      type: "errorstickyhide"
    });
  }
}
