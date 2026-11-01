import { Component, computed, inject, input, signal } from '@angular/core';
import { SupplierFacade } from '@app/features/supplier/facade/supplier.facade';
import { Supplier } from '@app/features/supplier/interfaces/supplier.interface';
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
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, startWith, switchMap } from 'rxjs';
import { UnitMeasureOptionFacade } from '@app/core/unit-measure-option/facade/unit-measure-option.facade';

@Component({
  selector: 'app-product-detail',
  imports: [
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
  private readonly supplierFacade = inject(SupplierFacade);

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

  // Suppliers are loaded once, app-wide, by AdminLayoutComponent — read the
  // store here, never dispatch another load from this component.
  private readonly allSuppliers = toSignal(this.supplierFacade.suppliers$, {
    initialValue: [] as Supplier[],
  });

  private readonly selectedSupplierId = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(formGroup =>
        formGroup.controls.supplierId.valueChanges.pipe(
          startWith(formGroup.controls.supplierId.value),
        ),
      ),
    ),
    { initialValue: null as string | null },
  );

  // Inactive suppliers are hidden from new assignments, but a product already
  // linked to one must keep showing it (backend FR-010).
  protected readonly supplierOptions = computed<Options[]>(() =>
    this.allSuppliers()
      .filter(
        supplier =>
          supplier.isActive || supplier.id === this.selectedSupplierId(),
      )
      .map(supplier => ({ code: supplier.id, description: supplier.name })),
  );

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

  private computeSku(referenceCode: string, index: number): string {
    return `${referenceCode.trim()}-${index + 1}`;
  }
}

/* Verificar stock tracked e criação de variants */
