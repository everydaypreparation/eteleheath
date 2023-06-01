import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { InsuranceRoutingModule } from './insurance-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ConsultantDetailsComponent } from './consultant-details/consultant-details.component';
import { ConsultantsListComponent } from './consultants-list/consultants-list.component';
import { PatientDetailsComponent } from './patient-details/patient-details.component';
import { PatientInfoFormComponent } from './patient-info-form/patient-info-form.component';
import { PaymentComponent } from './payment/payment.component';
import { PaymentDetailsComponent } from './payment-details/payment-details.component';
import { ProfileComponent } from './profile/profile.component';
import { FormsModule } from '@angular/forms';
import { SignaturePadModule } from 'angular2-signaturepad';
import { NgxMaskModule } from 'ngx-mask';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
// import { PaymentTypeComponent } from './payment-type/payment-type.component';
import { NgxPayPalModule } from 'ngx-paypal';
import { CarouselModule } from 'ngx-owl-carousel-o';
import { DirectiveSharedModule } from 'src/app/directives/directive-shared.module';
import { DragDropModule } from '@angular/cdk/drag-drop';

@NgModule({
  declarations: [DashboardComponent, ConsultantDetailsComponent, ConsultantsListComponent, PatientDetailsComponent, PatientInfoFormComponent, PaymentComponent, PaymentDetailsComponent, ProfileComponent],
  imports: [
    CommonModule,
    CarouselModule,
    InsuranceRoutingModule,
    MatMenuModule,
    MatTabsModule,
    MatSelectModule,
    MatInputModule,
    MatRadioModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    FormsModule,
    SignaturePadModule,
    MatAutocompleteModule,
    NgxPayPalModule,
    DragDropModule,
    NgxMaskModule.forRoot(),
    DirectiveSharedModule
  ]
})
export class InsuranceModule { }
