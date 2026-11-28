import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { map } from 'rxjs';
import {
  CreatePriceListRequest,
  UpdatePriceListRequest,
} from '../interfaces/price-list.interface';
import { PriceListActions } from '../store/price-list.actions';
import {
  selectError,
  selectIsLoading,
  selectPriceLists,
  selectSelectedPriceList,
} from '../store/price-list.selectors';

@Injectable({ providedIn: 'root' })
export class PriceListFacade {
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);

  priceLists$ = this.store.select(selectPriceLists);
  selectedPriceList$ = this.store.select(selectSelectedPriceList);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  /** Emits the list once a create or an update has succeeded on the API. */
  saved$ = this.actions$.pipe(
    ofType(
      PriceListActions.createPriceListSuccess,
      PriceListActions.updatePriceListSuccess,
    ),
    map(({ priceList }) => priceList),
  );

  loadPriceLists(): void {
    this.store.dispatch(PriceListActions.loadPriceLists());
  }

  createPriceList(request: CreatePriceListRequest): void {
    this.store.dispatch(PriceListActions.createPriceList({ request }));
  }

  /** Name and rule in a single PUT. */
  updatePriceList(id: string, request: UpdatePriceListRequest): void {
    this.store.dispatch(PriceListActions.updatePriceList({ id, request }));
  }

  setDefault(id: string): void {
    this.store.dispatch(PriceListActions.setDefaultPriceList({ id }));
  }

  activate(id: string): void {
    this.store.dispatch(PriceListActions.activatePriceList({ id }));
  }

  deactivate(id: string): void {
    this.store.dispatch(PriceListActions.deactivatePriceList({ id }));
  }

  delete(id: string): void {
    this.store.dispatch(PriceListActions.deletePriceList({ id }));
  }

  selectPriceList(id: string): void {
    this.store.dispatch(PriceListActions.selectPriceList({ id }));
  }

  clearSelectedPriceList(): void {
    this.store.dispatch(PriceListActions.clearSelectedPriceList());
  }
}
