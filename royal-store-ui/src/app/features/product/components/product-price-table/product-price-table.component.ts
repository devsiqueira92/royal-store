import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { map, merge, of, startWith, switchMap } from 'rxjs';
import {
  PriceList,
  ProductPriceFormControls,
} from '../../interfaces/price-list.interface';

interface PriceEntry {
  list: PriceList;
  group: FormGroup<ProductPriceFormControls>;
  /** Margin over the sale price, or over the default price when this list has none. */
  margin: number | null;
  /** The list has no price of its own and sells at the default one. */
  usesDefault: boolean;
  minAboveSale: boolean;
}

/**
 * The product's sale price on each active price list.
 *
 * Presentational: it edits the form groups it is given and never builds or
 * removes one — the parent keeps a group per list.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-product-price-table',
  imports: [DecimalPipe, FormsModule, ReactiveFormsModule, TranslatePipe, InputNumberModule],
  templateUrl: './product-price-table.component.html',
  styleUrl: './product-price-table.component.scss',
})
export class ProductPriceTableComponent {
  readonly priceLists = input.required<PriceList[]>();
  readonly priceGroups = input.required<FormGroup<ProductPriceFormControls>[]>();
  readonly costPrice = input<number | null>(null);

  // Zoneless: a control changing value is invisible to change detection, so
  // margins and counts are derived from the groups' value streams.
  private readonly values = toSignal(
    toObservable(this.priceGroups).pipe(
      switchMap((groups) =>
        groups.length
          ? merge(...groups.map((group) => group.valueChanges)).pipe(
              startWith(null),
              map(() => groups.map((group) => group.getRawValue())),
            )
          : of([]),
      ),
    ),
    { initialValue: [] },
  );

  protected readonly entries = computed<PriceEntry[]>(() => {
    const values = this.values();
    const groups = this.priceGroups();
    const lists = this.priceLists();
    const cost = this.costPrice();

    const defaultId = lists.find((list) => list.isDefault)?.id;
    const defaultPrice =
      values.find((value) => value.priceListId === defaultId)?.salePrice ?? null;

    return lists
      .filter((list) => list.isActive)
      .flatMap((list) => {
        const group = groups.find((item) => item.controls.priceListId.value === list.id);
        const value = values.find((item) => item.priceListId === list.id);

        if (!group || !value) {
          return [];
        }

        const price = value.salePrice ?? defaultPrice;

        return [
          {
            list,
            group,
            margin: price && cost != null ? ((price - cost) / price) * 100 : null,
            usesDefault: value.salePrice == null,
            minAboveSale:
              value.salePrice != null && value.minPrice != null && value.minPrice > value.salePrice,
          },
        ];
      });
  });

  protected readonly pricedCount = computed(
    () => this.entries().filter((entry) => !entry.usesDefault).length,
  );
}
