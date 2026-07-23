export interface Product {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryName: string;
  name: string;
  description?: string;
  sku?: string;
  price: number;
  costPrice?: number;
  stockQuantity: number;
  minStockQuantity?: number;
  isStockTracked: boolean;
  isActive: boolean;
  imageUrl?: string;
  unitOfMeasure: string;
  barcode?: string;
  ncm?: string;
  cest?: string;
  origin: number;
}
