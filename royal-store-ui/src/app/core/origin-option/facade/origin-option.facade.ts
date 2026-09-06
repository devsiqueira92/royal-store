import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { OriginOptionActions } from '../store/origin-option.actions';
import {
  selectOriginOptions,
  selectIsLoading,
  selectError,
} from '../store/origin-option.selectors';

@Injectable({
  providedIn: 'root',
})
export class OriginOptionFacade {
  private store = inject(Store);

  originOptions$ = this.store.select(selectOriginOptions);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadOriginOptions(): void {
    this.store.dispatch(OriginOptionActions.loadOriginOptions());
  }
}
