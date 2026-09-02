import { Component, computed, inject, input, signal } from '@angular/core';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { TextareaFieldComponent } from '@app/shared/components/form-fields/textarea-field/textarea-field.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { ImageUploadFieldComponent } from '@app/shared/components/form-fields/image-upload-field/image-upload-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import {
  FormArray,
  FormGroup,
  ReactiveFormsModule,
  UntypedFormGroup,
} from '@angular/forms';
import { Options } from '@app/shared/interceptors/options.interface';
import { toSignal } from '@angular/core/rxjs-interop';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { map } from 'rxjs/internal/operators/map';
import {
  ProductCategoryOption,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { TranslatePipe } from '@ngx-translate/core';

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
  ],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
})
export class ProductDetailComponent {
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  private readonly categoryFacade = inject(CategoryFacade);

  private readonly skuLockedGroups = new Set<
    FormGroup<ProductVariantFormControls>
  >();

  protected readonly unitOfMeasureOptions = UNIT_OF_MEASURE_OPTIONS;
  protected readonly photoFiles = signal<File[]>([]);

  protected readonly categories = toSignal(
    this.categoryFacade.categories$.pipe(
      map(categories =>
        categories.map((category): ProductCategoryOption => ({
          id: category.id,
          name: category.name,
          color: category.color,
        })),
      ),
    ),
    { initialValue: [] as ProductCategoryOption[] },
  );

  constructor() {
    this.categoryFacade.loadCategories();
  }

  protected readonly categoryOptions = computed<Options[]>(() =>
    this.categories().map(category => ({
      code: category.id,
      description: category.name,
    })),
  );

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
