import { FormArray, FormControl, FormGroup } from '@angular/forms';

export interface ProductCategoryOption {
  id: string;
  name: string;
  color?: string;
}

/** 0 = Physical product, 1 = Service */
export type ItemType = 0 | 1;

export interface ProductMeasure {
  netWeight?: number | null;
  grossWeight?: number | null;
  width?: number | null;
  height?: number | null;
  length?: number | null;
}

export interface Product {
  id: string;
  tenantId: string;
  type?: number;
  categoryId: string;
  categoryName: string;
  basePrice: number;
  baseCostPrice?: number;
  categoryColor?: string;
  name: string;
  sku: string;
  description?: string;
  unitOfMeasure: string;
  stockTrack: boolean;
  ncm?: string;
  cest?: string;
  origin: string;
  measure: ProductMeasure;
  version: number;
  isActive: boolean;
  coverPhotoUrl?: string;
  photos: ProductPhoto[];
  variants: ProductVariant[];
  supplierId?: string | null;
  supplierName?: string | null;

  // Fiscal
  taxProfileTemplateId?: string;
  nbs?: string;
  issCode?: string;
  overrideTaxProfile?: boolean;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
}

export interface ProductPhoto {
  id: string;
  url: string;
  isCover: boolean;
}

export interface ProductVariant {
  id: string;
  size?: string;
  color?: string;
  optionName?: string;
  sku?: string;
  barcode?: string;
  price?: number | null;
  costPrice?: number | null;
  stockQuantity?: number | null;
  minStockQuantity?: number | null;
}

export interface ProductVariantFormControls {
  id: FormControl<string | null>;
  size: FormControl<string | null>;
  color: FormControl<string | null>;
  optionName: FormControl<string | null>;
  sku: FormControl<string | null>;
  barcode: FormControl<string | null>;
  price: FormControl<number | null>;
  costPrice: FormControl<number | null>;
  stockQuantity: FormControl<number | null>;
  minStockQuantity: FormControl<number | null>;
}

export interface ProductFormControls {
  id: FormControl<string | null>;
  type: FormControl<number>;
  categoryId: FormControl<string | null>;
  name: FormControl<string | null>;
  sku: FormControl<string | null>;
  description: FormControl<string | null>;
  basePrice: FormControl<number | null>;
  baseCostPrice: FormControl<number | null>;
  unitOfMeasure: FormControl<string | null>;
  isActive: FormControl<boolean | null>;
  stockTrack: FormControl<boolean | null>;
  supplierId: FormControl<string | null>;
  // Taxes
  taxProfileTemplateId: FormControl<string | null>;
  ncm: FormControl<string | null>;
  origin: FormControl<string | null>;
  nbs: FormControl<string | null>;
  issCode: FormControl<string | null>;

  // Advanced fiscal settings
  overrideTaxProfile: FormControl<boolean>;
  cest: FormControl<string | null>;
  defaultCfop: FormControl<string | null>;
  icmsCst: FormControl<string | null>;
  pisCofinsCst: FormControl<string | null>;
  ivaCst: FormControl<string | null>;
  isSelectiveTax: FormControl<boolean>;

  netWeight: FormControl<number | null>;
  grossWeight: FormControl<number | null>;
  width: FormControl<number | null>;
  height: FormControl<number | null>;
  length: FormControl<number | null>;

  variants: FormArray<FormGroup<ProductVariantFormControls>>;
}
