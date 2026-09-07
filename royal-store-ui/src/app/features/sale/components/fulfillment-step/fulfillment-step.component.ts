import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectButtonModule } from 'primeng/selectbutton';
import {
  FulfillmentMode,
  StoreBranch,
} from '../../interfaces/checkout.interface';
import { PaymentOption } from '../../interfaces/payment.interface';

const MODE_OPTIONS: PaymentOption<FulfillmentMode>[] = [
  { label: 'sale.checkout.fulfillment.delivery', value: 'delivery' },
  { label: 'sale.checkout.fulfillment.pickup', value: 'pickup' },
];

@Component({
  selector: 'app-fulfillment-step',
  imports: [ReactiveFormsModule, TranslatePipe, SelectButtonModule],
  templateUrl: './fulfillment-step.component.html',
  styleUrl: './fulfillment-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FulfillmentStepComponent {
  mode = input.required<FormControl<FulfillmentMode>>();
  branches = input.required<StoreBranch[]>();
  selectedBranchId = input<string | null>(null);
  isPickup = input<boolean>(false);

  branchSelected = output<string>();

  protected readonly modeOptions = MODE_OPTIONS;
}
