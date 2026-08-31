export interface SaleLineItem {
  variantId: string;
  productId: string;
  name: string;
  variantLabel: string;
  variantSize?: string;
  variantColor?: string;
  unitPrice: number;
  quantity: number;
  coverPhotoUrl?: string;
}
