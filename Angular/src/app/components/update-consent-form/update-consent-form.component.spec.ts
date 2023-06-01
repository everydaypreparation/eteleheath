import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateConsentFormComponent } from './update-consent-form.component';

describe('UpdateConsentFormComponent', () => {
  let component: UpdateConsentFormComponent;
  let fixture: ComponentFixture<UpdateConsentFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UpdateConsentFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateConsentFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
