export interface ProductVariantRequest {
  id?: string | null;
  size?: string;
  color?: string;
  sku?: string;
  barcode?: string;
  price: number;
  costPrice?: number;
  stockQuantity: number;
  minStockQuantity?: number;
  isStockTracked: boolean;
}

export interface ProductRequest {
  id?: string | null;
  categoryId?: string;
  name?: string;
  description?: string;
  unitOfMeasure?: string;
  ncm?: string;
  cest?: string;
  origin?: number;
  isActive?: boolean;
  variants: ProductVariantRequest[];
  tagIds: string[];
  photos: File[];
}
