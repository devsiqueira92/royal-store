import { createFeature, createReducer, on } from '@ngrx/store';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';
import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { SaleActions } from './sale.actions';

export interface SaleState {
  isSyncing: boolean;
  lastSyncResult: SyncSalesResponse | null;
  posProducts: PosProduct[];
  isLoadingProducts: boolean;
  isRegisteringSale: boolean;
  lastRegisterSaleResult: RegisterSaleResponse | null;
  error: string | null;
}

export const initialSaleState: SaleState = {
  isSyncing: false,
  lastSyncResult: null,
  posProducts: [],
  isLoadingProducts: false,
  isRegisteringSale: false,
  lastRegisterSaleResult: null,
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
    on(SaleActions.loadPosProducts, (state): SaleState => ({
      ...state,
      isLoadingProducts: true,
      error: null,
    })),
    on(SaleActions.loadPosProductsSuccess, (state, { products }): SaleState => ({
      ...state,
      posProducts: products,
      isLoadingProducts: false,
    })),
    on(SaleActions.loadPosProductsFailure, (state, { error }): SaleState => ({
      ...state,
      isLoadingProducts: false,
      error,
    })),
    on(SaleActions.registerSale, (state): SaleState => ({
      ...state,
      isRegisteringSale: true,
      error: null,
    })),
    on(SaleActions.registerSaleSuccess, (state, { response }): SaleState => ({
      ...state,
      isRegisteringSale: false,
      lastRegisterSaleResult: response,
    })),
    on(SaleActions.registerSaleFailure, (state, { error }): SaleState => ({
      ...state,
      isRegisteringSale: false,
      error,
    })),
    on(SaleActions.clearRegisterSaleResult, (state): SaleState => ({
      ...state,
      lastRegisterSaleResult: null,
    })),
  ),
});
