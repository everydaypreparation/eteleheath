import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FamilyDoctorDashboardComponent } from './family-doctor-dashboard.component';

describe('FamilyDoctorDashboardComponent', () => {
  let component: FamilyDoctorDashboardComponent;
  let fixture: ComponentFixture<FamilyDoctorDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FamilyDoctorDashboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FamilyDoctorDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
