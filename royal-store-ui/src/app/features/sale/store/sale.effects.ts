import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { SaleService } from '../services/sale.service';
import { SaleActions } from './sale.actions';

export const syncSales = createEffect(
  (actions$ = inject(Actions), saleService = inject(SaleService)) => {
    return actions$.pipe(
      ofType(SaleActions.syncSales),
      switchMap(({ request }) =>
        saleService.sync(request).pipe(
          map(response => SaleActions.syncSalesSuccess({ response })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.syncSalesFailure({
                error: error.error?.message ?? 'error.unknowError',
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
