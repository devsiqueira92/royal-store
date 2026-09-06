import { ProductMeasure } from './product.interface';

export interface CreateProductVariantRequest {
  size?: string;
  color?: string;
  optionName?: string;
  sku?: string;
  barcode?: string;
  price?: number;
  costPrice?: number;
  stockQuantity?: number;
  minStockQuantity?: number;
}

export interface CreateProductRequest {
  categoryId: string;
  name: string;
  sku: string;
  stockTrack: boolean;
  basePrice: number;
  baseCostPrice: number;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: string;
  measure?: ProductMeasure;
  variants: CreateProductVariantRequest[];

  // Fiscal
  taxProfileTemplateId: string;
  overrideTaxProfile: boolean;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
}
