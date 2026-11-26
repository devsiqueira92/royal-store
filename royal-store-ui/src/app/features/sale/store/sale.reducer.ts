import { createFeature, createReducer, on } from '@ngrx/store';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';
import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { SaleListItem } from '../interfaces/sale-list-item.interface';
import { SaleDetails } from '../interfaces/sale-details.interface';
import { SaleReceipt } from '../interfaces/sale-receipt.interface';
import { GetSalesListParams } from '../interfaces/get-sales-list-params.interface';
import {
  initialPagedListState,
  pagedList,
  PagedListState,
} from '@app/shared/utils/paged-list-state.util';
import { SalePriceListOption } from '../interfaces/sale-price-list-option.interface';
import { SaleActions } from './sale.actions';

export interface SaleState {
  isSyncing: boolean;
  lastSyncResult: SyncSalesResponse | null;
  posProducts: PosProduct[];
  isLoadingProducts: boolean;
  /** Sales history, cursor-paged for infinite scroll (067). */
  salesList: PagedListState<SaleListItem, GetSalesListParams>;
  saleDetails: SaleDetails | null;
  isLoadingSaleDetails: boolean;
  saleReceipt: SaleReceipt | null;
  isLoadingSaleReceipt: boolean;
  isRegisteringSale: boolean;
  lastRegisterSaleResult: RegisterSaleResponse | null;
  /** Price lists offered at checkout, each with the current cart priced on it. */
  priceListOptions: SalePriceListOption[];
  isLoadingPriceListOptions: boolean;
  error: string | null;
}

export const initialSaleState: SaleState = {
  isSyncing: false,
  lastSyncResult: null,
  posProducts: [],
  isLoadingProducts: false,
  salesList: initialPagedListState<SaleListItem, GetSalesListParams>(),
  saleDetails: null,
  isLoadingSaleDetails: false,
  saleReceipt: null,
  isLoadingSaleReceipt: false,
  isRegisteringSale: false,
  lastRegisterSaleResult: null,
  priceListOptions: [],
  isLoadingPriceListOptions: false,
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
    on(SaleActions.loadSales, (state, { params }): SaleState => ({
      ...state,
      salesList: pagedList.firstPageRequested(state.salesList, params),
      error: null,
    })),
    on(SaleActions.loadMoreSales, (state): SaleState => ({
      ...state,
      salesList: pagedList.nextPageRequested(state.salesList),
    })),
    on(SaleActions.loadSalesSuccess, (state, { page, append }): SaleState => ({
      ...state,
      salesList: pagedList.pageLoaded(state.salesList, page, append),
    })),
    on(SaleActions.loadSalesFailure, (state, { error, append }): SaleState => ({
      ...state,
      salesList: pagedList.pageFailed(state.salesList, append),
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
    on(SaleActions.loadPriceListOptions, (state): SaleState => ({
      ...state,
      priceListOptions: [],
      isLoadingPriceListOptions: true,
    })),
    on(SaleActions.loadPriceListOptionsSuccess, (state, { options }): SaleState => ({
      ...state,
      priceListOptions: options,
      isLoadingPriceListOptions: false,
    })),
    // Not written to `error`: the checkout dialog reads that one as a failed registration.
    on(SaleActions.loadPriceListOptionsFailure, (state): SaleState => ({
      ...state,
      isLoadingPriceListOptions: false,
    })),
    on(SaleActions.clearPriceListOptions, (state): SaleState => ({
      ...state,
      priceListOptions: [],
      isLoadingPriceListOptions: false,
    })),
  ),
});
