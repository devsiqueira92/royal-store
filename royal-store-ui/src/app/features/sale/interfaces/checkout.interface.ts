import { PaymentDetails } from './payment.interface';
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
  saleDiscount: number;
  loyaltyRate: number;
  loyaltyDiscount: number;
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
  payment: PaymentDetails;
  totals: CheckoutTotals;
}
