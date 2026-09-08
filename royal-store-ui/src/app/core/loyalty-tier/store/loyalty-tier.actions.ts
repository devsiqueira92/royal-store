import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';
import {
  CreateLoyaltyTierRequest,
  UpdateLoyaltyTierRequest,
} from '../interfaces/loyalty-tier-request.interface';

export const LoyaltyTierActions = createActionGroup({
  source: 'LoyaltyTier',
  events: {
    'Load Loyalty Tiers': emptyProps(),
    'Load Loyalty Tiers Success': props<{ loyaltyTiers: LoyaltyTier[] }>(),
    'Load Loyalty Tiers Failure': props<{ error: string }>(),

    'Load Loyalty Tier': props<{ id: string }>(),
    'Load Loyalty Tier Success': props<{ loyaltyTier: LoyaltyTier }>(),
    'Load Loyalty Tier Failure': props<{ error: string }>(),

    'Create Loyalty Tier': props<{ request: CreateLoyaltyTierRequest }>(),
    'Create Loyalty Tier Success': props<{ loyaltyTier: LoyaltyTier }>(),
    'Create Loyalty Tier Failure': props<{ error: string }>(),

    'Update Loyalty Tier': props<{
      id: string;
      request: UpdateLoyaltyTierRequest;
    }>(),
    'Update Loyalty Tier Success': props<{ loyaltyTier: LoyaltyTier }>(),
    'Update Loyalty Tier Failure': props<{ error: string }>(),

    'Delete Loyalty Tier': props<{ id: string }>(),
    'Delete Loyalty Tier Success': props<{ id: string }>(),
    'Delete Loyalty Tier Failure': props<{ error: string }>(),

    'Activate Loyalty Tier': props<{ id: string }>(),
    'Activate Loyalty Tier Success': props<{ id: string }>(),
    'Activate Loyalty Tier Failure': props<{ error: string }>(),

    'Deactivate Loyalty Tier': props<{ id: string }>(),
    'Deactivate Loyalty Tier Success': props<{ id: string }>(),
    'Deactivate Loyalty Tier Failure': props<{ error: string }>(),

    'Clear Selected Loyalty Tier': emptyProps(),
  },
});
