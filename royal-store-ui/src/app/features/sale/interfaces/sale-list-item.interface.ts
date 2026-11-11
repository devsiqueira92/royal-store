export interface SaleListItem {
  id: string;
  sellerName: string | null;
  subtotal: number;
  discountAmount: number;
  customerName: string | null;
  soldAt: string;
  fulfillmentMode: string;
}
