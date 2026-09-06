import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { OriginOption } from '../interfaces/origin-option.interface';

export const OriginOptionActions = createActionGroup({
  source: 'OriginOption',
  events: {
    'Load Origin Options': emptyProps(),
    'Load Origin Options Success': props<{ originOptions: OriginOption[] }>(),
    'Load Origin Options Failure': props<{ error: string }>(),
  },
});
