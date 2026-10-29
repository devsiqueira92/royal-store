import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CurrentSellerActions } from '../store/current-seller.actions';
import {
  selectCurrentSeller,
  selectIsLoading,
  selectError,
} from '../store/current-seller.selectors';

@Injectable({
  providedIn: 'root',
})
export class CurrentSellerFacade {
  private store = inject(Store);

  currentSeller$ = this.store.select(selectCurrentSeller);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadCurrentSeller(): void {
    this.store.dispatch(CurrentSellerActions.loadCurrentSeller());
  }

  clearCurrentSeller(): void {
    this.store.dispatch(CurrentSellerActions.clearCurrentSeller());
  }
}
