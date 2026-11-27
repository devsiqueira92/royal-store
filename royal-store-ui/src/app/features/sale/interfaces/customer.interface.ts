import { CustomerSearchItem } from '@app/features/customer/interfaces/customer-search-item.interface';

/** The buyer as checkout knows them: the search result the cashier picked. */
export type Customer = CustomerSearchItem;

/**
 * The API stores the tier cashback as a whole number ("4" means 4%). Checkout math works with the
 * 0.04 fraction, so every consumer goes through this helper instead of dividing inline.
 */
export function loyaltyRateOf(
  customer: Pick<Customer, 'loyaltyTierCashbackPercentage'> | null,
): number {
  return customer ? customer.loyaltyTierCashbackPercentage / 100 : 0;
}
