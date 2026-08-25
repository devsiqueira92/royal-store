import { FormControl } from '@angular/forms';

export interface Product {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
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

export interface ProductFormControls {
  id: FormControl<string | null>;
  categoryId: FormControl<string | null>;
  name: FormControl<string | null>;
  sku: FormControl<string | null>;
  barcode: FormControl<string | null>;
  price: FormControl<number | null>;
  costPrice: FormControl<number | null>;
  stockQuantity: FormControl<number | null>;
  sizes: FormControl<string[] | null>;
  isActive: FormControl<boolean | null>;
}
