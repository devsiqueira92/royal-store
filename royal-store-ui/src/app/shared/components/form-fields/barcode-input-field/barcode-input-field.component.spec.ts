import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BarcodeInputFieldComponent } from './barcode-input-field.component';

describe('BarcodeInputFieldComponent', () => {
  let component: BarcodeInputFieldComponent;
  let fixture: ComponentFixture<BarcodeInputFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BarcodeInputFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BarcodeInputFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
