import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import {
  ProductCategoryOption,
  ProductFormControls,
  ProductVariant,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import {
  ProductRequest,
  ProductVariantRequest,
} from '../../interfaces/product-request.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { TextareaFieldComponent } from '@app/shared/components/form-fields/textarea-field/textarea-field.component';
import { ImageUploadFieldComponent } from '@app/shared/components/form-fields/image-upload-field/image-upload-field.component';
import { ToggleFieldComponent } from '@app/shared/components/form-fields/toggle-field/toggle-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { ProductVariantFormComponent } from '../product-variant-form/product-variant-form.component';
import { ProductTagsFieldComponent } from '../product-tags-field/product-tags-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';

type VariantSeed = Partial<{
  [K in keyof ProductVariant]: ProductVariant[K] | null;
}>;

const UNIT_OF_MEASURE_OPTIONS: Options[] = [
  { code: 'UN', description: 'UN — Unit' },
  { code: 'PC', description: 'PC — Piece' },
  { code: 'PAR', description: 'PAR — Pair' },
  { code: 'CX', description: 'CX — Box' },
  { code: 'KG', description: 'KG — Kilogram' },
];

@Component({
  selector: 'app-product-form',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    TranslatePipe,
    CurrencyPipe,
    InputFieldComponent,
    SelectFieldComponent,
    TextareaFieldComponent,
    ImageUploadFieldComponent,
    ToggleFieldComponent,
    ProductVariantFormComponent,
    ProductTagsFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormComponent {
  categories = input<ProductCategoryOption[]>([]);
  close = output<void>();
  submit = output<ProductRequest>();

  protected readonly unitOfMeasureOptions = UNIT_OF_MEASURE_OPTIONS;

  protected readonly categoryOptions = computed<Options[]>(() =>
    this.categories().map(category => ({
      code: category.id,
      description: category.name,
    })),
  );

  protected readonly photoFiles = signal<File[]>([]);
  protected readonly selectedTags = signal<ProductCategoryOption[]>([]);
  protected readonly formGroup = this.buildFormGroup();

  protected get variants(): FormArray<FormGroup<ProductVariantFormControls>> {
    return this.formGroup.controls.variants;
  }

  constructor() {
    this.addVariant();
  }

  protected onPhotoFilesChange(files: File[]): void {
    this.photoFiles.set(files);
  }

  protected onTagsChange(tags: ProductCategoryOption[]): void {
    this.selectedTags.set(tags);
  }

  protected addVariant(): void {
    this.variants.push(this.buildVariantGroup());
  }

  protected duplicateVariant(index: number): void {
    const source = this.variants.at(index).getRawValue();
    this.variants.insert(index + 1, this.buildVariantGroup(source));
  }

  protected removeVariant(index: number): void {
    if (this.variants.length <= 1) {
      return;
    }

    this.variants.removeAt(index);
  }

  protected getSummary(): {
    variantsCount: number;
    totalUnits: number;
    minPrice: number | null;
    maxPrice: number | null;
  } {
    const rows = this.variants.controls.map(group => group.getRawValue());
    const totalUnits = rows.reduce(
      (total, row) => total + (Number(row.stockQuantity) ?? 0),
      0,
    );
    const prices = rows.map(row => row.price ?? 0);

    return {
      variantsCount: rows.length,
      totalUnits,
      minPrice: prices.length ? Math.min(...prices) : null,
      maxPrice: prices.length ? Math.max(...prices) : null,
    };
  }

  protected onClose(): void {
    this.close.emit();
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.formGroup.invalid || this.variants.length === 0) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const {
      id,
      categoryId,
      name,
      description,
      unitOfMeasure,
      ncm,
      cest,
      isActive,
    } = this.formGroup.getRawValue();

    const variants: ProductVariantRequest[] = this.variants.controls.map(
      group => {
        const variant = group.getRawValue();

        return {
          id: variant.id,
          size: variant.size?.trim() || undefined,
          color: variant.color?.trim() || undefined,
          sku: variant.sku?.trim() || undefined,
          barcode: variant.barcode?.trim() || undefined,
          price: variant.price ?? 0,
          costPrice: variant.costPrice ?? undefined,
          stockQuantity: variant.stockQuantity ?? 0,
          minStockQuantity: variant.minStockQuantity ?? undefined,
          isStockTracked: variant.isStockTracked ?? true,
        };
      },
    );

    this.submit.emit({
      id,
      categoryId: categoryId ?? undefined,
      name: (name ?? '').trim(),
      description: description?.trim() || undefined,
      unitOfMeasure: unitOfMeasure?.trim() || 'UN',
      ncm: ncm?.trim() || undefined,
      cest: cest?.trim() || undefined,
      origin: 0,
      isActive: isActive ?? true,
      variants,
      tagIds: this.selectedTags().map(tag => tag.id),
      photos: this.photoFiles(),
    });
  }

  private buildVariantGroup(
    source?: VariantSeed,
  ): FormGroup<ProductVariantFormControls> {
    return new FormGroup<ProductVariantFormControls>({
      id: new FormControl(source?.id ?? null),
      size: new FormControl(source?.size ?? null, [Validators.maxLength(50)]),
      color: new FormControl(source?.color ?? null, [Validators.maxLength(50)]),
      sku: new FormControl(source?.sku ?? null, [Validators.maxLength(50)]),
      barcode: new FormControl(source?.barcode ?? null, [
        Validators.maxLength(14),
      ]),
      price: new FormControl(source?.price ?? 0, [
        Validators.required,
        Validators.min(0),
      ]),
      costPrice: new FormControl(source?.costPrice ?? null, [
        Validators.min(0),
      ]),
      stockQuantity: new FormControl(source?.stockQuantity ?? 0, [
        Validators.required,
        Validators.min(0),
      ]),
      minStockQuantity: new FormControl(source?.minStockQuantity ?? null),
      isStockTracked: new FormControl(source?.isStockTracked ?? true),
    });
  }

  private buildFormGroup() {
    return new FormGroup<ProductFormControls>({
      id: new FormControl(null),
      categoryId: new FormControl(null, [Validators.required]),
      name: new FormControl(null, [
        Validators.required,
        Validators.maxLength(120),
      ]),
      description: new FormControl(null, [Validators.maxLength(500)]),
      unitOfMeasure: new FormControl('UN', [Validators.maxLength(6)]),
      ncm: new FormControl(null, [
        Validators.maxLength(8),
        Validators.pattern(/^\d+$/),
      ]),
      cest: new FormControl(null, [
        Validators.maxLength(7),
        Validators.pattern(/^\d+$/),
      ]),
      isActive: new FormControl(true),
      variants: new FormArray<FormGroup<ProductVariantFormControls>>([]),
    });
  }
}
