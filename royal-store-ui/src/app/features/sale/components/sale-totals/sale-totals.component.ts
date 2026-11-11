import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { SaleDetails } from '../../interfaces/sale-details.interface';

export type SaleTotals = Pick<
  SaleDetails,
  | 'subtotal'
  | 'discountAmount'
  | 'loyaltyDiscountApplied'
  | 'paymentDiscountApplied'
  | 'taxAmount'
  | 'cashbackApplied'
  | 'totalAmount'
>;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-sale-totals',
  imports: [CurrencyPipe, TranslatePipe],
  templateUrl: './sale-totals.component.html',
  styleUrl: './sale-totals.component.scss',
})
export class SaleTotalsComponent {
  readonly totals = input.required<SaleTotals>();
}
