export interface CreateServiceVariantRequest {
  optionName: string;
}

export interface CreateServiceRequest {
  categoryId: string;
  name: string;
  sku: string;
  /** The service's one price; the API stores it on the default price list. */
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
