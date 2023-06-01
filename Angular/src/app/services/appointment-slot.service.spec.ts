import { TestBed } from '@angular/core/testing';

import { AppointmentSlotService } from './appointment-slot.service';

describe('AppointmentSlotService', () => {
  let service: AppointmentSlotService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppointmentSlotService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
