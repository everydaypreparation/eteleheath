import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SurveyFeedbacksComponent } from './survey-feedbacks.component';

describe('SurveyFeedbacksComponent', () => {
  let component: SurveyFeedbacksComponent;
  let fixture: ComponentFixture<SurveyFeedbacksComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SurveyFeedbacksComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SurveyFeedbacksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
