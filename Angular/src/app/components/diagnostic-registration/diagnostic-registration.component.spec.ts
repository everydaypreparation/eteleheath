import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiagnosticRegistrationComponent } from './diagnostic-registration.component';

describe('DiagnosticRegistrationComponent', () => {
  let component: DiagnosticRegistrationComponent;
  let fixture: ComponentFixture<DiagnosticRegistrationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DiagnosticRegistrationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DiagnosticRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
