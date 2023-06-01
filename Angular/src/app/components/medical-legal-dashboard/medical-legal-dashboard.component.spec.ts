import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MedicalLegalDashboardComponent } from './medical-legal-dashboard.component';

describe('MedicalLegalDashboardComponent', () => {
  let component: MedicalLegalDashboardComponent;
  let fixture: ComponentFixture<MedicalLegalDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MedicalLegalDashboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MedicalLegalDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
