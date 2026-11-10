import { inject, Injectable, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, debounceTime, map } from 'rxjs';
import { Store } from '@ngrx/store';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { RegisterSaleRequest } from '../interfaces/register-sale-request.interface';
import { SaleActions } from '../store/sale.actions';
import {
  selectIsSyncing,
  selectLastSyncResult,
  selectError,
  selectPosProducts,
  selectIsLoadingProducts,
  selectIsRegisteringSale,
  selectLastRegisterSaleResult,
} from '../store/sale.selectors';

@Injectable({
  providedIn: 'root',
})
export class SaleFacade {
  private store = inject(Store);

  isSyncing$ = this.store.select(selectIsSyncing);
  lastSyncResult$ = this.store.select(selectLastSyncResult);
  error$ = this.store.select(selectError);
  posProducts$ = this.store.select(selectPosProducts);
  isLoadingProducts$ = this.store.select(selectIsLoadingProducts);
  isRegisteringSale$ = this.store.select(selectIsRegisteringSale);
  lastRegisterSaleResult$ = this.store.select(selectLastRegisterSaleResult);

  // All active products for the selected category are loaded upfront (no pagination — see
  // loadPosProducts), so search just filters that in-memory list client-side, the same way the
  // barcode scanner already does, instead of round-tripping to the server on every keystroke.
  private readonly _searchTerm = signal('');
  readonly searchTerm = this._searchTerm.asReadonly();

  filteredPosProducts$ = combineLatest([
    this.posProducts$,
    toObservable(this._searchTerm).pipe(
      debounceTime(300),
      map(term => term.trim().toLowerCase()),
    ),
  ]).pipe(
    map(([products, term]) =>
      term
        ? products.filter(
            product =>
              product.productName.toLowerCase().includes(term) ||
              product.sku.toLowerCase().includes(term) ||
              product.barcode?.toLowerCase().includes(term),
          )
        : products,
    ),
  );

  setSearchTerm(term: string): void {
    this._searchTerm.set(term);
  }

  syncSales(request: SyncSalesRequest): void {
    this.store.dispatch(SaleActions.syncSales({ request }));
  }

  clearSyncResult(): void {
    this.store.dispatch(SaleActions.clearSyncResult());
  }

  loadPosProducts(categoryId?: string): void {
    this.store.dispatch(SaleActions.loadPosProducts({ categoryId }));
  }

  registerSale(request: RegisterSaleRequest): void {
    this.store.dispatch(SaleActions.registerSale({ request }));
  }

  clearRegisterSaleResult(): void {
    this.store.dispatch(SaleActions.clearRegisterSaleResult());
  }
}
