import { createFeature, createReducer, on } from '@ngrx/store';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';
import { LoyaltyTierActions } from './loyalty-tier.actions';

export interface LoyaltyTierState {
  loyaltyTiers: LoyaltyTier[];
  selectedLoyaltyTier: LoyaltyTier | null;
  isLoading: boolean;
  error: string | null;
}

export const initialLoyaltyTierState: LoyaltyTierState = {
  loyaltyTiers: [],
  selectedLoyaltyTier: null,
  isLoading: false,
  error: null,
};

const upsert = (
  state: LoyaltyTierState,
  loyaltyTier: LoyaltyTier,
): LoyaltyTierState => {
  const exists = state.loyaltyTiers.some(t => t.id === loyaltyTier.id);
  return {
    ...state,
    loyaltyTiers: exists
      ? state.loyaltyTiers.map(t => (t.id === loyaltyTier.id ? loyaltyTier : t))
      : [...state.loyaltyTiers, loyaltyTier],
    selectedLoyaltyTier:
      state.selectedLoyaltyTier?.id === loyaltyTier.id
        ? loyaltyTier
        : state.selectedLoyaltyTier,
    isLoading: false,
  };
};

const setActive = (
  state: LoyaltyTierState,
  id: string,
  isActive: boolean,
): LoyaltyTierState => ({
  ...state,
  loyaltyTiers: state.loyaltyTiers.map(t =>
    t.id === id ? { ...t, isActive } : t,
  ),
  selectedLoyaltyTier:
    state.selectedLoyaltyTier?.id === id
      ? { ...state.selectedLoyaltyTier, isActive }
      : state.selectedLoyaltyTier,
  isLoading: false,
});

export const loyaltyTierFeature = createFeature({
  name: 'loyaltyTier',
  reducer: createReducer(
    initialLoyaltyTierState,
    on(
      LoyaltyTierActions.loadLoyaltyTiers,
      LoyaltyTierActions.loadLoyaltyTier,
      LoyaltyTierActions.createLoyaltyTier,
      LoyaltyTierActions.updateLoyaltyTier,
      LoyaltyTierActions.deleteLoyaltyTier,
      LoyaltyTierActions.activateLoyaltyTier,
      LoyaltyTierActions.deactivateLoyaltyTier,
      (state): LoyaltyTierState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),
    on(
      LoyaltyTierActions.loadLoyaltyTiersFailure,
      LoyaltyTierActions.loadLoyaltyTierFailure,
      LoyaltyTierActions.createLoyaltyTierFailure,
      LoyaltyTierActions.updateLoyaltyTierFailure,
      LoyaltyTierActions.deleteLoyaltyTierFailure,
      LoyaltyTierActions.activateLoyaltyTierFailure,
      LoyaltyTierActions.deactivateLoyaltyTierFailure,
      (state, { error }): LoyaltyTierState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      LoyaltyTierActions.loadLoyaltyTiersSuccess,
      (state, { loyaltyTiers }): LoyaltyTierState => ({
        ...state,
        loyaltyTiers,
        isLoading: false,
      }),
    ),
    on(
      LoyaltyTierActions.loadLoyaltyTierSuccess,
      (state, { loyaltyTier }): LoyaltyTierState => ({
        ...state,
        selectedLoyaltyTier: loyaltyTier,
        isLoading: false,
      }),
    ),
    on(
      LoyaltyTierActions.createLoyaltyTierSuccess,
      LoyaltyTierActions.updateLoyaltyTierSuccess,
      (state, { loyaltyTier }) => upsert(state, loyaltyTier),
    ),
    on(
      LoyaltyTierActions.deleteLoyaltyTierSuccess,
      (state, { id }): LoyaltyTierState => ({
        ...state,
        loyaltyTiers: state.loyaltyTiers.filter(t => t.id !== id),
        selectedLoyaltyTier:
          state.selectedLoyaltyTier?.id === id
            ? null
            : state.selectedLoyaltyTier,
        isLoading: false,
      }),
    ),
    on(LoyaltyTierActions.activateLoyaltyTierSuccess, (state, { id }) =>
      setActive(state, id, true),
    ),
    on(LoyaltyTierActions.deactivateLoyaltyTierSuccess, (state, { id }) =>
      setActive(state, id, false),
    ),
    on(
      LoyaltyTierActions.clearSelectedLoyaltyTier,
      (state): LoyaltyTierState => ({
        ...state,
        selectedLoyaltyTier: null,
      }),
    ),
  ),
});
