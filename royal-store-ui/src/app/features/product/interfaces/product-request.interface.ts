export interface ProductRequest {
  id?: string | null;
  categoryId?: string;
  name?: string;
  sku?: string;
  barcode?: string;
  price?: number;
  costPrice?: number;
  stockQuantity?: number;
  sizes?: string[];
  isActive?: boolean;
  imageUrl?: string;
}
