export type CustomerTier = 'standard' | 'silver' | 'gold' | 'vip';

export interface Customer {
  id: string;
  name: string;
  phone: string;
  document: string;
  tier: CustomerTier;
  cashbackBalance: number;
}

/** Loyalty discount granted by tier, applied over the cart base. */
export const TIER_DISCOUNT_RATE: Record<CustomerTier, number> = {
  standard: 0,
  silver: 0.03,
  gold: 0.05,
  vip: 0.1,
};

export const TIER_LABEL: Record<CustomerTier, string> = {
  standard: 'sale.checkout.customer.tier.standard',
  silver: 'sale.checkout.customer.tier.silver',
  gold: 'sale.checkout.customer.tier.gold',
  vip: 'sale.checkout.customer.tier.vip',
};
