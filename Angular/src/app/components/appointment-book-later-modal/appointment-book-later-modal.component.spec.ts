import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentBookLaterModalComponent } from './appointment-book-later-modal.component';

describe('AppointmentBookLaterModalComponent', () => {
  let component: AppointmentBookLaterModalComponent;
  let fixture: ComponentFixture<AppointmentBookLaterModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AppointmentBookLaterModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AppointmentBookLaterModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
