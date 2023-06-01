import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCostModalComponent } from './add-cost-modal.component';

describe('AddCostModalComponent', () => {
  let component: AddCostModalComponent;
  let fixture: ComponentFixture<AddCostModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddCostModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddCostModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
