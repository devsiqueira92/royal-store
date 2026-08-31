export interface PosProduct {
  productId: string;
  variantId: string;
  productName: string;
  category: string;
  categoryColor?: string;
  price: number;
  coverImage?: string;
  variantSize?: string;
  variantColor?: string;
  isStockTracked: boolean;
  stockQuantity: number;
  soldOut: boolean;
}
