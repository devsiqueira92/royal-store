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

export const registerSale = createEffect(
  (actions$ = inject(Actions), saleService = inject(SaleService)) => {
    return actions$.pipe(
      ofType(SaleActions.registerSale),
      switchMap(({ request }) =>
        saleService.registerSale(request).pipe(
          map(response => SaleActions.registerSaleSuccess({ response })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.registerSaleFailure({
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

export const loadPosProducts = createEffect(
  (actions$ = inject(Actions), saleService = inject(SaleService)) => {
    return actions$.pipe(
      ofType(SaleActions.loadPosProducts),
      switchMap(({ categoryId }) =>
        saleService.getProducts(categoryId).pipe(
          map(products => SaleActions.loadPosProductsSuccess({ products })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.loadPosProductsFailure({
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
