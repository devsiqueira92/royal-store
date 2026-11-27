import { createSelector } from '@ngrx/store';
import { saleFeature } from './sale.reducer';

export const {
  selectIsSyncing,
  selectLastSyncResult,
  selectError,
  selectPosProducts,
  selectIsLoadingProducts,
  selectSalesList,
  selectSaleDetails,
  selectIsLoadingSaleDetails,
  selectSaleReceipt,
  selectIsLoadingSaleReceipt,
  selectIsRegisteringSale,
  selectLastRegisterSaleResult,
  selectPriceListOptions,
  selectIsLoadingPriceListOptions,
} = saleFeature;

export const selectSales = createSelector(selectSalesList, list => list.items);
export const selectIsLoadingSales = createSelector(selectSalesList, list => list.isLoading);
export const selectIsLoadingMoreSales = createSelector(
  selectSalesList,
  list => list.isLoadingMore,
);
export const selectSalesHasMore = createSelector(selectSalesList, list => list.hasMore);
export const selectSalesLoadMoreFailed = createSelector(
  selectSalesList,
  list => list.loadMoreFailed,
);
