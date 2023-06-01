import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewSurveyFeedbackModelComponent } from './view-survey-feedback-model.component';

describe('ViewSurveyFeedbackModelComponent', () => {
  let component: ViewSurveyFeedbackModelComponent;
  let fixture: ComponentFixture<ViewSurveyFeedbackModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewSurveyFeedbackModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewSurveyFeedbackModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
