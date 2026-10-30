import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CurrentSeller } from '../interfaces/current-seller.interface';

export const CurrentSellerActions = createActionGroup({
  source: 'Current Seller',
  events: {
    'Load Current Seller': emptyProps(),
    'Load Current Seller Success': props<{ currentSeller: CurrentSeller }>(),
    'Load Current Seller Failure': props<{ error: string }>(),
    'Clear Current Seller': emptyProps(),
  },
});
