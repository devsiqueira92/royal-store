import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { UnitMeasureOptionActions } from '../store/unit-measure-option.actions';
import {
  selectUnitMeasureOptions,
  selectIsLoading,
  selectError,
} from '../store/unit-measure-option.selectors';

@Injectable({
  providedIn: 'root',
})
export class UnitMeasureOptionFacade {
  private store = inject(Store);

  unitMeasureOptions$ = this.store.select(selectUnitMeasureOptions);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadUnitMeasureOptions(): void {
    this.store.dispatch(UnitMeasureOptionActions.loadUnitMeasureOptions());
  }
}
