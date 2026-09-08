import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { LoyaltyTierService } from '../services/loyalty-tier.service';
import { LoyaltyTierActions } from './loyalty-tier.actions';

export const loadLoyaltyTiers = createEffect(
  (actions$ = inject(Actions), service = inject(LoyaltyTierService)) => {
    return actions$.pipe(
      ofType(LoyaltyTierActions.loadLoyaltyTiers),
      switchMap(() =>
        service.getAll().pipe(
          map(loyaltyTiers =>
            LoyaltyTierActions.loadLoyaltyTiersSuccess({ loyaltyTiers }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              LoyaltyTierActions.loadLoyaltyTiersFailure({
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
