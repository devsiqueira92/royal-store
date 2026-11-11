export interface ReceiptItem {
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  sku: string | null;
  taxPercentage: number;
  taxAmount: number;
}
