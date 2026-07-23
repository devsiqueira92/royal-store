import { createFeature, createReducer, on } from '@ngrx/store';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { SaleActions } from './sale.actions';

export interface SaleState {
  isSyncing: boolean;
  lastSyncResult: SyncSalesResponse | null;
  error: string | null;
}

export const initialSaleState: SaleState = {
  isSyncing: false,
  lastSyncResult: null,
  error: null,
};

export const saleFeature = createFeature({
  name: 'sale',
  reducer: createReducer(
    initialSaleState,
    on(SaleActions.syncSales, (state): SaleState => ({
      ...state,
      isSyncing: true,
      error: null,
    })),
    on(SaleActions.syncSalesSuccess, (state, { response }): SaleState => ({
      ...state,
      isSyncing: false,
      lastSyncResult: response,
    })),
    on(SaleActions.syncSalesFailure, (state, { error }): SaleState => ({
      ...state,
      isSyncing: false,
      error,
    })),
    on(SaleActions.clearSyncResult, (state): SaleState => ({
      ...state,
      lastSyncResult: null,
    })),
  ),
});
