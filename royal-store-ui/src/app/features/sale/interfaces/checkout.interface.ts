import { FormControl, FormGroup } from '@angular/forms';
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

export interface SaleAddressFormControls {
  zipCode: FormControl<string | null>;
  street: FormControl<string | null>;
  number: FormControl<string | null>;
  city: FormControl<string | null>;
}

export interface FulfillmentFormControls {
  // Which of the selected customer's addresses fills `address`; never sent.
  customerAddressId: FormControl<string | null>;
  address: FormGroup<SaleAddressFormControls>;
}

export interface CheckoutResult {
  customer: Customer | null;
  useCashback: boolean;
  fulfillment: FulfillmentDetails;
  /** The chosen payment method, plus a separate line when cashback is redeemed alongside it. */
  payments: PaymentEntry[];
  totals: CheckoutTotals;
}
