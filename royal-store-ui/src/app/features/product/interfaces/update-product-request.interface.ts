import { CreateProductVariantRequest } from './create-product-request.interface';

export interface UpdateProductVariantRequest extends CreateProductVariantRequest {
  variantId: string;
}

export interface UpdateProductRequest {
  itemType: number;
  categoryId: string;
  name: string;
  referenceCode: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: number;
  version: number;
  variantsToAdd: CreateProductVariantRequest[];
  variantsToUpdate: UpdateProductVariantRequest[];

  // Fiscal
  taxProfileTemplate?: string;
  nbs?: string;
  issCode?: string;
  overrideTaxProfile?: boolean;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
}
