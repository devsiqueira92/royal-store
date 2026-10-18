import { FormControl } from '@angular/forms';

export interface CustomerAddress {
  id: string;
  zipCode: string | null;
  street: string | null;
  number: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  complement: string | null;
  additionalInfo: string | null;
  isDefaultDelivery: boolean;
  isActive: boolean;
}

export interface CustomerAddressFormControls {
  id: FormControl<string | null>;
  zipCode: FormControl<string | null>;
  street: FormControl<string | null>;
  number: FormControl<string | null>;
  neighborhood: FormControl<string | null>;
  city: FormControl<string | null>;
  state: FormControl<string | null>;
  complement: FormControl<string | null>;
  additionalInfo: FormControl<string | null>;
  isDefaultDelivery: FormControl<boolean>;
  isActive: FormControl<boolean>;
}
