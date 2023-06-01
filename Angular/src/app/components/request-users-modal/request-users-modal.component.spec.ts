import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestUsersModalComponent } from './request-users-modal.component';

describe('RequestUsersModalComponent', () => {
  let component: RequestUsersModalComponent;
  let fixture: ComponentFixture<RequestUsersModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RequestUsersModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequestUsersModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
