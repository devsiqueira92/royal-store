import {
  loadCustomers,
  loadCustomer,
  createCustomer,
  updateCustomer,
  deactivateCustomer,
  reactivateCustomer,
  loadCashbackHistory,
} from './customer.effects';

export const customerStore = {
  loadCustomers,
  loadCustomer,
  createCustomer,
  updateCustomer,
  deactivateCustomer,
  reactivateCustomer,
  loadCashbackHistory,
};
