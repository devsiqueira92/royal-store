import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { PaymentMethodService } from '../services/payment-method.service';
import { PaymentMethodActions } from './payment-method.actions';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadPaymentMethods = createEffect(
  (actions$ = inject(Actions), service = inject(PaymentMethodService)) => {
    return actions$.pipe(
      ofType(PaymentMethodActions.loadPaymentMethods),
      switchMap(() =>
        service.getAll().pipe(
          map(paymentMethods =>
            PaymentMethodActions.loadPaymentMethodsSuccess({ paymentMethods }),
          ),
          catchError(error =>
            of(
              PaymentMethodActions.loadPaymentMethodsFailure({
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

export const updatePaymentDiscount = createEffect(
  (actions$ = inject(Actions), service = inject(PaymentMethodService)) => {
    return actions$.pipe(
      ofType(PaymentMethodActions.updatePaymentDiscount),
      switchMap(({ id, request }) =>
        service.updateDiscount(id, request).pipe(
          map(paymentMethod =>
            PaymentMethodActions.updatePaymentDiscountSuccess({
              paymentMethod,
            }),
          ),
          catchError(error =>
            of(
              PaymentMethodActions.updatePaymentDiscountFailure({
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

export const activatePaymentMethod = createEffect(
  (actions$ = inject(Actions), service = inject(PaymentMethodService)) => {
    return actions$.pipe(
      ofType(PaymentMethodActions.activatePaymentMethod),
      switchMap(({ id }) =>
        service.activate(id).pipe(
          map(paymentMethod =>
            PaymentMethodActions.activatePaymentMethodSuccess({
              paymentMethod,
            }),
          ),
          catchError(error =>
            of(
              PaymentMethodActions.activatePaymentMethodFailure({
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

export const deactivatePaymentMethod = createEffect(
  (actions$ = inject(Actions), service = inject(PaymentMethodService)) => {
    return actions$.pipe(
      ofType(PaymentMethodActions.deactivatePaymentMethod),
      switchMap(({ id }) =>
        service.deactivate(id).pipe(
          map(paymentMethod =>
            PaymentMethodActions.deactivatePaymentMethodSuccess({
              paymentMethod,
            }),
          ),
          catchError(error =>
            of(
              PaymentMethodActions.deactivatePaymentMethodFailure({
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
