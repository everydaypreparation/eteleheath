import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FamilyDoctorsComponent } from './family-doctors.component';

describe('FamilyDoctorsComponent', () => {
  let component: FamilyDoctorsComponent;
  let fixture: ComponentFixture<FamilyDoctorsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FamilyDoctorsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FamilyDoctorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
