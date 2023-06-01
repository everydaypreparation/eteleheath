import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MedicalLegalRegistrationComponent } from './medical-legal-registration.component';

describe('MedicalLegalRegistrationComponent', () => {
  let component: MedicalLegalRegistrationComponent;
  let fixture: ComponentFixture<MedicalLegalRegistrationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MedicalLegalRegistrationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MedicalLegalRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
