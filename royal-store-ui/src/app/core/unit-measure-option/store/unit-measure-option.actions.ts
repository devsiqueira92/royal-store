import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { UnitMeasureOption } from '../interfaces/unit-measure-option.interface';

export const UnitMeasureOptionActions = createActionGroup({
  source: 'UnitMeasureOption',
  events: {
    'Load Unit Measure Options': emptyProps(),
    'Load Unit Measure Options Success': props<{ unitMeasureOptions: UnitMeasureOption[] }>(),
    'Load Unit Measure Options Failure': props<{ error: string }>(),
  },
});
