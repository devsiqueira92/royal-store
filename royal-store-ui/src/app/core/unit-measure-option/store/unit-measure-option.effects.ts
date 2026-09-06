import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { UnitMeasureOptionService } from '../services/unit-measure-option.service';
import { UnitMeasureOptionActions } from './unit-measure-option.actions';

export const loadUnitMeasureOptions = createEffect(
  (actions$ = inject(Actions), unitMeasureOptionService = inject(UnitMeasureOptionService)) => {
    return actions$.pipe(
      ofType(UnitMeasureOptionActions.loadUnitMeasureOptions),
      switchMap(() =>
        unitMeasureOptionService.getAll().pipe(
          map(unitMeasureOptions =>
            UnitMeasureOptionActions.loadUnitMeasureOptionsSuccess({ unitMeasureOptions }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              UnitMeasureOptionActions.loadUnitMeasureOptionsFailure({
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
