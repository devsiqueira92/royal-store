import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { CurrencyPipe, PercentPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { Customer, TIER_LABEL } from '../../interfaces/customer.interface';
import { PaymentState } from '../../interfaces/payment.interface';
import { PaymentPanelComponent } from '../payment-panel/payment-panel.component';

@Component({
  selector: 'app-payment-step',
  imports: [
    CurrencyPipe,
    PercentPipe,
    TranslatePipe,
    SwitchComponent,
    PaymentPanelComponent,
  ],
  templateUrl: './payment-step.component.html',
  styleUrl: './payment-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentStepComponent {
  customer = input<Customer | null>(null);
  loyaltyRate = input<number>(0);
  useCashback = input<boolean>(false);
  total = input<number>(0);
  base = input<number>(0);

  cashbackToggled = output<boolean>();
  paymentChange = output<PaymentState>();

  protected readonly tierLabel = TIER_LABEL;
}
