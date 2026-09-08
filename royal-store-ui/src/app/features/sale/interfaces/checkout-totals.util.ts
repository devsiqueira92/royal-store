import { CheckoutTotals } from './checkout.interface';
import { Customer, TIER_DISCOUNT_RATE } from './customer.interface';

export interface CheckoutTotalsInput {
  subtotal: number;
  saleDiscount: number;
  tax: number;
  customer: Customer | null;
  useCashback: boolean;
  paymentRate: number;
}

const round = (value: number): number => Math.round(value * 100) / 100;

/**
 * Loyalty and payment-method rates both run over the base already net of the manual discount, so
 * they add up the way the checkout presents them ("12% = 10% loyalty + 2% payment"). Cashback is
 * capped by what is still owed, so a large balance can never push the sale below zero.
 */
export function buildCheckoutTotals(
  input: CheckoutTotalsInput,
): CheckoutTotals {
  const { subtotal, saleDiscount, tax, customer, useCashback, paymentRate } =
    input;

  const base = Math.max(subtotal - saleDiscount, 0);
  const loyaltyRate = customer ? TIER_DISCOUNT_RATE[customer.tier] : 0;
  const loyaltyDiscount = round(base * loyaltyRate);
  const paymentDiscount = round(base * paymentRate);

  const owed = base - loyaltyDiscount - paymentDiscount;
  const cashbackApplied =
    useCashback && customer
      ? round(Math.min(customer.cashbackBalance, Math.max(owed, 0)))
      : 0;

  const discountTotal = round(
    saleDiscount + loyaltyDiscount + paymentDiscount + cashbackApplied,
  );

  return {
    subtotal,
    base,
    saleDiscount,
    loyaltyRate,
    loyaltyDiscount,
    paymentRate,
    paymentDiscount,
    cashbackApplied,
    discountTotal,
    tax,
    total: round(subtotal - discountTotal + tax),
  };
}
