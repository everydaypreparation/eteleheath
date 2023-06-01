import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewConsultantReportModalComponent } from './view-consultant-report-modal.component';

describe('ViewConsultantReportModalComponent', () => {
  let component: ViewConsultantReportModalComponent;
  let fixture: ComponentFixture<ViewConsultantReportModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewConsultantReportModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewConsultantReportModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
