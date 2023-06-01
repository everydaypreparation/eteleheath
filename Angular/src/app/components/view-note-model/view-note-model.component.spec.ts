import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewNoteModelComponent } from './view-note-model.component';

describe('ViewNoteModelComponent', () => {
  let component: ViewNoteModelComponent;
  let fixture: ComponentFixture<ViewNoteModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewNoteModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewNoteModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
