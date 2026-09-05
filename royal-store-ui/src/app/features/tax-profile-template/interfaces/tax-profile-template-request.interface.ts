import { TaxProfileTemplateTargetItemType } from './tax-profile-template.interface';

export interface CreateTaxProfileTemplateRequest {
  name: string;
  description?: string;
  targetItemType: TaxProfileTemplateTargetItemType;
  defaultCfop?: string;
  icmsCst?: string;
  pisCofinsCst?: string;
  ivaCst?: string;
  isSelectiveTax: boolean;
  isDefault: boolean;
}

// targetItemType is immutable once set — not part of the update contract.
export type UpdateTaxProfileTemplateRequest = Omit<
  CreateTaxProfileTemplateRequest,
  'targetItemType'
>;
