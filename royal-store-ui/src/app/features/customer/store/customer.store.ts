import {
  loadCustomers,
  loadCustomer,
  createCustomer,
  updateCustomer,
  deactivateCustomer,
  reactivateCustomer,
  loadCashbackHistory,
  loadCashbackBalance,
  searchCustomers,
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
  searchCustomers,
};
