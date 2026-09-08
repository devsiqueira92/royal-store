import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateCustomerRequest } from '../interfaces/create-customer-request.interface';
import { UpdateCustomerRequest } from '../interfaces/update-customer-request.interface';
import { CustomerActions } from '../store/customer.actions';
import {
  selectCustomers,
  selectSelectedCustomer,
  selectCashbackHistory,
  selectCashbackBalance,
  selectIsLoading,
  selectIsLoadingHistory,
  selectIsLoadingBalance,
  selectError,
} from '../store/customer.selectors';

@Injectable({
  providedIn: 'root',
})
export class CustomerFacade {
  private store = inject(Store);

  customers$ = this.store.select(selectCustomers);
  selectedCustomer$ = this.store.select(selectSelectedCustomer);
  cashbackHistory$ = this.store.select(selectCashbackHistory);
  cashbackBalance$ = this.store.select(selectCashbackBalance);
  isLoading$ = this.store.select(selectIsLoading);
  isLoadingHistory$ = this.store.select(selectIsLoadingHistory);
  isLoadingBalance$ = this.store.select(selectIsLoadingBalance);
  error$ = this.store.select(selectError);

  loadCustomers(): void {
    this.store.dispatch(CustomerActions.loadCustomers());
  }

  loadCustomer(id: string): void {
    this.store.dispatch(CustomerActions.loadCustomer({ id }));
  }

  createCustomer(request: CreateCustomerRequest): void {
    this.store.dispatch(CustomerActions.createCustomer({ request }));
  }

  updateCustomer(id: string, request: UpdateCustomerRequest): void {
    this.store.dispatch(CustomerActions.updateCustomer({ id, request }));
  }

  deactivateCustomer(id: string): void {
    this.store.dispatch(CustomerActions.deactivateCustomer({ id }));
  }

  reactivateCustomer(id: string): void {
    this.store.dispatch(CustomerActions.reactivateCustomer({ id }));
  }

  loadCashbackHistory(id: string): void {
    this.store.dispatch(CustomerActions.loadCashbackHistory({ id }));
  }

  loadCashbackBalance(id: string): void {
    this.store.dispatch(CustomerActions.loadCashbackBalance({ id }));
  }

  clearCashbackBalance(): void {
    this.store.dispatch(CustomerActions.clearCashbackBalance());
  }

  clearSelectedCustomer(): void {
    this.store.dispatch(CustomerActions.clearSelectedCustomer());
  }
}
