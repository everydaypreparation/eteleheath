import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MedicalLegalsComponent } from './medical-legals.component';

describe('MedicalLegalsComponent', () => {
  let component: MedicalLegalsComponent;
  let fixture: ComponentFixture<MedicalLegalsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MedicalLegalsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MedicalLegalsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
