import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsultantReportComponent } from './consultant-report.component';

describe('ConsultantReportComponent', () => {
  let component: ConsultantReportComponent;
  let fixture: ComponentFixture<ConsultantReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConsultantReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConsultantReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
