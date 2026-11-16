import { CreateProductVariantRequest } from './create-product-request.interface';
import { ProductMeasure } from './product.interface';
import { ProductPriceInput } from './price-list.interface';

export interface UpdateProductVariantRequest extends CreateProductVariantRequest {
  variantId: string;
}

export interface UpdateProductRequest {
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
  version: number;
  variantsToAdd: CreateProductVariantRequest[];
  variantsToUpdate: UpdateProductVariantRequest[];

  // Fiscal
  taxProfileTemplateId: string;
  overrideTaxProfile: boolean;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
  supplierId?: string;

  /** Replaces the product's price set: one sale price per price list. */
  prices?: ProductPriceInput[];
}
