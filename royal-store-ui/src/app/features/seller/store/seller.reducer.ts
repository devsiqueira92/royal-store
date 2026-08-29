import { createFeature, createReducer, on } from '@ngrx/store';
import { Seller, SellerRole } from '../interfaces/seller.interface';
import { SellerActions } from './seller.actions';

export interface SellerState {
  sellers: Seller[];
  selectedSeller: Seller | null;
  isLoading: boolean;
  error: string | null;
}

export const initialSellerState: SellerState = {
  sellers: [],
  selectedSeller: null,
  isLoading: false,
  error: null,
};

export const sellerFeature = createFeature({
  name: 'seller',
  reducer: createReducer(
    initialSellerState,
    on(SellerActions.loadSellers, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(SellerActions.loadSellersSuccess, (state, { sellers }): SellerState => ({
      ...state,
      sellers,
      isLoading: false,
    })),
    on(SellerActions.loadSellersFailure, (state, { error }): SellerState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(SellerActions.loadSeller, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(SellerActions.loadSellerSuccess, (state, { seller }): SellerState => ({
      ...state,
      selectedSeller: seller,
      isLoading: false,
    })),
    on(SellerActions.loadSellerFailure, (state, { error }): SellerState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(SellerActions.createSeller, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(SellerActions.createSellerSuccess, (state, { seller }): SellerState => ({
      ...state,
      sellers: [...state.sellers, seller],
      isLoading: false,
    })),
    on(SellerActions.createSellerFailure, (state, { error }): SellerState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(SellerActions.updateSeller, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(SellerActions.updateSellerSuccess, (state, { seller }): SellerState => ({
      ...state,
      sellers: state.sellers.map(s => (s.id === seller.id ? seller : s)),
      selectedSeller:
        state.selectedSeller?.id === seller.id ? seller : state.selectedSeller,
      isLoading: false,
    })),
    on(SellerActions.updateSellerFailure, (state, { error }): SellerState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(SellerActions.updateSellerPermissions, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      SellerActions.updateSellerPermissionsSuccess,
      (state, { permissions }): SellerState => {
        const role = permissions.role.toLowerCase() as SellerRole;
        const patch = (seller: Seller): Seller => ({
          ...seller,
          role,
          permissions: permissions.grants,
        });

        return {
          ...state,
          sellers: state.sellers.map(s =>
            s.id === permissions.sellerId ? patch(s) : s,
          ),
          selectedSeller:
            state.selectedSeller?.id === permissions.sellerId
              ? patch(state.selectedSeller)
              : state.selectedSeller,
          isLoading: false,
        };
      },
    ),
    on(
      SellerActions.updateSellerPermissionsFailure,
      (state, { error }): SellerState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(SellerActions.deactivateSeller, (state): SellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(SellerActions.deactivateSellerSuccess, (state, { id }): SellerState => ({
      ...state,
      sellers: state.sellers.map(s => (s.id === id ? { ...s, isActive: false } : s)),
      selectedSeller:
        state.selectedSeller?.id === id
          ? { ...state.selectedSeller, isActive: false }
          : state.selectedSeller,
      isLoading: false,
    })),
    on(SellerActions.deactivateSellerFailure, (state, { error }): SellerState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(SellerActions.clearSelectedSeller, (state): SellerState => ({
      ...state,
      selectedSeller: null,
    })),
  ),
});
