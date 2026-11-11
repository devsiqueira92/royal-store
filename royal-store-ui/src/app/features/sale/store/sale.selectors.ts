import { saleFeature } from './sale.reducer';

export const {
  selectIsSyncing,
  selectLastSyncResult,
  selectError,
  selectPosProducts,
  selectIsLoadingProducts,
  selectSales,
  selectIsLoadingSales,
  selectSaleDetails,
  selectIsLoadingSaleDetails,
  selectIsRegisteringSale,
  selectLastRegisterSaleResult,
} = saleFeature;
