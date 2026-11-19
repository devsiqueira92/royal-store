import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { TooltipModule } from 'primeng/tooltip';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { startWith, switchMap } from 'rxjs';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';
import { Options } from '@app/shared/interceptors/options.interface';
import { CostCompositionFacade } from '@app/features/cost-composition/facade/cost-composition.facade';
import { CostComposition } from '@app/features/cost-composition/interfaces/cost-composition.interface';
import { ProductPriceTableComponent } from '../product-price-table/product-price-table.component';
import {
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';

const roundCents = (value: number): number => Math.round(value * 100) / 100;

@Component({
  selector: 'app-product-pricing',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    CurrencyPipe,
    TooltipModule,
    FormLayoutComponent,
    InputNumberFieldComponent,
    SelectFieldComponent,
    ProductPriceTableComponent,
  ],
  templateUrl: './product-pricing.component.html',
  styleUrl: './product-pricing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductPricingComponent {
  private readonly costCompositionFacade = inject(CostCompositionFacade);
  private readonly translate = inject(TranslateService);
  // Native Intl, not `new CurrencyPipe('pt-BR')`: the pipe needs Angular's pt-BR
  // locale data registered (the app runs on en-US), otherwise it throws NG0701.
  private readonly currency = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });

  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  protected readonly showPriceTable = signal(false);

  protected togglePriceTable(): void {
    this.showPriceTable.update(v => !v);
  }

  private readonly costCompositions = toSignal(
    this.costCompositionFacade.costCompositions$,
    { initialValue: [] as CostComposition[] },
  );

  protected readonly costCompositionOptions = computed<Options[]>(() =>
    this.costCompositions().map(c => ({ code: c.id, description: c.name })),
  );

  protected readonly isLoadingCostCompositions = toSignal(
    this.costCompositionFacade.isLoading$,
    { initialValue: false },
  );

  // The product cost is a UI-only helper: the backend stores the total
  // (baseCostPrice) and the linked composition, so the product cost is
  // derived back as total minus the composition's total when a product loads.
  protected readonly productCost = new FormControl<number | null>(null);
  protected readonly profitMarginPercent = new FormControl<number | null>(0);
  protected readonly profitAmount = new FormControl<number | null>(null);

  protected readonly localFormGroup = new FormGroup({
    productCost: this.productCost,
    profitMarginPercent: this.profitMarginPercent,
    profitAmount: this.profitAmount,
  });

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

  private readonly costCompositionIdValue = toSignal(
    toObservable(this.formGroup).pipe(
      switchMap(formGroup =>
        formGroup.controls.costCompositionId.valueChanges.pipe(
          startWith(formGroup.controls.costCompositionId.value),
        ),
      ),
    ),
    { initialValue: null as string | null },
  );

  protected readonly selectedComposition = computed(
    () =>
      this.costCompositions().find(
        c => c.id === this.costCompositionIdValue(),
      ) ?? null,
  );

  /** The cost typed in "Product cost": total minus the linked composition. */
  protected readonly productCostPart = computed(() =>
    roundCents(
      (this.baseCostPrice() ?? 0) -
        (this.selectedComposition()?.totalValue ?? 0),
    ),
  );

  protected readonly costTooltip = computed(() => {
    const format = (v: number): string => this.currency.format(v);
    const composition = this.selectedComposition();
    const base = `${this.translate.instant('product.form.productCost')}: ${format(this.productCostPart())}`;
    return composition
      ? `${base} + ${format(composition.totalValue)} (${composition.name})`
      : base;
  });

  protected readonly approximateProfit = computed(() => {
    const price = this.basePrice();
    const cost = this.baseCostPrice();
    return price == null || cost == null ? null : roundCents(price - cost);
  });

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
    this.costCompositionFacade.loadCostCompositions();

    this.profitAmount.disable({ emitEvent: false });

    // costPrice is shown read-only — it is always the product's base cost
    // plus the linked composition's total, never typed directly.
    effect(() => {
      this.formGroup().controls.baseCostPrice.disable({ emitEvent: false });
    });

    effect(() => {
      const id = this.productId();
      const compositions = this.costCompositions();
      if (id === this.seededProductId) {
        return;
      }

      // A linked composition's total is needed to derive the product cost;
      // wait for the list to arrive before seeding.
      const compositionId = this.formGroup().controls.costCompositionId.value;
      if (compositionId && !compositions.some(c => c.id === compositionId)) {
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
        price == null || cost == null ? null : roundCents(price - cost),
        { emitEvent: false },
      );

      if (price == null || cost == null || cost === 0) {
        this.profitMarginPercent.setValue(null, { emitEvent: false });
        return;
      }
      if (price) {
        const percent = Math.round(((price - cost) / cost) * 100);
        this.profitMarginPercent.setValue(percent, { emitEvent: false });
      }
    });
  }

  protected onCostCompositionChange(): void {
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

    const basePrice = roundCents(costPrice * (1 + percent / 100));
    this.formGroup().controls.basePrice.setValue(basePrice);
    this.onBasePriceBlur();
  }

  /** `totalValue` of the selected composition; 0 when none is selected. */
  private selectedCompositionTotal(): number {
    const id = this.formGroup().controls.costCompositionId.value;
    return this.costCompositions().find(c => c.id === id)?.totalValue ?? 0;
  }

  // Only the product cost has to be derived back from the stored total.
  private seedPricing(): void {
    const existingCostPrice = this.formGroup().controls.baseCostPrice.value;
    const productCost =
      existingCostPrice == null
        ? null
        : roundCents(existingCostPrice - this.selectedCompositionTotal());

    this.productCost.setValue(productCost, { emitEvent: false });
    this.recalculateCostPrice();
  }

  private recalculateCostPrice(): void {
    const total = roundCents(
      (this.productCost.value ?? 0) + this.selectedCompositionTotal(),
    );
    this.formGroup().controls.baseCostPrice.setValue(total);
  }

  private cascadeCostPriceToVariants(): void {
    const baseCostPrice = this.formGroup().controls.baseCostPrice.value;
    this.variants().controls.forEach(group => {
      group.controls.costPrice.setValue(baseCostPrice);
    });
  }
}
