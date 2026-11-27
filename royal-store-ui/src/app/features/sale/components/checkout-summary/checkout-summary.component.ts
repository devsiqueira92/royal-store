import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectButtonModule } from 'primeng/selectbutton';
import { cashChange } from '../../interfaces/cash-change.util';
import {
  CheckoutDiscountLine,
  CheckoutTotals,
} from '../../interfaces/checkout.interface';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { DiscountMode } from '../../interfaces/sale-totals.interface';

interface DiscountModeOption {
  label: string;
  value: DiscountMode;
}

const DISCOUNT_MODE_OPTIONS: DiscountModeOption[] = [
  { label: 'R$', value: 'amount' },
  { label: '%', value: 'percent' },
];

/** Lines listed before the "show all" toggle, so a long cart never pushes the total away. */
const COLLAPSED_ITEMS = 4;

/**
 * The order as it will be charged: lines priced on the chosen list, the manual discount the
 * cashier can give here, every other deduction, the approximate taxes and the cashback the
 * customer earns. Projected content renders below the total (the page's actions).
 */
@Component({
  selector: 'app-checkout-summary',
  imports: [
    CurrencyPipe,
    PercentPipe,
    FormsModule,
    TranslatePipe,
    InputNumberModule,
    SelectButtonModule,
  ],
  templateUrl: './checkout-summary.component.html',
  styleUrl: './checkout-summary.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutSummaryComponent {
  items = input<SaleLineItem[]>([]);
  totals = input.required<CheckoutTotals>();
  /** Manual discount as typed, in `discountMode` units; its amount comes in `totals.saleDiscount`. */
  discountValue = input<number>(0);
  discountMode = input<DiscountMode>('amount');
  /** Deductions to list between subtotal and total, already filtered and ordered by the page. */
  discountLines = input<CheckoutDiscountLine[]>([]);
  /** What the customer saves on this sale (shown under the total when positive). */
  savings = input<number>(0);
  priceListName = input<string | null>(null);
  /** Cashback this sale credits to the customer (informational, not deducted). */
  cashbackEarned = input<number>(0);
  /** Credit-card installments; 1 for every other method. */
  installments = input<number>(1);
  /** Cash handed over by the customer; null unless paying in cash and typed in. */
  cashReceived = input<number | null>(null);

  discountChange = output<{ value: number; mode: DiscountMode }>();

  protected readonly modeOptions = DISCOUNT_MODE_OPTIONS;
  protected readonly expanded = signal(false);

  protected readonly unitCount = computed(() =>
    this.items().reduce((total, item) => total + item.quantity, 0),
  );
  protected readonly visibleItems = computed(() =>
    this.expanded() ? this.items() : this.items().slice(0, COLLAPSED_ITEMS),
  );
  protected readonly hiddenCount = computed(() =>
    Math.max(this.items().length - COLLAPSED_ITEMS, 0),
  );
  protected readonly installmentAmount = computed(
    () =>
      Math.round((this.totals().total / Math.max(this.installments(), 1)) * 100) /
      100,
  );

  protected readonly change = computed(() =>
    cashChange(this.cashReceived(), this.totals().total),
  );

  protected changeDiscountValue(value: number | null): void {
    this.discountChange.emit({
      value: value && value > 0 ? value : 0,
      mode: this.discountMode(),
    });
  }

  protected changeDiscountMode(mode: DiscountMode): void {
    this.discountChange.emit({ value: this.discountValue(), mode });
  }

  protected toggleItems(): void {
    this.expanded.update(expanded => !expanded);
  }
}
