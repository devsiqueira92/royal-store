import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CurrentSeller,
  UpdateMyProfileRequest,
} from '../interfaces/current-seller.interface';

export const CurrentSellerActions = createActionGroup({
  source: 'Current Seller',
  events: {
    'Load Current Seller': emptyProps(),
    'Load Current Seller Success': props<{ currentSeller: CurrentSeller }>(),
    'Load Current Seller Failure': props<{ error: string }>(),
    'Update My Profile': props<{ request: UpdateMyProfileRequest }>(),
    'Update My Profile Success': props<{ currentSeller: CurrentSeller }>(),
    'Update My Profile Failure': props<{ error: string }>(),
    'Clear Current Seller': emptyProps(),
  },
});
