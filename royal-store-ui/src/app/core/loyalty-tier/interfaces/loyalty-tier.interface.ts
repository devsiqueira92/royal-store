import { FormControl } from '@angular/forms';

export interface LoyaltyTier {
  id: string;
  name: string;
  cashbackPercentage: number;
  cashbackValidityDays: number;
  isActive: boolean;
}

export interface LoyaltyTierFormControls {
  name: FormControl<string | null>;
  cashbackPercentage: FormControl<number | null>;
  cashbackValidityDays: FormControl<number | null>;
}
