import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { PriceListService } from '../services/price-list.service';
import { PriceListActions } from './price-list.actions';

const errorMsg = (e: HttpErrorResponse): string =>
  e.error?.message ?? 'error.unknowError';

export const loadPriceLists = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.loadPriceLists),
      switchMap(() =>
        service.getAll().pipe(
          map(priceLists =>
            PriceListActions.loadPriceListsSuccess({ priceLists }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(PriceListActions.loadPriceListsFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const createPriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.createPriceList),
      switchMap(({ request }) =>
        service.create(request).pipe(
          map(priceList =>
            PriceListActions.createPriceListSuccess({ priceList }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(PriceListActions.createPriceListFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const setDefaultPriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.setDefaultPriceList),
      switchMap(({ id }) =>
        service.setDefault(id).pipe(
          map(priceList =>
            PriceListActions.setDefaultPriceListSuccess({ priceList }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              PriceListActions.setDefaultPriceListFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const activatePriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.activatePriceList),
      switchMap(({ id }) =>
        service.activate(id).pipe(
          map(() => PriceListActions.activatePriceListSuccess({ id })),
          catchError((e: HttpErrorResponse) =>
            of(
              PriceListActions.activatePriceListFailure({ error: errorMsg(e) }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deactivatePriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.deactivatePriceList),
      switchMap(({ id }) =>
        service.deactivate(id).pipe(
          map(() => PriceListActions.deactivatePriceListSuccess({ id })),
          catchError((e: HttpErrorResponse) =>
            of(
              PriceListActions.deactivatePriceListFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deletePriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.deletePriceList),
      switchMap(({ id }) =>
        service.delete(id).pipe(
          map(() => PriceListActions.deletePriceListSuccess({ id })),
          catchError((e: HttpErrorResponse) =>
            of(PriceListActions.deletePriceListFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const updatePriceList = createEffect(
  (actions$ = inject(Actions), service = inject(PriceListService)) =>
    actions$.pipe(
      ofType(PriceListActions.updatePriceList),
      switchMap(({ id, request }) =>
        service.update(id, request).pipe(
          map(priceList =>
            PriceListActions.updatePriceListSuccess({ priceList }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              PriceListActions.updatePriceListFailure({ error: errorMsg(e) }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);
