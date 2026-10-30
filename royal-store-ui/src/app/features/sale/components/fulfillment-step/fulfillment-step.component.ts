import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { FormGroup } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FulfillmentFormControls } from '../../interfaces/checkout.interface';
import { CustomerAddress } from '@app/features/customer/interfaces/customer-address.interface';
import { formatAddressLine } from '@app/features/customer/utils/customer-address-form.util';
import { Options } from '@app/shared/interceptors/options.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { SelectFieldComponent } from '@app/shared/components/form-fields/select-field/select-field.component';

@Component({
  selector: 'app-fulfillment-step',
  imports: [TranslatePipe, InputFieldComponent, SelectFieldComponent],
  templateUrl: './fulfillment-step.component.html',
  styleUrl: './fulfillment-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FulfillmentStepComponent {
  fulfillmentForm = input.required<FormGroup<FulfillmentFormControls>>();
  hasCustomer = input<boolean>(false);
  addresses = input<CustomerAddress[]>([]);

  protected readonly addressOptions = computed<Options[]>(() =>
    this.addresses().map(address => ({
      code: address.id,
      description: formatAddressLine(address),
    })),
  );
}
