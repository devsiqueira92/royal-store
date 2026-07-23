import { SaleItemRequest } from './sale-item-request.interface';
import { SalePaymentRequest } from './sale-payment-request.interface';

export interface SaleSyncRequest {
  id: string;
  cashShiftId: string;
  totalAmount: number;
  discountAmount: number;
  soldAt: string;
  items: SaleItemRequest[];
  payments: SalePaymentRequest[];
}
