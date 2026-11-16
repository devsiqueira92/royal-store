import { DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { PriceListFacade } from '@app/features/price-list/facade/price-list.facade';
import { PriceList } from '@app/features/price-list/interfaces/price-list.interface';
import { ProductPriceFormControls } from '../../interfaces/price-list.interface';
import { Product } from '../../interfaces/product.interface';
import { ProductFacade } from '../../facade/product.facade';
import { buildProductPriceGroup } from '../../utils/product-price-form.util';

type PriceGroup = FormGroup<ProductPriceFormControls>;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-product-price-table',
  imports: [
    DecimalPipe,
    FormsModule,
    ReactiveFormsModule,
    TranslatePipe,
    InputNumberModule,
    InputNumberFieldComponent,
  ],
  templateUrl: './product-price-table.component.html',
  styleUrl: './product-price-table.component.scss',
})
export class ProductPriceTableComponent {
  private readonly priceListFacade = inject(PriceListFacade);
  protected readonly priceLists = toSignal(this.priceListFacade.priceLists$, {
    initialValue: [],
  });

  private readonly selectedProduct = toSignal(
    inject(ProductFacade).selectedProduct$,
    { initialValue: null as Product | null },
  );

  protected readonly pricedCount = computed(
    () =>
      this.priceLists().filter(entry => entry.discountPercentage !== null)
        .length,
  );
  readonly costPrice = input<number | null>(null);

  /** The product form's `prices` array: this table fills it, the footer submits it. */
  readonly prices = input.required<FormArray<PriceGroup>>();

  protected readonly form = computed(
    () => new FormGroup({ prices: this.prices() }),
  );

  // Zoneless: FormArray mutations are invisible to change detection, so the
  // template renders from this signal. Every mutation goes through `syncPrices`.
  protected readonly groups = signal<PriceGroup[]>([]);

  constructor() {
    this.priceListFacade.loadPriceLists();

    effect(() => this.syncPrices());

    inject(DestroyRef).onDestroy(() => this.prices().clear());
  }

  private syncPrices(): void {
    const lists = this.priceLists();
    this.costPrice();
    const saved = this.selectedProduct()?.prices ?? [];
    const array = this.prices();

    untracked(() => {
      array.clear();
      lists.forEach(list => {
        const savedPrice = saved.find(p => p.priceListId === list.id);
        array.push(
          buildProductPriceGroup(
            list.id,
            savedPrice ? savedPrice.salePrice : this.discountedPrice(list),
          ),
        );
      });
      this.groups.set([...array.controls]);
    });
  }

  private discountedPrice(list: PriceList, depth = 0): number | null {
    const cost = this.costPrice();
    if (cost === null || list.discountPercentage === null) return cost;

    const base = this.priceLists().find(l => l.id === list.basePriceListId);
    const basePrice =
      base && depth < this.priceLists().length
        ? this.discountedPrice(base, depth + 1)
        : cost;
    if (basePrice === null) return null;

    return (
      Math.round(basePrice * (1 - list.discountPercentage / 100) * 100) / 100
    );
  }

  protected effectiveSalePrice(
    group: PriceGroup,
    list: PriceList,
  ): number | null {
    const own = group.controls.salePrice.value;
    if (own !== null || list.isDefault) return own;
    const defaultIndex = this.priceLists().findIndex(l => l.isDefault);
    return this.groups()[defaultIndex]?.controls.salePrice.value ?? null;
  }

  protected usesDefault(group: PriceGroup, list: PriceList): boolean {
    return !list.isDefault && group.controls.salePrice.value === null;
  }

  protected margin(group: PriceGroup, list: PriceList): number | null {
    const sale = this.effectiveSalePrice(group, list);
    const cost = this.costPrice();
    if (sale === null || sale <= 0 || cost === null) return null;
    return ((sale - cost) / sale) * 100;
  }
}
