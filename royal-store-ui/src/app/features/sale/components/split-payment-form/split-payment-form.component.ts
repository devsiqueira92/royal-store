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
import { SplitPaymentFormArray } from '../../interfaces/payment.interface';
import { SelectablePaymentMethod } from '../../interfaces/payment-method-option.const';

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
  form = input.required<SplitPaymentFormArray>();
  /** Real catalog methods a split row can settle with — the split option itself is excluded. */
  methods = input.required<SelectablePaymentMethod[]>();
  remaining = input<number>(0);

  addPayment = output<void>();
  removePayment = output<number>();
}
