import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewDocModalComponent } from './view-doc-modal.component';

describe('ViewDocModalComponent', () => {
  let component: ViewDocModalComponent;
  let fixture: ComponentFixture<ViewDocModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewDocModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewDocModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
