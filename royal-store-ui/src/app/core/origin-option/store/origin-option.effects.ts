import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { OriginOptionService } from '../services/origin-option.service';
import { OriginOptionActions } from './origin-option.actions';

export const loadOriginOptions = createEffect(
  (actions$ = inject(Actions), originOptionService = inject(OriginOptionService)) => {
    return actions$.pipe(
      ofType(OriginOptionActions.loadOriginOptions),
      switchMap(() =>
        originOptionService.getAll().pipe(
          map(originOptions => OriginOptionActions.loadOriginOptionsSuccess({ originOptions })),
          catchError((error: HttpErrorResponse) =>
            of(
              OriginOptionActions.loadOriginOptionsFailure({
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
