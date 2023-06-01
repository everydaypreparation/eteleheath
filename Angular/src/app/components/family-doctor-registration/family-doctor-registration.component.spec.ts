import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FamilyDoctorRegistrationComponent } from './family-doctor-registration.component';

describe('FamilyDoctorRegistrationComponent', () => {
  let component: FamilyDoctorRegistrationComponent;
  let fixture: ComponentFixture<FamilyDoctorRegistrationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FamilyDoctorRegistrationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FamilyDoctorRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
