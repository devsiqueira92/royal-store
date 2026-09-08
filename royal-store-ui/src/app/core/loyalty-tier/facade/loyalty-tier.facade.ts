import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoyaltyTierActions } from '../store/loyalty-tier.actions';
import {
  CreateLoyaltyTierRequest,
  UpdateLoyaltyTierRequest,
} from '../interfaces/loyalty-tier-request.interface';
import {
  selectLoyaltyTiers,
  selectSelectedLoyaltyTier,
  selectIsLoading,
  selectError,
} from '../store/loyalty-tier.selectors';

@Injectable({
  providedIn: 'root',
})
export class LoyaltyTierFacade {
  private store = inject(Store);

  loyaltyTiers$ = this.store.select(selectLoyaltyTiers);
  selectedLoyaltyTier$ = this.store.select(selectSelectedLoyaltyTier);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadLoyaltyTiers(): void {
    this.store.dispatch(LoyaltyTierActions.loadLoyaltyTiers());
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
