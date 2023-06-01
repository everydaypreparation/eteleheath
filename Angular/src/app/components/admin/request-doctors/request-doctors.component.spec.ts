import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestDoctorsComponent } from './request-doctors.component';

describe('RequestDoctorsComponent', () => {
  let component: RequestDoctorsComponent;
  let fixture: ComponentFixture<RequestDoctorsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RequestDoctorsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequestDoctorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
