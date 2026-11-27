import { ProductMeasure } from './product.interface';
import { ProductPriceInput } from './price-list.interface';

export interface CreateProductVariantRequest {
  size?: string;
  color?: string;
  optionName?: string;
  sku?: string;
  barcode?: string;
  stockQuantity?: number;
  minStockQuantity?: number;
}

export interface CreateProductRequest {
  categoryId: string;
  name: string;
  sku: string;
  stockTrack: boolean;
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
  supplierId?: string;

  /** One sale price per price list; the default list's is required. */
  prices: ProductPriceInput[];

  /** Optional cost composition to link to the product. */
  costCompositionId?: string | null;
}
