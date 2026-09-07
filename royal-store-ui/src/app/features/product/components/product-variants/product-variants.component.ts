import { Component, effect, inject, input, signal } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import {
  ProductFormControls,
  ProductVariant,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductVariantFormComponent } from '../product-variant-form/product-variant-form.component';
import { ProductFacade } from '../../facade/product.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs';
import { toggleControl } from '@app/shared/utils/toggle-control.utils';

type VariantSeed = Partial<{
  [K in keyof ProductVariant]: ProductVariant[K] | null;
}>;

@Component({
  selector: 'app-product-variants',
  imports: [FormLayoutComponent, TranslatePipe, ProductVariantFormComponent],
  templateUrl: './product-variants.component.html',
  styleUrl: './product-variants.component.scss',
})
export class ProductVariantsComponent {
  itemType = input.required<number>();
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  private readonly productFacade = inject(ProductFacade);

  protected readonly selectedVariants = toSignal(
    this.productFacade.productVariants$,
    { initialValue: [] as ProductVariant[] },
  );

  protected readonly selectedProduct = toSignal(
    this.productFacade.selectedProduct$,
    { initialValue: null },
  );

  // The app runs zoneless: mutating the FormArray (push/insert/removeAt) is
  // invisible to change detection, so the template renders from this signal
  // instead of `variants().controls`. Every mutation goes through
  // `syncVariantGroups`, which is what actually repaints the list.
  protected readonly variantGroups = signal<
    FormGroup<ProductVariantFormControls>[]
  >([]);

  // Rebuild the FormArray only when a different product is loaded. Later
  // emissions for the same product (a variant deleted on the backend) are
  // reconciled instead, so rows the user is still filling in survive.
  private populatedForProductId: string | null | undefined = undefined;

  constructor() {
    effect(() => {
      const variants = this.selectedVariants() ?? [];
      const productId = this.selectedProduct()?.id ?? null;

      if (productId !== this.populatedForProductId) {
        this.populatedForProductId = productId;
        this.rebuildForm(variants);
        return;
      }

      this.dropRemovedVariants(variants);
    });
  }

  ngOnInit(): void {
    const stockTrackControl = this.formGroup().controls.stockTrack;
    stockTrackControl.valueChanges
      .pipe(startWith(stockTrackControl.value))
      .subscribe(() => this.refreshAllVariants());
  }

  protected addVariant(): void {
    const group = this.buildVariantGroup();
    this.applyVariantVisibility(group);
    this.variants().push(group);
    this.setComputedSku(group, this.variants().length - 1);
    this.syncVariantGroups();
  }

  protected duplicateVariant(index: number): void {
    const source = this.variants().at(index).getRawValue();
    const group = this.buildVariantGroup({ ...source, id: null });
    this.applyVariantVisibility(group);
    const insertIndex = index + 1;
    this.variants().insert(insertIndex, group);
    this.setComputedSku(group, insertIndex);
    this.syncVariantGroups();
  }

  protected removeVariant(
    index: number,
    variantGroup: FormGroup<ProductVariantFormControls>,
  ): void {
    if (this.variants().length <= 1) {
      return;
    }

    const variantId = variantGroup.controls.id.value;

    // Persisted variant: DELETE /api/products/variants/{variantId} is the
    // source of truth, so the row leaves the form once the store drops it.
    // Unsaved row: nothing to delete, remove it right away.
    if (variantId) {
      this.productFacade.removeProductVariant(variantId);
      return;
    }

    this.variants().removeAt(index);
    this.syncVariantGroups();
  }

  private rebuildForm(variants: ProductVariant[]): void {
    this.variants().clear();

    if (variants.length === 0) {
      this.addVariant();
      return;
    }

    this.populateForm(variants);
    this.syncVariantGroups();
  }

  /** Drops the rows whose variant no longer exists in the store. */
  private dropRemovedVariants(variants: ProductVariant[]): void {
    const persistedIds = new Set(variants.map(variant => variant.id));
    const controls = this.variants().controls;
    let removed = false;

    for (let index = controls.length - 1; index >= 0; index--) {
      const id = controls[index].controls.id.value;
      if (id && !persistedIds.has(id)) {
        this.variants().removeAt(index);
        removed = true;
      }
    }

    if (!removed) {
      return;
    }

    if (this.variants().length === 0) {
      this.addVariant();
      return;
    }

    this.syncVariantGroups();
  }

  private syncVariantGroups(): void {
    this.variantGroups.set([...this.variants().controls]);
  }

  private populateForm(variants: ProductVariant[]): void {
    for (const variant of variants) {
      const group = this.buildVariantGroup(variant);
      this.applyVariantVisibility(group);
      this.variants().push(group);
    }
  }

  private refreshAllVariants(): void {
    for (const group of this.variants().controls) {
      this.applyVariantVisibility(group);
    }
  }

  /**
   * Toggles a variant's fields based on the owning item's kind and stock
   * tracking: `optionName` is required-and-visible only for a Service;
   * `size`/`color`/`sku`/`barcode`/`stockQuantity`/`minStockQuantity` are
   * Product-only, and among those the stock fields are further gated by
   * `stockTrack` (required when tracked, optional otherwise) — see
   * data-model.md FR-010/FR-011/FR-015/FR-016.
   */
  private applyVariantVisibility(
    group: FormGroup<ProductVariantFormControls>,
  ): void {
    const isService = this.itemType() === 1;
    const stockTrack = !!this.formGroup().controls.stockTrack.value;

    toggleControl(group.controls.optionName, isService);
    toggleControl(group.controls.size, !isService);
    toggleControl(group.controls.color, !isService);
    toggleControl(group.controls.sku, !isService);
    toggleControl(group.controls.barcode, !isService);
    this.setRequired(group.controls.optionName, isService);

    const trackStockFields = !isService && stockTrack;
    toggleControl(group.controls.stockQuantity, !isService);
    toggleControl(group.controls.minStockQuantity, !isService);
    this.setRequired(group.controls.sku, trackStockFields);
    this.setRequired(group.controls.barcode, trackStockFields);
    this.setRequired(group.controls.stockQuantity, trackStockFields);
  }

  private setRequired(control: AbstractControl, required: boolean): void {
    control.setValidators(
      required
        ? [Validators.required, Validators.maxLength(50)]
        : [Validators.maxLength(50)],
    );
    control.updateValueAndValidity({ emitEvent: false });
  }

  private setComputedSku(
    group: FormGroup<ProductVariantFormControls>,
    index: number,
  ): void {
    const referenceCode = this.formGroup().controls.sku.value ?? '';
    group.controls.sku.setValue(this.computeSku(referenceCode, index));
  }

  private computeSku(referenceCode: string, index: number): string {
    return `${referenceCode.trim()}-${index + 1}`;
  }

  private buildVariantGroup(
    source?: VariantSeed,
  ): FormGroup<ProductVariantFormControls> {
    return new FormGroup<ProductVariantFormControls>({
      id: new FormControl(source?.id ?? null),
      size: new FormControl(source?.size ?? null, [Validators.maxLength(50)]),
      color: new FormControl(source?.color ?? null, [Validators.maxLength(50)]),
      optionName: new FormControl(source?.optionName ?? null, [
        Validators.maxLength(50),
      ]),
      sku: new FormControl(source?.sku ?? null, [Validators.maxLength(50)]),
      barcode: new FormControl(source?.barcode ?? null, [
        Validators.maxLength(14),
      ]),
      price: new FormControl(source?.price ?? null, [Validators.min(0)]),
      costPrice: new FormControl(source?.costPrice ?? null, [
        Validators.min(0),
      ]),

      stockQuantity: new FormControl(source?.stockQuantity ?? null, [
        Validators.min(0),
      ]),
      minStockQuantity: new FormControl(source?.minStockQuantity ?? null, [
        Validators.min(0),
      ]),
    });
  }
}
