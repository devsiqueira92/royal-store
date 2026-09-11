import { inject, Injectable } from '@angular/core';
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
