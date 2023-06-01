import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChildNotFoundComponent } from './child-not-found.component';

describe('ChildNotFoundComponent', () => {
  let component: ChildNotFoundComponent;
  let fixture: ComponentFixture<ChildNotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ChildNotFoundComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ChildNotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
