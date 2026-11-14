import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MessageModule } from 'primeng/message';
import { MethodProfit, PricingInput, PricingResult } from '../../interfaces/pricing.interface';

interface BreakdownLine {
  labelKey: string;
  rate: number | null;
  amount: number;
  highlight?: boolean;
}

/**
 * The suggested price and where every cent of it goes.
 *
 * Presentational: it receives the computed result and only formats it.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-pricing-result',
  imports: [CurrencyPipe, DecimalPipe, TranslatePipe, MessageModule],
  templateUrl: './pricing-result.component.html',
  styleUrl: './pricing-result.component.scss',
})
export class PricingResultComponent {
  readonly pricingInput = input.required<PricingInput>();
  readonly result = input.required<PricingResult>();
  readonly methods = input<MethodProfit[]>([]);
  /** Price the selected product is sold for today, when one is selected. */
  readonly currentPrice = input<number | null>(null);

  protected readonly lines = computed<BreakdownLine[]>(() => {
    const pricingInput = this.pricingInput();
    const result = this.result();

    return [
      { labelKey: 'pricing.result.cost', rate: null, amount: pricingInput.cost },
      { labelKey: 'pricing.result.tax', rate: pricingInput.taxRate, amount: result.taxAmount },
      { labelKey: 'pricing.result.card', rate: pricingInput.cardRate, amount: result.cardAmount },
      {
        labelKey: 'pricing.result.fixedExpenses',
        rate: pricingInput.fixedExpenseRate,
        amount: result.fixedExpenseAmount,
      },
      {
        labelKey: 'pricing.result.commission',
        rate: pricingInput.commissionRate,
        amount: result.commissionAmount,
      },
      {
        labelKey: 'pricing.result.profit',
        rate: pricingInput.marginRate,
        amount: result.profitAmount,
        highlight: true,
      },
    ];
  });

  /** Net margin the product really yields at today's price, with the same deductions. */
  protected readonly currentMargin = computed<number | null>(() => {
    const price = this.currentPrice();
    const pricingInput = this.pricingInput();

    if (!price || price <= 0 || !(pricingInput.cost > 0)) {
      return null;
    }

    const deductions = this.result().totalRate - pricingInput.marginRate;

    return ((price * (1 - deductions / 100) - pricingInput.cost) / price) * 100;
  });
}
