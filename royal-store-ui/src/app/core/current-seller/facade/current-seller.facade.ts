import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CurrentSellerActions } from '../store/current-seller.actions';
import { UpdateMyProfileRequest } from '../interfaces/current-seller.interface';
import {
  selectCurrentSeller,
  selectIsLoading,
  selectIsSaving,
  selectError,
} from '../store/current-seller.selectors';

@Injectable({
  providedIn: 'root',
})
export class CurrentSellerFacade {
  private store = inject(Store);

  currentSeller$ = this.store.select(selectCurrentSeller);
  isLoading$ = this.store.select(selectIsLoading);
  isSaving$ = this.store.select(selectIsSaving);
  error$ = this.store.select(selectError);

  loadCurrentSeller(): void {
    this.store.dispatch(CurrentSellerActions.loadCurrentSeller());
  }

  updateMyProfile(request: UpdateMyProfileRequest): void {
    this.store.dispatch(CurrentSellerActions.updateMyProfile({ request }));
  }

  clearCurrentSeller(): void {
    this.store.dispatch(CurrentSellerActions.clearCurrentSeller());
  }
}
