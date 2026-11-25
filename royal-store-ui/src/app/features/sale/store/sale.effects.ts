import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { catchError, filter, map, of, switchMap, withLatestFrom } from 'rxjs';
import { SaleService } from '../services/sale.service';
import { CashShiftActions } from '../../cash-shift/store/cash-shift.actions';
import { SaleActions } from './sale.actions';
import { selectSalesList } from './sale.selectors';

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

export const refreshCashShiftAfterSale = createEffect(
  (actions$ = inject(Actions)) => {
    return actions$.pipe(
      ofType(SaleActions.registerSaleSuccess),
      map(() => CashShiftActions.refreshCurrent()),
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

/**
 * First page and next pages of the sales history (067). One stream with switchMap: a restart (new
 * filters) cancels a next page still in flight, so no page of the old filters is ever appended.
 * The reducer has already run, so the slice read here holds the new filters / the pending cursor.
 */
export const loadSales = createEffect(
  (
    actions$ = inject(Actions),
    store = inject(Store),
    saleService = inject(SaleService),
  ) => {
    return actions$.pipe(
      ofType(SaleActions.loadSales, SaleActions.loadMoreSales),
      withLatestFrom(store.select(selectSalesList)),
      // A next page needs a pending cursor; the reducer leaves isLoadingMore false otherwise.
      filter(
        ([action, list]) =>
          action.type === SaleActions.loadSales.type || list.isLoadingMore,
      ),
      switchMap(([action, list]) => {
        const append = action.type === SaleActions.loadMoreSales.type;
        const page = append ? { cursor: list.nextCursor ?? undefined } : undefined;
        return saleService.getAll(list.params, page).pipe(
          map(result => SaleActions.loadSalesSuccess({ page: result, append })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.loadSalesFailure({
                error: error.error?.message ?? 'error.unknowError',
                append,
              }),
            ),
          ),
        );
      }),
    );
  },
  {
    functional: true,
  },
);

export const loadSaleDetails = createEffect(
  (actions$ = inject(Actions), saleService = inject(SaleService)) => {
    return actions$.pipe(
      ofType(SaleActions.loadSaleDetails),
      switchMap(({ saleId }) =>
        saleService.getById(saleId).pipe(
          map(saleDetails => SaleActions.loadSaleDetailsSuccess({ saleDetails })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.loadSaleDetailsFailure({
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

export const loadSaleReceipt = createEffect(
  (actions$ = inject(Actions), saleService = inject(SaleService)) => {
    return actions$.pipe(
      ofType(SaleActions.loadSaleReceipt),
      switchMap(({ saleId }) =>
        saleService.getReceipt(saleId).pipe(
          map(saleReceipt => SaleActions.loadSaleReceiptSuccess({ saleReceipt })),
          catchError((error: HttpErrorResponse) =>
            of(
              SaleActions.loadSaleReceiptFailure({
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
