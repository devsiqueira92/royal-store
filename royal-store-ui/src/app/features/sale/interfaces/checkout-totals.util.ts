import { CheckoutTotals } from './checkout.interface';
import { Customer, TIER_DISCOUNT_RATE } from './customer.interface';

export interface CheckoutTotalsInput {
  subtotal: number;
  saleDiscount: number;
  tax: number;
  customer: Customer | null;
  useCashback: boolean;
}

const round = (value: number): number => Math.round(value * 100) / 100;

/**
 * Loyalty runs over the base already net of the manual discount; cashback is capped by what is
 * still owed so a large balance can never push the sale below zero.
 */
export function buildCheckoutTotals(
  input: CheckoutTotalsInput,
): CheckoutTotals {
  const { subtotal, saleDiscount, tax, customer, useCashback } = input;

  const base = Math.max(subtotal - saleDiscount, 0);
  const loyaltyRate = customer ? TIER_DISCOUNT_RATE[customer.tier] : 0;
  const loyaltyDiscount = round(base * loyaltyRate);

  const cashbackApplied =
    useCashback && customer
      ? round(Math.min(customer.cashbackBalance, base - loyaltyDiscount))
      : 0;

  const discountTotal = round(saleDiscount + loyaltyDiscount + cashbackApplied);

  return {
    subtotal,
    saleDiscount,
    loyaltyRate,
    loyaltyDiscount,
    cashbackApplied,
    discountTotal,
    tax,
    total: round(subtotal - discountTotal + tax),
  };
}
