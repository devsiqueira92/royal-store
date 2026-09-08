import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { CustomerService } from '../services/customer.service';
import { CustomerActions } from './customer.actions';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadCustomers = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.loadCustomers),
      switchMap(() =>
        customerService.getAll().pipe(
          map(customers => CustomerActions.loadCustomersSuccess({ customers })),
          catchError(error =>
            of(CustomerActions.loadCustomersFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const loadCustomer = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.loadCustomer),
      switchMap(({ id }) =>
        customerService.getById(id).pipe(
          map(customer => CustomerActions.loadCustomerSuccess({ customer })),
          catchError(error =>
            of(CustomerActions.loadCustomerFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const createCustomer = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.createCustomer),
      switchMap(({ request }) =>
        customerService.create(request).pipe(
          map(customer => CustomerActions.createCustomerSuccess({ customer })),
          catchError(error =>
            of(
              CustomerActions.createCustomerFailure({ error: toError(error) }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const updateCustomer = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.updateCustomer),
      switchMap(({ id, request }) =>
        customerService.update(id, request).pipe(
          map(customer => CustomerActions.updateCustomerSuccess({ customer })),
          catchError(error =>
            of(
              CustomerActions.updateCustomerFailure({ error: toError(error) }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const deactivateCustomer = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.deactivateCustomer),
      switchMap(({ id }) =>
        customerService.deactivate(id).pipe(
          map(() => CustomerActions.deactivateCustomerSuccess({ id })),
          catchError(error =>
            of(
              CustomerActions.deactivateCustomerFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const reactivateCustomer = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.reactivateCustomer),
      switchMap(({ id }) =>
        customerService.reactivate(id).pipe(
          map(() => CustomerActions.reactivateCustomerSuccess({ id })),
          catchError(error =>
            of(
              CustomerActions.reactivateCustomerFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const loadCashbackHistory = createEffect(
  (actions$ = inject(Actions), customerService = inject(CustomerService)) => {
    return actions$.pipe(
      ofType(CustomerActions.loadCashbackHistory),
      switchMap(({ id }) =>
        customerService.getCashbackHistory(id).pipe(
          map(entries =>
            CustomerActions.loadCashbackHistorySuccess({ entries }),
          ),
          catchError(error =>
            of(
              CustomerActions.loadCashbackHistoryFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);
