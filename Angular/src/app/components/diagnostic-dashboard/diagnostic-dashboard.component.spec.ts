import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiagnosticDashboardComponent } from './diagnostic-dashboard.component';

describe('DiagnosticDashboardComponent', () => {
  let component: DiagnosticDashboardComponent;
  let fixture: ComponentFixture<DiagnosticDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DiagnosticDashboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DiagnosticDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
