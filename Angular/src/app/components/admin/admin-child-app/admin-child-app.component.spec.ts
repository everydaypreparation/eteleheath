import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminChildAppComponent } from './admin-child-app.component';

describe('AdminChildAppComponent', () => {
  let component: AdminChildAppComponent;
  let fixture: ComponentFixture<AdminChildAppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AdminChildAppComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AdminChildAppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
