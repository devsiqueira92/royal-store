import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { map } from 'rxjs';
import { SupplierActions } from '../store/supplier.actions';
import { SupplierRequest } from '../interfaces/supplier-request.interface';
import {
  selectSuppliers,
  selectSelectedSupplier,
  selectIsLoading,
  selectError,
} from '../store/supplier.selectors';

@Injectable({
  providedIn: 'root',
})
export class SupplierFacade {
  private store = inject(Store);
  private readonly actions$ = inject(Actions);

  suppliers$ = this.store.select(selectSuppliers);
  selectedSupplier$ = this.store.select(selectSelectedSupplier);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  /** Emits the supplier once a create or an update has succeeded on the API. */
  saved$ = this.actions$.pipe(
    ofType(
      SupplierActions.createSupplierSuccess,
      SupplierActions.updateSupplierSuccess,
    ),
    map(({ supplier }) => supplier),
  );

  loadSuppliers(): void {
    this.store.dispatch(SupplierActions.loadSuppliers());
  }

  loadSupplier(id: string): void {
    this.store.dispatch(SupplierActions.loadSupplier({ id }));
  }

  createSupplier(request: SupplierRequest): void {
    this.store.dispatch(SupplierActions.createSupplier({ request }));
  }

  updateSupplier(id: string, request: SupplierRequest): void {
    this.store.dispatch(SupplierActions.updateSupplier({ id, request }));
  }

  activate(id: string): void {
    this.store.dispatch(SupplierActions.activateSupplier({ id }));
  }

  deactivate(id: string): void {
    this.store.dispatch(SupplierActions.deactivateSupplier({ id }));
  }

  clearSelectedSupplier(): void {
    this.store.dispatch(SupplierActions.clearSelectedSupplier());
  }
}
