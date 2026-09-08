import { FormControl } from '@angular/forms';

export interface Customer {
  id: string;
  name: string;
  document: string;
  phone?: string | null;
  loyaltyTierId: string;
  loyaltyTierName: string;
  isActive: boolean;
  cashbackBalance: number;
}

export interface CustomerFormControls {
  id: FormControl<string | null>;
  name: FormControl<string | null>;
  document: FormControl<string | null>;
  phone: FormControl<string | null>;
  loyaltyTierId: FormControl<string | null>;
}
