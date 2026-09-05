/** 0 = targets Product, 1 = targets Service */
export type TaxProfileTemplateTargetItemType = 0 | 1;

export interface TaxProfileTemplate {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  targetItemType: TaxProfileTemplateTargetItemType;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax: boolean;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
  updatedBy?: string;
}
