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
 * The payment-method rate runs over the base already net of the manual discount. The loyalty rate
 * is the tier's cashback percentage — what the customer earns back — so it is informational only
 * and never reduces the total. Redeemed cashback is capped by what is still owed, so a large
 * balance can never push the sale below zero.
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
  const paymentDiscount = round(base * paymentRate);

  const owed = base - paymentDiscount;
  const cashbackApplied =
    useCashback && customer
      ? round(Math.min(Math.max(cashbackBalance, 0), Math.max(owed, 0)))
      : 0;

  const discountTotal = round(
    saleDiscount + paymentDiscount + cashbackApplied,
  );

  return {
    subtotal,
    base,
    saleDiscount,
    loyaltyRate,
    loyaltyDiscount: 0,
    paymentRate,
    paymentDiscount,
    cashbackApplied,
    discountTotal,
    tax,
    total: round(subtotal - discountTotal),
  };
}

/**
 * Cashback the customer will earn on this sale, mirroring the API's accrual: the tier rate over
 * the subtotal net of the manual discount and of any cashback redeemed on this same sale.
 * Informational only — it is credited after the sale and never changes what is charged now.
 */
export function estimateCashbackEarned(totals: CheckoutTotals): number {
  const earningBase = Math.max(totals.base - totals.cashbackApplied, 0);
  return round(earningBase * totals.loyaltyRate);
}
