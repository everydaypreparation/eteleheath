import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FindingConsultantComponent } from './finding-consultant.component';

describe('FindingConsultantComponent', () => {
  let component: FindingConsultantComponent;
  let fixture: ComponentFixture<FindingConsultantComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FindingConsultantComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FindingConsultantComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
