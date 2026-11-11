import { createFeature, createReducer, on } from '@ngrx/store';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';
import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { SaleListItem } from '../interfaces/sale-list-item.interface';
import { SaleDetails } from '../interfaces/sale-details.interface';
import { SaleReceipt } from '../interfaces/sale-receipt.interface';
import { SaleActions } from './sale.actions';

export interface SaleState {
  isSyncing: boolean;
  lastSyncResult: SyncSalesResponse | null;
  posProducts: PosProduct[];
  isLoadingProducts: boolean;
  sales: SaleListItem[];
  isLoadingSales: boolean;
  saleDetails: SaleDetails | null;
  isLoadingSaleDetails: boolean;
  saleReceipt: SaleReceipt | null;
  isLoadingSaleReceipt: boolean;
  isRegisteringSale: boolean;
  lastRegisterSaleResult: RegisterSaleResponse | null;
  error: string | null;
}

export const initialSaleState: SaleState = {
  isSyncing: false,
  lastSyncResult: null,
  posProducts: [],
  isLoadingProducts: false,
  sales: [],
  isLoadingSales: false,
  saleDetails: null,
  isLoadingSaleDetails: false,
  saleReceipt: null,
  isLoadingSaleReceipt: false,
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
    on(SaleActions.loadSales, (state): SaleState => ({
      ...state,
      isLoadingSales: true,
      error: null,
    })),
    on(SaleActions.loadSalesSuccess, (state, { sales }): SaleState => ({
      ...state,
      sales,
      isLoadingSales: false,
    })),
    on(SaleActions.loadSalesFailure, (state, { error }): SaleState => ({
      ...state,
      isLoadingSales: false,
      error,
    })),
    on(SaleActions.loadSaleDetails, (state, { saleId }): SaleState => ({
      ...state,
      saleDetails: state.saleDetails?.id === saleId ? state.saleDetails : null,
      isLoadingSaleDetails: true,
      error: null,
    })),
    on(SaleActions.loadSaleDetailsSuccess, (state, { saleDetails }): SaleState => ({
      ...state,
      saleDetails,
      isLoadingSaleDetails: false,
    })),
    on(SaleActions.loadSaleDetailsFailure, (state, { error }): SaleState => ({
      ...state,
      isLoadingSaleDetails: false,
      error,
    })),
    on(SaleActions.clearSaleDetails, (state): SaleState => ({
      ...state,
      saleDetails: null,
    })),
    on(SaleActions.loadSaleReceipt, (state): SaleState => ({
      ...state,
      saleReceipt: null,
      isLoadingSaleReceipt: true,
      error: null,
    })),
    on(SaleActions.loadSaleReceiptSuccess, (state, { saleReceipt }): SaleState => ({
      ...state,
      saleReceipt,
      isLoadingSaleReceipt: false,
    })),
    on(SaleActions.loadSaleReceiptFailure, (state, { error }): SaleState => ({
      ...state,
      isLoadingSaleReceipt: false,
      error,
    })),
    on(SaleActions.clearSaleReceipt, (state): SaleState => ({
      ...state,
      saleReceipt: null,
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
