import { FormArray, FormControl, FormGroup } from '@angular/forms';

export interface ProductCategoryOption {
  id: string;
  name: string;
  color?: string;
}

export interface Product {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
  name: string;
  referenceCode: string;
  description?: string;
  unitOfMeasure: string;
  ncm?: string;
  cest?: string;
  origin: number;
  version: number;
  isActive: boolean;
  coverPhotoUrl?: string;
  photos: ProductPhoto[];
  variants: ProductVariant[];
  tags: ProductTag[];
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
}

export interface ProductTag {
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
}

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
  isStockTracked: FormControl<boolean | null>;
}

export interface ProductFormControls {
  id: FormControl<string | null>;
  categoryId: FormControl<string | null>;
  name: FormControl<string | null>;
  referenceCode: FormControl<string | null>;
  description: FormControl<string | null>;
  unitOfMeasure: FormControl<string | null>;
  ncm: FormControl<string | null>;
  cest: FormControl<string | null>;
  isActive: FormControl<boolean | null>;
  price: FormControl<number | null>;
  costPrice: FormControl<number | null>;
  minStockQuantity: FormControl<number | null>;
  variants: FormArray<FormGroup<ProductVariantFormControls>>;
}
