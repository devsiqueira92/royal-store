import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
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
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import {
  Product,
  ProductCategoryOption,
  ProductFormControls,
  ProductVariant,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import {
  CreateProductRequest,
  CreateProductVariantRequest,
} from '../../interfaces/create-product-request.interface';
import {
  UpdateProductRequest,
  UpdateProductVariantRequest,
} from '../../interfaces/update-product-request.interface';
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
import { ProductFacade } from '../../facade/product.facade';
import { CategoryFacade } from '@app/features/category/facade/category.facade';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

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
  private readonly productFacade = inject(ProductFacade);
  private readonly categoryFacade = inject(CategoryFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly unitOfMeasureOptions = UNIT_OF_MEASURE_OPTIONS;

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

  protected readonly categoryOptions = computed<Options[]>(() =>
    this.categories().map(category => ({
      code: category.id,
      description: category.name,
    })),
  );

  protected readonly selectedProduct = toSignal(
    this.productFacade.selectedProduct$,
    { initialValue: null as Product | null },
  );

  protected readonly photoFiles = signal<File[]>([]);
  protected readonly selectedTags = signal<ProductCategoryOption[]>([]);
  protected readonly formGroup = this.buildFormGroup();

  // Variants whose SKU came from the database keep that value forever — the
  // referenceCode-size-formIndex auto-fill only applies to variants without
  // a persisted SKU (new ones, or existing ones that never had one).
  private readonly skuLockedGroups = new Set<
    FormGroup<ProductVariantFormControls>
  >();

  protected get variants(): FormArray<FormGroup<ProductVariantFormControls>> {
    return this.formGroup.controls.variants;
  }

  constructor() {
    this.categoryFacade.loadCategories();

    effect(() => {
      const product = this.selectedProduct();
      if (product) {
        this.populateForm(product);
      } else {
        this.resetForm();
      }
    });

    // The base price/cost/minStock fields live-sync into every variant
    // form whose own field is still locked, in both create and edit mode.
    // A variant unlocked via its pencil/lock toggle stops receiving this
    // sync until it's locked again.
    this.formGroup.controls.price.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(value => this.syncLockedVariantFields('price', value));
    this.formGroup.controls.costPrice.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(value => this.syncLockedVariantFields('costPrice', value));

    this.formGroup.controls.minStockQuantity.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(value =>
        this.syncLockedVariantFields('minStockQuantity', value),
      );
  }

  private syncLockedVariantFields(
    field: 'price' | 'costPrice' | 'minStockQuantity',
    value: number | null,
  ): void {
    for (const group of this.variants.controls) {
      const control = group.controls[field];
      if (control.disabled) {
        control.setValue(value);
      }
    }
  }

  protected onPhotoFilesChange(files: File[]): void {
    this.photoFiles.set(files);
  }

  protected onTagsChange(tags: ProductCategoryOption[]): void {
    this.selectedTags.set(tags);
  }

  protected addVariant(): void {
    const group = this.buildVariantGroup(
      this.buildVariantSeedFromProductFields(),
    );
    this.variants.push(group);
    this.setComputedSku(group, this.variants.length - 1);
  }

  private buildVariantSeedFromProductFields(): VariantSeed {
    const { price, costPrice, minStockQuantity } =
      this.formGroup.getRawValue();

    return { price, costPrice, minStockQuantity };
  }

  protected duplicateVariant(index: number): void {
    const source = this.variants.at(index).getRawValue();
    const group = this.buildVariantGroup({ ...source, id: null });
    const insertIndex = index + 1;
    this.variants.insert(insertIndex, group);
    this.setComputedSku(group, insertIndex);
  }

  protected onReferenceCodeBlur(): void {
    const referenceCode = this.formGroup.controls.referenceCode.value ?? '';
    this.variants.controls.forEach((group, index) => {
      if (this.skuLockedGroups.has(group)) {
        return;
      }
      group.controls.sku.setValue(this.computeSku(referenceCode, index));
    });
  }

  private setComputedSku(
    group: FormGroup<ProductVariantFormControls>,
    index: number,
  ): void {
    const referenceCode = this.formGroup.controls.referenceCode.value ?? '';
    group.controls.sku.setValue(this.computeSku(referenceCode, index));
  }

  private computeSku(referenceCode: string, index: number): string {
    return `${referenceCode.trim()}-${index + 1}`;
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
    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.formGroup.invalid || this.variants.length === 0) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const {
      categoryId,
      name,
      referenceCode,
      description,
      unitOfMeasure,
      ncm,
      cest,
    } = this.formGroup.getRawValue();

    const baseRequest = {
      categoryId: categoryId ?? '',
      name: (name ?? '').trim(),
      referenceCode: (referenceCode ?? '').trim(),
      description: description?.trim() || undefined,
      unitOfMeasure: unitOfMeasure?.trim() || 'UN',
      ncm: ncm?.trim() || undefined,
      cest: cest?.trim() || undefined,
      origin: 0,
    };

    const product = this.selectedProduct();

    if (product) {
      const { toRemove, variantsToAdd, variantsToUpdate } =
        this.buildVariantChanges(product);

      const request: UpdateProductRequest = {
        ...baseRequest,
        version: product.version,
        variantsToAdd,
        variantsToUpdate,
      };
      this.productFacade.saveProductEdit(product.id, request, toRemove);
    } else {
      const variants = this.variants.controls.map(group =>
        this.toVariantRequest(group),
      );

      const request: CreateProductRequest = {
        ...baseRequest,
        variants,
        tagIds: this.selectedTags().map(tag => tag.id),
      };
      this.productFacade.createProduct(request);
    }

    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.closeDrawer();
  }

  private buildVariantChanges(product: Product): {
    toRemove: string[];
    variantsToAdd: CreateProductVariantRequest[];
    variantsToUpdate: UpdateProductVariantRequest[];
  } {
    const originalById = new Map(product.variants.map(v => [v.id, v]));
    const currentIds = new Set(
      this.variants.controls
        .map(group => group.getRawValue().id)
        .filter((id): id is string => !!id),
    );

    const toRemove = [...originalById.keys()].filter(id => !currentIds.has(id));
    const variantsToUpdate: UpdateProductVariantRequest[] = [];
    const variantsToAdd: CreateProductVariantRequest[] = [];

    for (const group of this.variants.controls) {
      const variantId = group.getRawValue().id;
      const original = variantId ? originalById.get(variantId) : undefined;
      const request = this.toVariantRequest(group);

      if (original) {
        if (this.variantChanged(original, request)) {
          variantsToUpdate.push({ variantId: original.id, ...request });
        }
      } else {
        variantsToAdd.push(request);
      }
    }

    return { toRemove, variantsToAdd, variantsToUpdate };
  }

  private variantChanged(
    original: ProductVariant,
    request: CreateProductVariantRequest,
  ): boolean {
    return (
      (original.size ?? undefined) !== request.size ||
      (original.color ?? undefined) !== request.color ||
      (original.sku ?? undefined) !== request.sku ||
      (original.barcode ?? undefined) !== request.barcode ||
      original.price !== request.price ||
      (original.costPrice ?? undefined) !== request.costPrice ||
      original.stockQuantity !== request.stockQuantity ||
      (original.minStockQuantity ?? undefined) !== request.minStockQuantity ||
      original.isStockTracked !== request.isStockTracked
    );
  }

  private toVariantRequest(
    group: FormGroup<ProductVariantFormControls>,
  ): CreateProductVariantRequest {
    const variant = group.getRawValue();

    return {
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
  }

  private populateForm(product: Product): void {
    this.formGroup.patchValue({
      id: product.id,
      categoryId: product.categoryId,
      name: product.name,
      referenceCode: product.referenceCode,
      description: product.description ?? null,
      unitOfMeasure: product.unitOfMeasure,
      ncm: product.ncm ?? null,
      cest: product.cest ?? null,
      isActive: product.isActive,
    });

    this.variants.clear();
    this.skuLockedGroups.clear();
    for (const variant of product.variants) {
      const group = this.buildVariantGroup(variant);
      this.variants.push(group);
      if (variant.sku) {
        this.skuLockedGroups.add(group);
      }
    }
    if (this.variants.length === 0) {
      this.addVariant();
    }

    // emitEvent: false — this just displays variant[0]'s values on the
    // base fields; it must not cascade into every other locked variant
    // and overwrite their own loaded price/cost/minStock with variant[0]'s.
    this.formGroup.patchValue(
      {
        price: product.variants[0]?.price ?? null,
        costPrice: product.variants[0]?.costPrice ?? null,
        minStockQuantity: product.variants[0]?.minStockQuantity ?? null,
      },
      { emitEvent: false },
    );

    this.selectedTags.set(
      product.tags.map(tag => ({
        id: tag.categoryId,
        name: tag.categoryName,
        color: tag.categoryColor,
      })),
    );
    this.photoFiles.set([]);
  }

  private resetForm(): void {
    // emitEvent: false — the variants array below still holds the previous
    // product's (or default) groups until cleared; the sync must not fire
    // against them mid-transition.
    this.formGroup.reset(
      {
        id: null,
        categoryId: null,
        name: null,
        referenceCode: null,
        description: null,
        unitOfMeasure: 'UN',
        ncm: null,
        cest: null,
        isActive: true,
        price: null,
        costPrice: null,
        minStockQuantity: null,
      },
      { emitEvent: false },
    );

    this.variants.clear();
    this.skuLockedGroups.clear();
    this.addVariant();
    this.selectedTags.set([]);
    this.photoFiles.set([]);
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
      // price/costPrice/minStockQuantity start locked — driven by the
      // product-level fields until the user clicks the per-variant pencil
      // toggle to edit them directly.
      price: new FormControl(
        { value: source?.price ?? 0, disabled: true },
        [Validators.required, Validators.min(0)],
      ),
      costPrice: new FormControl(
        { value: source?.costPrice ?? null, disabled: true },
        [Validators.min(0)],
      ),
      stockQuantity: new FormControl(source?.stockQuantity ?? 0, [
        Validators.required,
        Validators.min(0),
      ]),
      minStockQuantity: new FormControl(
        { value: source?.minStockQuantity ?? null, disabled: true },
      ),
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
      referenceCode: new FormControl(null, [
        Validators.required,
        Validators.maxLength(50),
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
      price: new FormControl(null, [Validators.min(0)]),
      costPrice: new FormControl(null, [Validators.min(0)]),
      minStockQuantity: new FormControl(null, [Validators.min(0)]),
      variants: new FormArray<FormGroup<ProductVariantFormControls>>([]),
    });
  }
}
