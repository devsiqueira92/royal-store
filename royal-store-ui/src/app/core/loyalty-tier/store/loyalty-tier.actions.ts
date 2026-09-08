import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';

export const LoyaltyTierActions = createActionGroup({
  source: 'LoyaltyTier',
  events: {
    'Load Loyalty Tiers': emptyProps(),
    'Load Loyalty Tiers Success': props<{ loyaltyTiers: LoyaltyTier[] }>(),
    'Load Loyalty Tiers Failure': props<{ error: string }>(),
  },
});
