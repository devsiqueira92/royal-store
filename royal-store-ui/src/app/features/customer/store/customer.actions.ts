import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Customer } from '../interfaces/customer.interface';
import { CreateCustomerRequest } from '../interfaces/create-customer-request.interface';
import { UpdateCustomerRequest } from '../interfaces/update-customer-request.interface';
import { CashbackEntry } from '../interfaces/cashback-entry.interface';

export const CustomerActions = createActionGroup({
  source: 'Customer',
  events: {
    'Load Customers': emptyProps(),
    'Load Customers Success': props<{ customers: Customer[] }>(),
    'Load Customers Failure': props<{ error: string }>(),
    'Load Customer': props<{ id: string }>(),
    'Load Customer Success': props<{ customer: Customer }>(),
    'Load Customer Failure': props<{ error: string }>(),
    'Create Customer': props<{ request: CreateCustomerRequest }>(),
    'Create Customer Success': props<{ customer: Customer }>(),
    'Create Customer Failure': props<{ error: string }>(),
    'Update Customer': props<{ id: string; request: UpdateCustomerRequest }>(),
    'Update Customer Success': props<{ customer: Customer }>(),
    'Update Customer Failure': props<{ error: string }>(),
    'Deactivate Customer': props<{ id: string }>(),
    'Deactivate Customer Success': props<{ id: string }>(),
    'Deactivate Customer Failure': props<{ error: string }>(),
    'Reactivate Customer': props<{ id: string }>(),
    'Reactivate Customer Success': props<{ id: string }>(),
    'Reactivate Customer Failure': props<{ error: string }>(),
    'Load Cashback History': props<{ id: string }>(),
    'Load Cashback History Success': props<{ entries: CashbackEntry[] }>(),
    'Load Cashback History Failure': props<{ error: string }>(),
    'Load Cashback Balance': props<{ id: string }>(),
    'Load Cashback Balance Success': props<{ balance: number }>(),
    'Load Cashback Balance Failure': props<{ error: string }>(),
    'Clear Cashback Balance': emptyProps(),
    'Clear Selected Customer': emptyProps(),
  },
});
