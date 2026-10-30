import { CreateServiceVariantRequest } from './create-service-request.interface';

export interface UpdateServiceVariantRequest extends CreateServiceVariantRequest {
  variantId: string;
}

export interface UpdateServiceRequest {
  categoryId: string;
  name: string;
  sku: string;
  basePrice: number;
  baseCostPrice: number;
  description?: string;
  unitOfMeasure: string;
  nbs: string;
  issCode: string;
  version: number;
  variantsToAdd: CreateServiceVariantRequest[];
  variantsToUpdate: UpdateServiceVariantRequest[];

  // Fiscal
  taxProfileTemplateId: string;
  overrideTaxProfile: boolean;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
  supplierId?: string;
}
