import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectablePaymentMethod } from '../../interfaces/payment-method-option.const';

@Component({
  selector: 'app-payment-method-list',
  imports: [CurrencyPipe, TranslatePipe],
  templateUrl: './payment-method-list.component.html',
  styleUrl: './payment-method-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentMethodListComponent {
  methods = input.required<SelectablePaymentMethod[]>();
  selected = input<string | null>(null);
  /** Cart base the discount preview is calculated over. */
  base = input<number>(0);

  methodSelected = output<string>();

  protected discountAmount(discount: number): number {
    return Math.round(this.base() * (discount / 100) * 100) / 100;
  }
}
