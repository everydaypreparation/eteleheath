import {
  Component,
  Input,
  OnDestroy,
  Inject,
  ViewEncapsulation
} from "@angular/core";
import {
  Router,
  NavigationStart,
  NavigationEnd,
  NavigationCancel,
  NavigationError
} from "@angular/router";
import { DOCUMENT } from "@angular/common";

@Component({
  selector: "app-spinner",
  template: `
    <div class="preloader" *ngIf="isSpinnerVisible">
      <div class="spinner cntr-cls" style="z-index: 9999;">
        <img src="assets/images/loading.gif" class="loading-bg-cls"/>
      </div>
    </div>
  `,
  encapsulation: ViewEncapsulation.None
})
export class SpinnerComponent implements OnDestroy {
  public isSpinnerVisible = true;

  @Input() public backgroundColor = "rgba(0, 115, 170, 0.69)";

  constructor(
    private router: Router,
    @Inject(DOCUMENT) private document: Document
  ) {
    this.router.events.subscribe(
      event => {
        if (event instanceof NavigationStart) {
          this.isSpinnerVisible = true;
        } else if (
          event instanceof NavigationEnd ||
          event instanceof NavigationCancel ||
          event instanceof NavigationError
        ) {
          this.isSpinnerVisible = false;
          document.getElementById("root_loading").style.display = "none";
        }
      },
      () => {
        this.isSpinnerVisible = false;
        document.getElementById("root_loading").style.display = "none";
      }
    );
  }

  ngOnDestroy(): void {
    this.isSpinnerVisible = false;
    document.getElementById("root_loading").style.display = "none";
  }
}
