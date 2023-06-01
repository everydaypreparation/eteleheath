import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SamvaadLogoutRedirectComponent } from './samvaad-logout-redirect.component';

describe('SamvaadLogoutRedirectComponent', () => {
  let component: SamvaadLogoutRedirectComponent;
  let fixture: ComponentFixture<SamvaadLogoutRedirectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SamvaadLogoutRedirectComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SamvaadLogoutRedirectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
