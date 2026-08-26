export interface CreateProductVariantRequest {
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

export interface CreateProductRequest {
  categoryId: string;
  name: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: number;
  variants?: CreateProductVariantRequest[];
  tagIds?: string[];
}
