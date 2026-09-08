import { FormControl } from '@angular/forms';

export interface PaymentMethod {
  id: string;
  paymentMethod: string;
  paymentType: string;
  discount: number;
  active: boolean;
}

export interface PaymentMethodFormControls {
  discount: FormControl<number | null>;
}
