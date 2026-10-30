import { createFeature, createReducer, on } from '@ngrx/store';
import { CurrentSeller } from '../interfaces/current-seller.interface';
import { CurrentSellerActions } from './current-seller.actions';
import { AuthActions } from '@app/core/auth/store/auth.actions';

export interface CurrentSellerState {
  currentSeller: CurrentSeller | null;
  isLoading: boolean;
  error: string | null;
}

export const initialCurrentSellerState: CurrentSellerState = {
  currentSeller: null,
  isLoading: false,
  error: null,
};

export const currentSellerFeature = createFeature({
  name: 'currentSeller',
  reducer: createReducer(
    initialCurrentSellerState,
    on(CurrentSellerActions.loadCurrentSeller, (state): CurrentSellerState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      CurrentSellerActions.loadCurrentSellerSuccess,
      (state, { currentSeller }): CurrentSellerState => ({
        ...state,
        currentSeller,
        isLoading: false,
      }),
    ),
    on(
      CurrentSellerActions.loadCurrentSellerFailure,
      (state, { error }): CurrentSellerState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    // The profile belongs to whoever is signed in: never keep it past logout.
    on(
      CurrentSellerActions.clearCurrentSeller,
      AuthActions.logout,
      (): CurrentSellerState => initialCurrentSellerState,
    ),
  ),
});
