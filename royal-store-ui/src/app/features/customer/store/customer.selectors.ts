import { customerFeature } from './customer.reducer';

export const {
  selectCustomers,
  selectSelectedCustomer,
  selectCashbackHistory,
  selectIsLoading,
  selectIsLoadingHistory,
  selectError,
} = customerFeature;
