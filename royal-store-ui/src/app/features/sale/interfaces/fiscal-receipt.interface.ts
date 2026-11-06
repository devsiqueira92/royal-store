import { SaleLineItem } from './sale-line-item.interface';
import { CheckoutTotals } from './checkout.interface';

/** Everything the printable cupom needs, snapshotted at confirmation time so it stays
 * stable even if the cart or selection it was built from changes afterwards. */
export interface FiscalReceiptData {
  saleId: string;
  issuedAt: Date;
  items: SaleLineItem[];
  totals: CheckoutTotals;
  paymentLabel: string;
  customerName: string | null;
}
