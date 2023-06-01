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
import { DiagnosticRoutingModule } from './diagnostic-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ProfileComponent } from './profile/profile.component';
import { FormsModule } from '@angular/forms';
import { NgxMaskModule } from 'ngx-mask';
import { PatientDetailsComponent } from './patient-details/patient-details.component';
import { CarouselModule } from 'ngx-owl-carousel-o';
import { DirectiveSharedModule } from 'src/app/directives/directive-shared.module';
import { DragDropModule } from '@angular/cdk/drag-drop';


@NgModule({
  declarations: [DashboardComponent, ProfileComponent, PatientDetailsComponent],
  imports: [
    CommonModule,
    CarouselModule,
    DiagnosticRoutingModule,
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
    DragDropModule,
    NgxMaskModule.forRoot(),
    DirectiveSharedModule
  ]
})
export class DiagnosticModule { }
