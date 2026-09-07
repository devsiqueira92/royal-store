import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import {
  PaymentOption,
  SplitPaymentFormArray,
  SplitPaymentMethod,
} from '../../interfaces/payment.interface';

const METHOD_OPTIONS: PaymentOption<SplitPaymentMethod>[] = [
  { label: 'sale.current.payment.cash', value: 'cash' },
  { label: 'sale.current.payment.debit', value: 'debit' },
  { label: 'sale.current.payment.credit', value: 'credit' },
  { label: 'sale.current.payment.pix', value: 'pix' },
];

@Component({
  selector: 'app-split-payment-form',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    TranslatePipe,
    InputNumberModule,
    SelectModule,
  ],
  templateUrl: './split-payment-form.component.html',
  styleUrl: './split-payment-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SplitPaymentFormComponent {
  protected readonly methodOptions = METHOD_OPTIONS;

  form = input.required<SplitPaymentFormArray>();
  remaining = input<number>(0);

  addPayment = output<void>();
  removePayment = output<number>();
}
