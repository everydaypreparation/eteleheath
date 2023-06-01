import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewRequestDoctorModelComponent } from './view-request-doctor-model.component';

describe('ViewRequestDoctorModelComponent', () => {
  let component: ViewRequestDoctorModelComponent;
  let fixture: ComponentFixture<ViewRequestDoctorModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewRequestDoctorModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewRequestDoctorModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
