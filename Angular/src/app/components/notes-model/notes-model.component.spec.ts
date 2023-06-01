import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotesModelComponent } from './notes-model.component';

describe('NotesModelComponent', () => {
  let component: NotesModelComponent;
  let fixture: ComponentFixture<NotesModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NotesModelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NotesModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
