import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsultantsListComponent } from './consultants-list.component';

describe('ConsultantsListComponent', () => {
  let component: ConsultantsListComponent;
  let fixture: ComponentFixture<ConsultantsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConsultantsListComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConsultantsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
