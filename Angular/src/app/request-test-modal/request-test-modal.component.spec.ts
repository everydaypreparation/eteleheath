import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestTestModalComponent } from './request-test-modal.component';

describe('RequestTestModalComponent', () => {
  let component: RequestTestModalComponent;
  let fixture: ComponentFixture<RequestTestModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RequestTestModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequestTestModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
