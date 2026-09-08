import { createFeature, createReducer, on } from '@ngrx/store';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';
import { LoyaltyTierActions } from './loyalty-tier.actions';

export interface LoyaltyTierState {
  loyaltyTiers: LoyaltyTier[];
  isLoading: boolean;
  error: string | null;
}

export const initialLoyaltyTierState: LoyaltyTierState = {
  loyaltyTiers: [],
  isLoading: false,
  error: null,
};

export const loyaltyTierFeature = createFeature({
  name: 'loyaltyTier',
  reducer: createReducer(
    initialLoyaltyTierState,
    on(LoyaltyTierActions.loadLoyaltyTiers, (state): LoyaltyTierState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      LoyaltyTierActions.loadLoyaltyTiersSuccess,
      (state, { loyaltyTiers }): LoyaltyTierState => ({
        ...state,
        loyaltyTiers,
        isLoading: false,
      }),
    ),
    on(
      LoyaltyTierActions.loadLoyaltyTiersFailure,
      (state, { error }): LoyaltyTierState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
  ),
});
