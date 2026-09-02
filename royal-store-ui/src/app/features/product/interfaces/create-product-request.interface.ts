export interface CreateProductVariantRequest {
  size?: string;
  color?: string;
  sku?: string;
  barcode?: string;
  price: number;
  costPrice?: number;
  promoPrice?: number;
  stockQuantity: number;
  minStockQuantity?: number;
  netWeight?: number;
  grossWeight?: number;
  width?: number;
  height?: number;
  length?: number;
}

export interface CreateProductRequest {
  itemType: number;
  categoryId: string;
  name: string;
  referenceCode: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: number;
  variants?: CreateProductVariantRequest[];
  tagIds?: string[];

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
