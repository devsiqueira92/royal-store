import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { CurrentSellerService } from '../services/current-seller.service';
import { CurrentSellerActions } from './current-seller.actions';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadCurrentSeller = createEffect(
  (actions$ = inject(Actions), service = inject(CurrentSellerService)) => {
    return actions$.pipe(
      ofType(CurrentSellerActions.loadCurrentSeller),
      switchMap(() =>
        service.getCurrent().pipe(
          map(currentSeller =>
            CurrentSellerActions.loadCurrentSellerSuccess({ currentSeller }),
          ),
          catchError(error =>
            of(
              CurrentSellerActions.loadCurrentSellerFailure({
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
