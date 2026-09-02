import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { IbptTaxActions } from '../store/ibpt-tax.actions';
import {
  selectResults,
  selectIsLoading,
  selectError,
} from '../store/ibpt-tax.selectors';

@Injectable({
  providedIn: 'root',
})
export class IbptTaxFacade {
  private store = inject(Store);

  results$ = this.store.select(selectResults);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  /**
   * @param type 0 = NCM (products), 1 = NBS (services)
   */
  search(q: string, type: number): void {
    this.store.dispatch(IbptTaxActions.searchIbptTaxes({ q, taxType: type }));
  }

  clear(): void {
    this.store.dispatch(IbptTaxActions.clearIbptTaxes());
  }
}
