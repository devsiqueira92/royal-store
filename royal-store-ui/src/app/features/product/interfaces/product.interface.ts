import { FormArray, FormControl, FormGroup } from '@angular/forms';

export interface ProductCategoryOption {
  id: string;
  name: string;
  color?: string;
}

/** 0 = Physical product, 1 = Service */
export type ItemType = 0 | 1;

export interface Product {
  id: string;
  tenantId: string;
  itemType?: number;
  categoryId: string;
  categoryName: string;
  price: number;
  costPrice?: number;
  categoryColor?: string;
  name: string;
  sku: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: string;
  version: number;
  isActive: boolean;
  coverPhotoUrl?: string;
  photos: ProductPhoto[];
  variants: ProductVariant[];
  // tags: ProductTag[];

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

export interface ProductPhoto {
  id: string;
  url: string;
  isCover: boolean;
}

export interface ProductVariant {
  id: string;
  size?: string;
  color?: string;
  sku?: string;
  barcode?: string;
  price: number;
  costPrice?: number;
  stockQuantity: number;
  minStockQuantity?: number;
  isStockTracked: boolean;
  netWeight?: number;
  grossWeight?: number;
  width?: number;
  height?: number;
  length?: number;
}

// export interface ProductTag {
//   categoryId: string;
//   categoryName: string;
//   categoryColor?: string;
// }

export interface ProductVariantFormControls {
  id: FormControl<string | null>;
  size: FormControl<string | null>;
  color: FormControl<string | null>;
  sku: FormControl<string | null>;
  barcode: FormControl<string | null>;
  price: FormControl<number | null>;
  costPrice: FormControl<number | null>;
  stockQuantity: FormControl<number | null>;
  minStockQuantity: FormControl<number | null>;
}

export interface ProductFormControls {
  id: FormControl<string | null>;
  itemType: FormControl<number>;
  categoryId: FormControl<string | null>;
  name: FormControl<string | null>;
  sku: FormControl<string | null>;
  description: FormControl<string | null>;
  price: FormControl<number | null>;
  costPrice: FormControl<number | null>;
  unitOfMeasure: FormControl<string | null>;
  isActive: FormControl<boolean | null>;
  isStockTracked: FormControl<boolean | null>;
  // Taxes
  taxProfileTemplate: FormControl<string | null>;
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
