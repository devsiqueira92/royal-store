export interface SaleItemRequest {
  productId: string;
  productName: string;
  productCode?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  ncm: string;
  cest?: string;
}
