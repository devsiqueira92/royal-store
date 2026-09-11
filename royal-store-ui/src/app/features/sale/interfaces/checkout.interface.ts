import { PaymentEntry } from './payment.interface';
import { Customer } from './customer.interface';

export type FulfillmentMode = 'delivery' | 'pickup';

export interface StoreBranch {
  id: string;
  name: string;
  address: string;
  openingHours: string;
}

/** Money figures presented in the checkout footer, before the sale is registered. */
export interface CheckoutTotals {
  subtotal: number;
  base: number;
  saleDiscount: number;
  loyaltyRate: number;
  loyaltyDiscount: number;
  paymentRate: number;
  paymentDiscount: number;
  cashbackApplied: number;
  discountTotal: number;
  tax: number;
  total: number;
}

export interface FulfillmentDetails {
  mode: FulfillmentMode;
  branchId: string | null;
}

export interface CheckoutResult {
  customer: Customer | null;
  useCashback: boolean;
  fulfillment: FulfillmentDetails;
  /** The chosen payment method, plus a separate line when cashback is redeemed alongside it. */
  payments: PaymentEntry[];
  totals: CheckoutTotals;
}
