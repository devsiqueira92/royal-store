import { createFeature, createReducer, on } from '@ngrx/store';
import { Customer } from '../interfaces/customer.interface';
import { CashbackEntry } from '../interfaces/cashback-entry.interface';
import { CustomerActions } from './customer.actions';

export interface CustomerState {
  customers: Customer[];
  selectedCustomer: Customer | null;
  cashbackHistory: CashbackEntry[];
  cashbackBalance: number | null;
  isLoading: boolean;
  isLoadingHistory: boolean;
  isLoadingBalance: boolean;
  error: string | null;
}

export const initialCustomerState: CustomerState = {
  customers: [],
  selectedCustomer: null,
  cashbackHistory: [],
  cashbackBalance: null,
  isLoading: false,
  isLoadingHistory: false,
  isLoadingBalance: false,
  error: null,
};

const setActive = (
  state: CustomerState,
  id: string,
  isActive: boolean,
): CustomerState => ({
  ...state,
  customers: state.customers.map(c => (c.id === id ? { ...c, isActive } : c)),
  selectedCustomer:
    state.selectedCustomer?.id === id
      ? { ...state.selectedCustomer, isActive }
      : state.selectedCustomer,
  isLoading: false,
});

export const customerFeature = createFeature({
  name: 'customer',
  reducer: createReducer(
    initialCustomerState,
    on(
      CustomerActions.loadCustomers,
      CustomerActions.loadCustomer,
      CustomerActions.createCustomer,
      CustomerActions.updateCustomer,
      CustomerActions.deactivateCustomer,
      CustomerActions.reactivateCustomer,
      (state): CustomerState => ({ ...state, isLoading: true, error: null }),
    ),
    on(
      CustomerActions.loadCustomersFailure,
      CustomerActions.loadCustomerFailure,
      CustomerActions.createCustomerFailure,
      CustomerActions.updateCustomerFailure,
      CustomerActions.deactivateCustomerFailure,
      CustomerActions.reactivateCustomerFailure,
      (state, { error }): CustomerState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      CustomerActions.loadCustomersSuccess,
      (state, { customers }): CustomerState => ({
        ...state,
        customers,
        isLoading: false,
      }),
    ),
    on(
      CustomerActions.loadCustomerSuccess,
      (state, { customer }): CustomerState => ({
        ...state,
        selectedCustomer: customer,
        isLoading: false,
      }),
    ),
    on(
      CustomerActions.createCustomerSuccess,
      (state, { customer }): CustomerState => ({
        ...state,
        customers: [...state.customers, customer],
        isLoading: false,
      }),
    ),
    on(
      CustomerActions.updateCustomerSuccess,
      (state, { customer }): CustomerState => ({
        ...state,
        customers: state.customers.map(c =>
          c.id === customer.id ? customer : c,
        ),
        selectedCustomer:
          state.selectedCustomer?.id === customer.id
            ? customer
            : state.selectedCustomer,
        isLoading: false,
      }),
    ),
    on(CustomerActions.deactivateCustomerSuccess, (state, { id }) =>
      setActive(state, id, false),
    ),
    on(CustomerActions.reactivateCustomerSuccess, (state, { id }) =>
      setActive(state, id, true),
    ),
    on(CustomerActions.loadCashbackHistory, (state): CustomerState => ({
      ...state,
      cashbackHistory: [],
      isLoadingHistory: true,
    })),
    on(
      CustomerActions.loadCashbackHistorySuccess,
      (state, { entries }): CustomerState => ({
        ...state,
        cashbackHistory: entries,
        isLoadingHistory: false,
      }),
    ),
    on(
      CustomerActions.loadCashbackHistoryFailure,
      (state, { error }): CustomerState => ({
        ...state,
        isLoadingHistory: false,
        error,
      }),
    ),
    on(CustomerActions.loadCashbackBalance, (state): CustomerState => ({
      ...state,
      cashbackBalance: null,
      isLoadingBalance: true,
    })),
    on(
      CustomerActions.loadCashbackBalanceSuccess,
      (state, { balance }): CustomerState => ({
        ...state,
        cashbackBalance: balance,
        isLoadingBalance: false,
      }),
    ),
    on(
      CustomerActions.loadCashbackBalanceFailure,
      (state, { error }): CustomerState => ({
        ...state,
        cashbackBalance: null,
        isLoadingBalance: false,
        error,
      }),
    ),
    on(CustomerActions.clearCashbackBalance, (state): CustomerState => ({
      ...state,
      cashbackBalance: null,
      isLoadingBalance: false,
    })),
    on(CustomerActions.clearSelectedCustomer, (state): CustomerState => ({
      ...state,
      selectedCustomer: null,
      cashbackHistory: [],
      cashbackBalance: null,
    })),
  ),
});
