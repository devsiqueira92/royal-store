import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { CheckoutTotals } from '../../interfaces/checkout.interface';

@Component({
  selector: 'app-checkout-summary',
  imports: [CurrencyPipe, PercentPipe, TranslatePipe, AccordionModule],
  templateUrl: './checkout-summary.component.html',
  styleUrl: './checkout-summary.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutSummaryComponent {
  totals = input.required<CheckoutTotals>();
}
