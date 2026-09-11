import { saleFeature } from './sale.reducer';

export const {
  selectIsSyncing,
  selectLastSyncResult,
  selectError,
  selectPosProducts,
  selectIsLoadingProducts,
  selectIsRegisteringSale,
  selectLastRegisterSaleResult,
} = saleFeature;
