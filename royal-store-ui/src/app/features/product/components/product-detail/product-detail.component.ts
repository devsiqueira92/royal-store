import { Component, computed, inject, input, signal } from '@angular/core';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { TextareaFieldComponent } from '@app/shared/components/form-fields/textarea-field/textarea-field.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { ImageUploadFieldComponent } from '@app/shared/components/form-fields/image-upload-field/image-upload-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Options } from '@app/shared/interceptors/options.interface';
import {
  ItemType,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { CategorySelectFieldComponent } from '@app/shared/components/form-fields/category-select-field/category-select-field.component';

const UNIT_OF_MEASURE_OPTIONS: Options[] = [
  { code: 'UN', description: 'UN — Unit' },
  { code: 'PC', description: 'PC — Piece' },
  { code: 'PAR', description: 'PAR — Pair' },
  { code: 'CX', description: 'CX — Box' },
  { code: 'KG', description: 'KG — Kilogram' },
];

@Component({
  selector: 'app-product-detail',
  imports: [
    InputNumberFieldComponent,
    TextareaFieldComponent,
    InputFieldComponent,
    FormLayoutComponent,
    SelectFieldComponent,
    ImageUploadFieldComponent,
    ToggleFieldComponent,
    ReactiveFormsModule,
    TranslatePipe,
    CategorySelectFieldComponent,
  ],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
})
export class ProductDetailComponent {
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();
  itemType = input.required<number>();

  private readonly skuLockedGroups = new Set<
    FormGroup<ProductVariantFormControls>
  >();

  protected readonly unitOfMeasureOptions = UNIT_OF_MEASURE_OPTIONS;
  protected readonly photoFiles = signal<File[]>([]);

  protected onPhotoFilesChange(files: File[]): void {
    this.photoFiles.set(files);
  }

  protected onReferenceCodeBlur(): void {
    const referenceCode = this.formGroup().controls.sku.value ?? '';
    this.variants().controls.forEach((group, index) => {
      if (this.skuLockedGroups.has(group)) {
        return;
      }
      group.controls.sku.setValue(this.computeSku(referenceCode, index));
    });
  }

  protected onBasePriceBlur(): void {
    const basePrice = this.formGroup().controls.basePrice.value;
    this.variants().controls.forEach(group => {
      group.controls.price.setValue(basePrice);
    });
  }

  protected onBaseCostPriceBlur(): void {
    const baseCostPrice = this.formGroup().controls.baseCostPrice.value;
    this.variants().controls.forEach(group => {
      group.controls.costPrice.setValue(baseCostPrice);
    });
  }

  private computeSku(referenceCode: string, index: number): string {
    return `${referenceCode.trim()}-${index + 1}`;
  }
}

/* Verificar stock tracked e criação de variants */
