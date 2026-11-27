import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { SalePriceListOption } from '../../interfaces/sale-price-list-option.interface';

/** How a list's price relates to its base, as told under its name. */
type PriceListRuleKind = 'default' | 'discount' | 'surcharge' | 'manual';

interface PriceListTile {
  option: SalePriceListOption;
  kind: PriceListRuleKind;
  /** Size of the discount or surcharge as a 0..1 rate; 0 for the other kinds. */
  rate: number;
}

function toTile(option: SalePriceListOption): PriceListTile {
  const percentage = option.discountPercentage ?? 0;

  const kind: PriceListRuleKind = option.isDefault
    ? 'default'
    : percentage < 0
      ? 'discount'
      : percentage > 0
        ? 'surcharge'
        : 'manual';

  return { option, kind, rate: Math.abs(percentage) / 100 };
}

/** Tiles of the price lists a sale can be charged on: name, its rule and the cart's subtotal on it. */
@Component({
  selector: 'app-price-list-picker',
  imports: [CurrencyPipe, PercentPipe, TranslatePipe, SkeletonModule],
  templateUrl: './price-list-picker.component.html',
  styleUrl: './price-list-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriceListPickerComponent {
  options = input<SalePriceListOption[]>([]);
  selectedId = input<string | null>(null);
  loading = input<boolean>(false);
  /** Accessible name of the radio group, owned by the section heading around it. */
  labelledBy = input<string | null>(null);

  selected = output<string>();

  protected readonly tiles = computed(() => this.options().map(toTile));

  protected select(id: string): void {
    if (id !== this.selectedId()) {
      this.selected.emit(id);
    }
  }
}
