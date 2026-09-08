import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { LoyaltyTierService } from '../services/loyalty-tier.service';
import { LoyaltyTierActions } from './loyalty-tier.actions';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadLoyaltyTiers = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.loadLoyaltyTiers),
      switchMap(() =>
        service.getAll().pipe(
          map(loyaltyTiers =>
            LoyaltyTierActions.loadLoyaltyTiersSuccess({ loyaltyTiers }),
          ),
          catchError(error =>
            of(
              LoyaltyTierActions.loadLoyaltyTiersFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const loadLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.loadLoyaltyTier),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map(loyaltyTier =>
            LoyaltyTierActions.loadLoyaltyTierSuccess({ loyaltyTier }),
          ),
          catchError(error =>
            of(
              LoyaltyTierActions.loadLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const createLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.createLoyaltyTier),
      switchMap(({ request }) =>
        service.create(request).pipe(
          map(loyaltyTier =>
            LoyaltyTierActions.createLoyaltyTierSuccess({ loyaltyTier }),
          ),
          catchError(error =>
            of(
              LoyaltyTierActions.createLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.updateLoyaltyTier),
      switchMap(({ id, request }) =>
        service.update(id, request).pipe(
          map(loyaltyTier =>
            LoyaltyTierActions.updateLoyaltyTierSuccess({ loyaltyTier }),
          ),
          catchError(error =>
            of(
              LoyaltyTierActions.updateLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const deleteLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.deleteLoyaltyTier),
      switchMap(({ id }) =>
        service.delete(id).pipe(
          map(() => LoyaltyTierActions.deleteLoyaltyTierSuccess({ id })),
          catchError(error =>
            of(
              LoyaltyTierActions.deleteLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const activateLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.activateLoyaltyTier),
      switchMap(({ id }) =>
        service.activate(id).pipe(
          map(() => LoyaltyTierActions.activateLoyaltyTierSuccess({ id })),
          catchError(error =>
            of(
              LoyaltyTierActions.activateLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const deactivateLoyaltyTier = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.deactivateLoyaltyTier),
      switchMap(({ id }) =>
        service.deactivate(id).pipe(
          map(() => LoyaltyTierActions.deactivateLoyaltyTierSuccess({ id })),
          catchError(error =>
            of(
              LoyaltyTierActions.deactivateLoyaltyTierFailure({
                error: toError(error),
              }),
            ),
          ),
        ),
      ),
    );
  },
  { functional: true },
);
