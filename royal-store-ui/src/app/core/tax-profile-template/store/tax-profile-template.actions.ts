import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  TaxProfileTemplate,
  TaxProfileTemplateTargetItemType,
} from '../interfaces/tax-profile-template.interface';
import {
  CreateTaxProfileTemplateRequest,
  UpdateTaxProfileTemplateRequest,
} from '../interfaces/tax-profile-template-request.interface';

export const TaxProfileTemplateActions = createActionGroup({
  source: 'TaxProfileTemplate',
  events: {
    'Load Templates': props<{
      itemType?: TaxProfileTemplateTargetItemType;
    }>(),
    'Load Templates Success': props<{ templates: TaxProfileTemplate[] }>(),
    'Load Templates Failure': props<{ error: string }>(),
    'Create Template': props<{ request: CreateTaxProfileTemplateRequest }>(),
    'Create Template Success': props<{ template: TaxProfileTemplate }>(),
    'Create Template Failure': props<{ error: string }>(),
    'Update Template': props<{
      id: string;
      request: UpdateTaxProfileTemplateRequest;
    }>(),
    'Update Template Success': props<{ template: TaxProfileTemplate }>(),
    'Update Template Failure': props<{ error: string }>(),
    'Deactivate Template': props<{ id: string }>(),
    'Deactivate Template Success': props<{ id: string }>(),
    'Deactivate Template Failure': props<{ error: string }>(),
    'Clear Templates Error': emptyProps(),
    'Select Template': props<{ id: string }>(),
    'Clear Selected Template': emptyProps(),
  },
});
