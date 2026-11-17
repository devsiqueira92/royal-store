import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
  signal,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { startWith, switchMap } from 'rxjs';
import { DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { ProductPriceTableComponent } from '../product-price-table/product-price-table.component';
import {
  ProductCostItemFormControls,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { ProductPriceFormControls } from '../../interfaces/price-list.interface';
import {
  buildProductCostItemGroup,
  COST_ITEM_DESCRIPTION_MAX_LENGTH,
  sumProductCostItems,
} from '../../utils/product-cost-item-form.util';

@Component({
  selector: 'app-product-pricing',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    DecimalPipe,
    FormLayoutComponent,
    InputNumberFieldComponent,
    ProductPriceTableComponent,
  ],
  templateUrl: './product-pricing.component.html',
  styleUrl: './product-pricing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductPricingComponent {
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  protected readonly priceGroups = signal<
    FormGroup<ProductPriceFormControls>[]
  >([]);
  protected readonly showPriceTable = signal(false);

  protected togglePriceTable(): void {
    this.showPriceTable.update(v => !v);
  }

  protected readonly descriptionMaxLength = COST_ITEM_DESCRIPTION_MAX_LENGTH;

  // The product cost is a UI-only helper: the backend stores the total
  // (baseCostPrice) and the composition lines (costItems), so the product
  // cost is derived back as total minus lines when a product loads.
  protected readonly productCost = new FormControl<number | null>(null);
  protected readonly profitMarginPercent = new FormControl<number | null>(null);
  protected readonly profitAmount = new FormControl<number | null>(null);

  protected readonly localFormGroup = new FormGroup({
    productCost: this.productCost,
    profitMarginPercent: this.profitMarginPercent,
    profitAmount: this.profitAmount,
  });

  // Zoneless app: mutating the FormArray (push/removeAt) is invisible to
  // change detection, so the template renders from this signal instead of
  // `costItems.controls`. Every mutation goes through `syncCostItemGroups`.
  protected readonly costItemGroups = signal<
    FormGroup<ProductCostItemFormControls>[]
  >([]);

  private get costItems(): FormArray<FormGroup<ProductCostItemFormControls>> {
    return this.formGroup().controls.costItems;
  }

  readonly basePrice = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(formGroup =>
        formGroup.controls.basePrice.valueChanges.pipe(
          startWith(formGroup.controls.basePrice.value),
        ),
      ),
    ),
    { initialValue: null as number | null },
  );

  readonly baseCostPrice = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(formGroup =>
        formGroup.controls.baseCostPrice.valueChanges.pipe(
          startWith(formGroup.controls.baseCostPrice.value),
        ),
      ),
    ),
    { initialValue: null as number | null },
  );

  private readonly productId = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(formGroup =>
        formGroup.controls.id.valueChanges.pipe(
          startWith(formGroup.controls.id.value),
        ),
      ),
    ),
    { initialValue: null as string | null },
  );

  // Rebuilt only when a different product loads: `formGroup().patchValue`
  // runs synchronously and this effect is scheduled afterwards, so reading
  // the control's live value here always sees the patched total regardless
  // of the patch's key order.
  private seededProductId: string | null | undefined = undefined;

  constructor() {
    this.profitAmount.disable({ emitEvent: false });

    // costPrice is shown read-only — it is always the product's base cost
    // plus the composition lines, never typed directly.
    effect(() => {
      this.formGroup().controls.baseCostPrice.disable({ emitEvent: false });
    });

    effect(() => {
      const id = this.productId();
      if (id === this.seededProductId) {
        return;
      }
      this.seededProductId = id;
      this.seedPricing();
    });

    // Keeps the % field and the read-only profit amount in sync whenever
    // price/cost change through any other means (product load, blur
    // cascades) — never the other way around, to avoid fighting the user
    // while they type a new percentage.
    effect(() => {
      const price = this.basePrice();
      const cost = this.baseCostPrice();

      this.profitAmount.setValue(
        price == null || cost == null
          ? null
          : Math.round((price - cost) * 100) / 100,
        { emitEvent: false },
      );

      if (price == null || cost == null || cost === 0) {
        this.profitMarginPercent.setValue(null, { emitEvent: false });
        return;
      }

      const percent = Math.round(((price - cost) / cost) * 100);
      this.profitMarginPercent.setValue(percent, { emitEvent: false });
    });
  }

  protected addCostItem(): void {
    this.costItems.push(buildProductCostItemGroup());
    this.syncCostItemGroups();
  }

  protected removeCostItem(index: number): void {
    this.costItems.removeAt(index);
    this.syncCostItemGroups();
    this.recalculateCostPrice();
    this.cascadeCostPriceToVariants();
  }

  protected onCostItemInput(): void {
    this.recalculateCostPrice();
  }

  protected onCostItemBlur(): void {
    this.recalculateCostPrice();
    this.cascadeCostPriceToVariants();
  }

  protected onProductCostBlur(): void {
    this.recalculateCostPrice();
    this.cascadeCostPriceToVariants();
  }

  protected onBasePriceBlur(): void {
    const basePrice = this.formGroup().controls.basePrice.value;
    this.variants().controls.forEach(group => {
      group.controls.price.setValue(basePrice);
    });
  }

  protected onProfitMarginBlur(): void {
    const percent = this.profitMarginPercent.value;
    const costPrice = this.formGroup().controls.baseCostPrice.value;

    if (percent == null || costPrice == null) {
      return;
    }

    const basePrice = Math.round(costPrice * (1 + percent / 100) * 100) / 100;
    this.formGroup().controls.basePrice.setValue(basePrice);
    this.onBasePriceBlur();
  }

  // The rows were already rebuilt by the form from the loaded product; only
  // the product cost has to be derived back from the stored total.
  private seedPricing(): void {
    this.syncCostItemGroups();

    const existingCostPrice = this.formGroup().controls.baseCostPrice.value;
    const productCost =
      existingCostPrice == null
        ? null
        : Math.round(
            (existingCostPrice - sumProductCostItems(this.costItems.controls)) *
              100,
          ) / 100;

    this.productCost.setValue(productCost, { emitEvent: false });
    this.recalculateCostPrice();
  }

  private syncCostItemGroups(): void {
    this.costItemGroups.set([...this.costItems.controls]);
  }

  private recalculateCostPrice(): void {
    const total =
      Math.round(
        ((this.productCost.value ?? 0) +
          sumProductCostItems(this.costItemGroups())) *
          100,
      ) / 100;
    this.formGroup().controls.baseCostPrice.setValue(total);
  }

  private cascadeCostPriceToVariants(): void {
    const baseCostPrice = this.formGroup().controls.baseCostPrice.value;
    this.variants().controls.forEach(group => {
      group.controls.costPrice.setValue(baseCostPrice);
    });
  }
}
