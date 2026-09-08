import { Customer } from '@app/features/customer/interfaces/customer.interface';

export type { Customer } from '@app/features/customer/interfaces/customer.interface';

/**
 * The API stores the tier cashback as a whole number ("4" means 4%). Checkout math works with the
 * 0.04 fraction, so every consumer goes through this helper instead of dividing inline.
 */
export function loyaltyRateOf(customer: Customer | null): number {
  return customer ? customer.loyaltyTierCashbackPercentage / 100 : 0;
}
