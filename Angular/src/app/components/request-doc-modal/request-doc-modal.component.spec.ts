import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestDocModalComponent } from './request-doc-modal.component';

describe('RequestDocModalComponent', () => {
  let component: RequestDocModalComponent;
  let fixture: ComponentFixture<RequestDocModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RequestDocModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequestDocModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
