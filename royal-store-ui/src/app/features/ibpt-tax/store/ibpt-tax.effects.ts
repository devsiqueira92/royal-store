import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { IbptTaxService } from '../services/ibpt-tax.service';
import { IbptTaxActions } from './ibpt-tax.actions';

export const searchIbptTaxes = createEffect(
  (actions$ = inject(Actions), ibptTaxService = inject(IbptTaxService)) => {
    return actions$.pipe(
      ofType(IbptTaxActions.searchIbptTaxes),
      // switchMap: a new keystroke aborts the in-flight request.
      switchMap(({ q, taxType }) =>
        ibptTaxService.search(q, taxType).pipe(
          map(results => IbptTaxActions.searchIbptTaxesSuccess({ results })),
          catchError((error: HttpErrorResponse) =>
            of(
              IbptTaxActions.searchIbptTaxesFailure({
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
