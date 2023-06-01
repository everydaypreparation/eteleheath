import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentSlotModelComponent } from './appointment-slot-model.component';

describe('AppointmentSlotModelComponent', () => {
  let component: AppointmentSlotModelComponent;
  let fixture: ComponentFixture<AppointmentSlotModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AppointmentSlotModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AppointmentSlotModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
