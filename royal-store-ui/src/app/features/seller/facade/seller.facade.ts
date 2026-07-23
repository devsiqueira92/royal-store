import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateSellerRequest } from '../interfaces/create-seller-request.interface';
import { UpdateSellerRequest } from '../interfaces/update-seller-request.interface';
import { GetSellersParams } from '../interfaces/get-sellers-params.interface';
import { SellerActions } from '../store/seller.actions';
import {
  selectSellers,
  selectSelectedSeller,
  selectIsLoading,
  selectError,
} from '../store/seller.selectors';

@Injectable({
  providedIn: 'root',
})
export class SellerFacade {
  private store = inject(Store);

  sellers$ = this.store.select(selectSellers);
  selectedSeller$ = this.store.select(selectSelectedSeller);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadSellers(params?: GetSellersParams): void {
    this.store.dispatch(SellerActions.loadSellers({ params }));
  }

  loadSeller(id: string): void {
    this.store.dispatch(SellerActions.loadSeller({ id }));
  }

  createSeller(request: CreateSellerRequest): void {
    this.store.dispatch(SellerActions.createSeller({ request }));
  }

  updateSeller(id: string, request: UpdateSellerRequest): void {
    this.store.dispatch(SellerActions.updateSeller({ id, request }));
  }

  deactivateSeller(id: string): void {
    this.store.dispatch(SellerActions.deactivateSeller({ id }));
  }

  clearSelectedSeller(): void {
    this.store.dispatch(SellerActions.clearSelectedSeller());
  }
}
