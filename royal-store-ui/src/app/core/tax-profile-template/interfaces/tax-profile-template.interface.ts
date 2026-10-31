import { FormControl } from '@angular/forms';

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

export interface TaxProfileTemplateFormControls {
  name: FormControl<string | null>;
  description: FormControl<string | null>;
  // Held as the select field's string code ('0' | '1'); converted to a
  // number only when building the create request.
  targetItemType: FormControl<string | null>;
  defaultCfop: FormControl<string | null>;
  icmsCst: FormControl<string | null>;
  pisCofinsCst: FormControl<string | null>;
  ivaCst: FormControl<string | null>;
  isSelectiveTax: FormControl<boolean>;
  isDefault: FormControl<boolean>;
}
