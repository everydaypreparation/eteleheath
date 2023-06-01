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
import { PatientRoutingModule } from './patient-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AddInfoFormComponent } from './add-info-form/add-info-form.component';
import { DoctorsListComponent } from './doctors-list/doctors-list.component';
import { DoctorDetailsComponent } from './doctor-details/doctor-details.component';
import { PaymentComponent } from './payment/payment.component';
import { PaymentDetailsComponent } from './payment-details/payment-details.component';
import { ProfileComponent } from './profile/profile.component';
import { FormsModule } from '@angular/forms';
import { SignaturePadModule } from 'angular2-signaturepad';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { NgxMaskModule } from 'ngx-mask';
import { NgxPayPalModule } from 'ngx-paypal';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { DirectiveSharedModule } from 'src/app/directives/directive-shared.module';
import { DragDropModule } from '@angular/cdk/drag-drop';
import {MatListModule} from '@angular/material/list';
import {MatCheckboxModule} from '@angular/material/checkbox';


@NgModule({
  declarations: [DashboardComponent, AddInfoFormComponent, DoctorsListComponent, DoctorDetailsComponent, PaymentComponent, PaymentDetailsComponent, ProfileComponent],
  imports: [
    CommonModule,
    PatientRoutingModule,
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
    NgxDocViewerModule,
    DragDropModule,
    MatListModule,
    MatCheckboxModule,
    NgxMaskModule.forRoot(),
    DirectiveSharedModule
  ]
})
export class PatientModule { }
