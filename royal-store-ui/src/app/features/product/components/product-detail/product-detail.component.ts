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
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { UnitMeasureOptionFacade } from '@app/core/unit-measure-option/facade/unit-measure-option.facade';

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
  private readonly unitMeasureOptionFacade = inject(UnitMeasureOptionFacade);

  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();
  itemType = input.required<number>();

  private readonly skuLockedGroups = new Set<
    FormGroup<ProductVariantFormControls>
  >();

  protected readonly unitOfMeasureOptions = toSignal(
    this.unitMeasureOptionFacade.unitMeasureOptions$.pipe(
      map(options =>
        options.map((option): Options => ({
          code: option.id,
          description: option.name,
        })),
      ),
    ),
    { initialValue: [] as Options[] },
  );
  protected readonly isUnitOfMeasureOptionsLoading = toSignal(
    this.unitMeasureOptionFacade.isLoading$,
    { initialValue: false },
  );
  protected readonly photoFiles = signal<File[]>([]);

  constructor() {
    this.unitMeasureOptionFacade.loadUnitMeasureOptions();
  }

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
