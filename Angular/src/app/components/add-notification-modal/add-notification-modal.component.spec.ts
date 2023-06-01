import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddNotificationModalComponent } from './add-notification-modal.component';

describe('AddNotificationModalComponent', () => {
  let component: AddNotificationModalComponent;
  let fixture: ComponentFixture<AddNotificationModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddNotificationModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddNotificationModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
