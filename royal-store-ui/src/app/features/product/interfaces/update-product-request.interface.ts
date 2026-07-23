export interface UpdateProductRequest {
  categoryId: string;
  name: string;
  description?: string;
  sku?: string;
  price: number;
  costPrice?: number;
  stockQuantity: number;
  minStockQuantity?: number;
  isStockTracked: boolean;
  imageUrl?: string;
  unitOfMeasure: string;
  barcode?: string;
  ncm?: string;
  cest?: string;
  origin: number;
}
