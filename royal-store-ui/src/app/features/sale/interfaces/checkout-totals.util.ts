import { CheckoutTotals } from './checkout.interface';
import { Customer, loyaltyRateOf } from './customer.interface';

export interface CheckoutTotalsInput {
  subtotal: number;
  saleDiscount: number;
  tax: number;
  customer: Customer | null;
  /** Usable cashback for this customer, fetched from the balance endpoint (0 when none). */
  cashbackBalance: number;
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
  const {
    subtotal,
    saleDiscount,
    tax,
    customer,
    cashbackBalance,
    useCashback,
    paymentRate,
  } = input;

  const base = Math.max(subtotal - saleDiscount, 0);
  const loyaltyRate = loyaltyRateOf(customer);
  const loyaltyDiscount = round(base * loyaltyRate);
  const paymentDiscount = round(base * paymentRate);

  const owed = base - loyaltyDiscount - paymentDiscount;
  const cashbackApplied =
    useCashback && customer
      ? round(Math.min(Math.max(cashbackBalance, 0), Math.max(owed, 0)))
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
    total: round(subtotal - discountTotal),
  };
}
