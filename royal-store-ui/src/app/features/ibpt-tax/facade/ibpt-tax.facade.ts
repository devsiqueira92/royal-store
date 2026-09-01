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

  search(q: string): void {
    this.store.dispatch(IbptTaxActions.searchIbptTaxes({ q }));
  }

  clear(): void {
    this.store.dispatch(IbptTaxActions.clearIbptTaxes());
  }
}
