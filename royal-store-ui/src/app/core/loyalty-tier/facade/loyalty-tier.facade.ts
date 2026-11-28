import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { map } from 'rxjs';
import { LoyaltyTierActions } from '../store/loyalty-tier.actions';
import {
  CreateLoyaltyTierRequest,
  UpdateLoyaltyTierRequest,
} from '../interfaces/loyalty-tier-request.interface';
import {
  selectLoyaltyTiers,
  selectSelectedLoyaltyTier,
  selectLoyaltyTierOptions,
  selectIsLoading,
  selectError,
} from '../store/loyalty-tier.selectors';

@Injectable({
  providedIn: 'root',
})
export class LoyaltyTierFacade {
  private store = inject(Store);
  private readonly actions$ = inject(Actions);

  loyaltyTiers$ = this.store.select(selectLoyaltyTiers);
  selectedLoyaltyTier$ = this.store.select(selectSelectedLoyaltyTier);
  /** Active tiers for dropdowns; call `loadLoyaltyTierOptions` first. */
  loyaltyTierOptions$ = this.store.select(selectLoyaltyTierOptions);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  /** Emits the tier once a create or an update has succeeded on the API. */
  saved$ = this.actions$.pipe(
    ofType(
      LoyaltyTierActions.createLoyaltyTierSuccess,
      LoyaltyTierActions.updateLoyaltyTierSuccess,
    ),
    map(({ loyaltyTier }) => loyaltyTier),
  );

  loadLoyaltyTiers(): void {
    this.store.dispatch(LoyaltyTierActions.loadLoyaltyTiers());
  }

  loadLoyaltyTierOptions(): void {
    this.store.dispatch(LoyaltyTierActions.loadLoyaltyTierOptions());
  }

  loadLoyaltyTier(id: string): void {
    this.store.dispatch(LoyaltyTierActions.loadLoyaltyTier({ id }));
  }

  createLoyaltyTier(request: CreateLoyaltyTierRequest): void {
    this.store.dispatch(LoyaltyTierActions.createLoyaltyTier({ request }));
  }

  updateLoyaltyTier(id: string, request: UpdateLoyaltyTierRequest): void {
    this.store.dispatch(LoyaltyTierActions.updateLoyaltyTier({ id, request }));
  }

  deleteLoyaltyTier(id: string): void {
    this.store.dispatch(LoyaltyTierActions.deleteLoyaltyTier({ id }));
  }

  activate(id: string): void {
    this.store.dispatch(LoyaltyTierActions.activateLoyaltyTier({ id }));
  }

  deactivate(id: string): void {
    this.store.dispatch(LoyaltyTierActions.deactivateLoyaltyTier({ id }));
  }

  clearSelectedLoyaltyTier(): void {
    this.store.dispatch(LoyaltyTierActions.clearSelectedLoyaltyTier());
  }
}
