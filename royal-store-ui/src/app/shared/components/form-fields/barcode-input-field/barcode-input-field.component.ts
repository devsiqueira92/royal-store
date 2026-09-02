import { Component, computed, input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ProductVariantFormControls } from '@app/features/product/interfaces/product.interface';
import {
  generateEan13,
  isValidEan13,
} from '@app/features/product/utils/barcode.util';
import { InputFieldComponent } from '../input-field/input-field.component';

@Component({
  selector: 'app-barcode-input-field',
  imports: [ReactiveFormsModule, InputFieldComponent],
  templateUrl: './barcode-input-field.component.html',
  styleUrl: './barcode-input-field.component.scss',
})
export class BarcodeInputFieldComponent {
  parentForm = input.required<FormGroup<ProductVariantFormControls>>();
  controlName = input.required<string>();

  protected generateBarcode(): void {
    this.parentForm().get(this.controlName())!.setValue(generateEan13());
  }

  protected readonly barcodeValid = computed(() =>
    isValidEan13(this.parentForm().get(this.controlName())!.value),
  );
}
