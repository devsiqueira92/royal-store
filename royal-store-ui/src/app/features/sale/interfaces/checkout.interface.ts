import { PaymentEntry } from './payment.interface';
import { Customer } from './customer.interface';

export type FulfillmentMode = 'delivery' | 'pickup';

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

/** One deduction listed between the subtotal and the total of the checkout summary. */
export interface CheckoutDiscountLine {
  /** Translation key of the row label. */
  label: string;
  /** Rate shown next to the label (0..1) when the deduction is a percentage; null otherwise. */
  rate: number | null;
  amount: number;
}

/** Wire shape of `RegisterSaleAddress`; every field is optional on the API. */
export interface SaleAddress {
  street: string | null;
  number: string | null;
  city: string | null;
  zipCode: string | null;
}

export interface FulfillmentDetails {
  mode: FulfillmentMode;
  branchId: string | null;
  address: SaleAddress | null;
}

export interface CheckoutResult {
  /** Price list the sale is charged on; null → the store's default list. */
  priceListId: string | null;
  customer: Customer | null;
  useCashback: boolean;
  fulfillment: FulfillmentDetails;
  /** The chosen payment method, plus a separate line when cashback is redeemed alongside it. */
  payments: PaymentEntry[];
  totals: CheckoutTotals;
}
