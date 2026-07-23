import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { SellerService } from '../services/seller.service';
import { SellerActions } from './seller.actions';

export const loadSellers = createEffect(
  (actions$ = inject(Actions), sellerService = inject(SellerService)) => {
    return actions$.pipe(
      ofType(SellerActions.loadSellers),
      switchMap(({ params }) =>
        sellerService.getAll(params).pipe(
          map(sellers => SellerActions.loadSellersSuccess({ sellers })),
          catchError((error: HttpErrorResponse) =>
            of(
              SellerActions.loadSellersFailure({
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

export const loadSeller = createEffect(
  (actions$ = inject(Actions), sellerService = inject(SellerService)) => {
    return actions$.pipe(
      ofType(SellerActions.loadSeller),
      switchMap(({ id }) =>
        sellerService.getById(id).pipe(
          map(seller => SellerActions.loadSellerSuccess({ seller })),
          catchError((error: HttpErrorResponse) =>
            of(
              SellerActions.loadSellerFailure({
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

export const createSeller = createEffect(
  (actions$ = inject(Actions), sellerService = inject(SellerService)) => {
    return actions$.pipe(
      ofType(SellerActions.createSeller),
      switchMap(({ request }) =>
        sellerService.create(request).pipe(
          map(seller => SellerActions.createSellerSuccess({ seller })),
          catchError((error: HttpErrorResponse) =>
            of(
              SellerActions.createSellerFailure({
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

export const updateSeller = createEffect(
  (actions$ = inject(Actions), sellerService = inject(SellerService)) => {
    return actions$.pipe(
      ofType(SellerActions.updateSeller),
      switchMap(({ id, request }) =>
        sellerService.update(id, request).pipe(
          map(seller => SellerActions.updateSellerSuccess({ seller })),
          catchError((error: HttpErrorResponse) =>
            of(
              SellerActions.updateSellerFailure({
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

export const deactivateSeller = createEffect(
  (actions$ = inject(Actions), sellerService = inject(SellerService)) => {
    return actions$.pipe(
      ofType(SellerActions.deactivateSeller),
      switchMap(({ id }) =>
        sellerService.deactivate(id).pipe(
          map(() => SellerActions.deactivateSellerSuccess({ id })),
          catchError((error: HttpErrorResponse) =>
            of(
              SellerActions.deactivateSellerFailure({
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
