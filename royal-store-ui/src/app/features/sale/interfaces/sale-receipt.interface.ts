import { ReceiptItem } from './receipt-item.interface';
import { ReceiptPayment } from './receipt-payment.interface';

export interface SaleReceipt {
  receiptCode: string;
  soldAt: string;
  customerName: string | null;
  storeName: string | null;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  items: ReceiptItem[];
  payments: ReceiptPayment[];
}
