import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InsuranceRegistrationComponent } from './insurance-registration.component';

describe('InsuranceRegistrationComponent', () => {
  let component: InsuranceRegistrationComponent;
  let fixture: ComponentFixture<InsuranceRegistrationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InsuranceRegistrationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InsuranceRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
