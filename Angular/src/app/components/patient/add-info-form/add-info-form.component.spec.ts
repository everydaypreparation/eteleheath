import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddInfoFormComponent } from './add-info-form.component';

describe('AddInfoFormComponent', () => {
  let component: AddInfoFormComponent;
  let fixture: ComponentFixture<AddInfoFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddInfoFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddInfoFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
