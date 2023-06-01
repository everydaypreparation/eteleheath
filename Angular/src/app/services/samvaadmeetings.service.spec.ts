import { TestBed } from '@angular/core/testing';

import { SamvaadmeetingsService } from './samvaadmeetings.service';

describe('SamvaadmeetingsService', () => {
  let service: SamvaadmeetingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SamvaadmeetingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
