import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

/** The cart totals at the POS: subtotal, approximate taxes and total. Discounts are given at checkout. */
@Component({
  selector: 'app-sale-summary',
  imports: [CurrencyPipe, DecimalPipe, TranslatePipe],
  templateUrl: './sale-summary.component.html',
  styleUrl: './sale-summary.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaleSummaryComponent {
  subtotal = input<number>(0);
  taxPercent = input<number>(0);
  tax = input<number>(0);
  total = input<number>(0);
}
