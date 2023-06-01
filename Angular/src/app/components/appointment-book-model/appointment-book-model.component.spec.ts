import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentBookModelComponent } from './appointment-book-model.component';

describe('AppointmentBookModelComponent', () => {
  let component: AppointmentBookModelComponent;
  let fixture: ComponentFixture<AppointmentBookModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AppointmentBookModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AppointmentBookModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
