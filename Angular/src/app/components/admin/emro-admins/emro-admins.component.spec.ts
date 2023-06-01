import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmroAdminsComponent } from './emro-admins.component';

describe('EmroAdminsComponent', () => {
  let component: EmroAdminsComponent;
  let fixture: ComponentFixture<EmroAdminsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EmroAdminsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EmroAdminsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
