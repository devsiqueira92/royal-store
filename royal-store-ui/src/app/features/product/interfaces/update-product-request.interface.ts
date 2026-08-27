import { CreateProductVariantRequest } from './create-product-request.interface';

export interface UpdateProductVariantRequest extends CreateProductVariantRequest {
  variantId: string;
}

export interface UpdateProductRequest {
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
}
