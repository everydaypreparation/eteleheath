import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UploadPaymentDocumentModalComponent } from './upload-payment-document-modal.component';

describe('UploadPaymentDocumentModalComponent', () => {
  let component: UploadPaymentDocumentModalComponent;
  let fixture: ComponentFixture<UploadPaymentDocumentModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UploadPaymentDocumentModalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UploadPaymentDocumentModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
