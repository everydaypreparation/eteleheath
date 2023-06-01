import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsentFormsComponent } from './consent-forms.component';

describe('ConsentFormsComponent', () => {
  let component: ConsentFormsComponent;
  let fixture: ComponentFixture<ConsentFormsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConsentFormsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConsentFormsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
