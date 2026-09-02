import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { IbptTax } from '../../../core/ibpt-tax/interfaces/ibpt-tax.interface';

export const IbptTaxActions = createActionGroup({
  source: 'IbptTax',
  events: {
    'Search Ibpt Taxes': props<{ q: string; taxType: number }>(),
    'Search Ibpt Taxes Success': props<{ results: IbptTax[] }>(),
    'Search Ibpt Taxes Failure': props<{ error: string }>(),
    'Clear Ibpt Taxes': emptyProps(),
  },
});
