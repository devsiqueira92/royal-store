import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoyaltyTierActions } from '../store/loyalty-tier.actions';
import {
  selectLoyaltyTiers,
  selectIsLoading,
  selectError,
} from '../store/loyalty-tier.selectors';

@Injectable({
  providedIn: 'root',
})
export class LoyaltyTierFacade {
  private store = inject(Store);

  loyaltyTiers$ = this.store.select(selectLoyaltyTiers);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadLoyaltyTiers(): void {
    this.store.dispatch(LoyaltyTierActions.loadLoyaltyTiers());
  }
}
