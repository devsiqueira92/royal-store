export interface CreateServiceVariantRequest {
  optionName: string;
  price?: number;
  costPrice?: number;
}

export interface CreateServiceRequest {
  categoryId: string;
  name: string;
  sku: string;
  basePrice: number;
  baseCostPrice: number;
  description?: string;
  unitOfMeasure: string;
  nbs: string;
  issCode: string;
  variants: CreateServiceVariantRequest[];

  // Fiscal
  taxProfileTemplateId: string;
  overrideTaxProfile: boolean;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax?: boolean;
  supplierId?: string;
}
