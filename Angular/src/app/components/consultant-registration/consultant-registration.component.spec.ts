import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsultantRegistrationComponent } from './consultant-registration.component';

describe('ConsultantRegistrationComponent', () => {
  let component: ConsultantRegistrationComponent;
  let fixture: ComponentFixture<ConsultantRegistrationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConsultantRegistrationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConsultantRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
