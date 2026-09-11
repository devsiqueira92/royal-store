import { CardPaymentDetails } from './payment.interface';
import { CheckoutTotals, FulfillmentDetails } from './checkout.interface';

/** Wire shape of `RegisterSaleCommand` — the backend recomputes pricing itself, so this only
 * states what was sold, who's buying, and how it's being paid and fulfilled. */
export interface RegisterSaleItemRequest {
  variantId: string;
  productId: string;
  quantity: number;
}

export interface RegisterSaleCustomerRef {
  id: string;
}

export interface RegisterSalePaymentRequest {
  methodId: string;
  fiscalType: number;
  amount: number;
  discountRate: number;
  card: CardPaymentDetails | null;
}

export interface RegisterSaleCheckoutRequest {
  customer: RegisterSaleCustomerRef | null;
  useCashback: boolean;
  fulfillment: FulfillmentDetails;
  payments: RegisterSalePaymentRequest[];
  totals: CheckoutTotals;
}

export interface RegisterSaleRequest {
  items: RegisterSaleItemRequest[];
  checkout: RegisterSaleCheckoutRequest;
}
