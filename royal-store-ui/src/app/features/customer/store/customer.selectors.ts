import { customerFeature } from './customer.reducer';

export const {
  selectCustomers,
  selectSelectedCustomer,
  selectCashbackHistory,
  selectCashbackBalance,
  selectIsLoading,
  selectIsLoadingHistory,
  selectIsLoadingBalance,
  selectError,
} = customerFeature;
