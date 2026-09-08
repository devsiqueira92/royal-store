import {
  loadCustomers,
  loadCustomer,
  createCustomer,
  updateCustomer,
  deactivateCustomer,
  reactivateCustomer,
  loadCashbackHistory,
  loadCashbackBalance,
} from './customer.effects';

export const customerStore = {
  loadCustomers,
  loadCustomer,
  createCustomer,
  updateCustomer,
  deactivateCustomer,
  reactivateCustomer,
  loadCashbackHistory,
  loadCashbackBalance,
};
