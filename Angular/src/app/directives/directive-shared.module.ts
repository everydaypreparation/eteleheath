import { NgModule } from '@angular/core';
import { DebounceClickDirective } from './debounce-click.directive';

@NgModule({
    exports: [DebounceClickDirective],
    declarations: [DebounceClickDirective]
})
export class DirectiveSharedModule { }