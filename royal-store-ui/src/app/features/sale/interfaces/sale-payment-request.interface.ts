import { PaymentMethod } from './payment-method.enum';

export interface SalePaymentRequest {
  method: PaymentMethod;
  amount: number;
}
