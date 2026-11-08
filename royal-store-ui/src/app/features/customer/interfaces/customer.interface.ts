import { FormArray, FormControl, FormGroup } from '@angular/forms';
import {
  CustomerAddress,
  CustomerAddressFormControls,
} from './customer-address.interface';

export interface Customer {
  id: string;
  name: string;
  document?: string | null;
  phone?: string | null;
  birthDate?: string | null;
  loyaltyTierId: string;
  loyaltyTierName: string;
  loyaltyTierCashbackPercentage: number;
  isActive: boolean;
  // Null until the customer earns cashback for the first time.
  cashbackBalance: number | null;
  // Omitted from the list response.
  addresses?: CustomerAddress[];
}

export interface CustomerFormControls {
  id: FormControl<string | null>;
  name: FormControl<string | null>;
  document: FormControl<string | null>;
  phone: FormControl<string | null>;
  birthDate: FormControl<string | null>;
  loyaltyTierId: FormControl<string | null>;
  addresses: FormArray<FormGroup<CustomerAddressFormControls>>;
}
