import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { CostCompositionService } from '../services/cost-composition.service';
import { CostCompositionActions } from './cost-composition.actions';

const errorMsg = (e: HttpErrorResponse): string =>
  e.error?.message ?? 'error.unknowError';

export const loadCostCompositions = createEffect(
  (actions$ = inject(Actions), service = inject(CostCompositionService)) =>
    actions$.pipe(
      ofType(CostCompositionActions.loadCostCompositions),
      switchMap(() =>
        service.getAll().pipe(
          map(costCompositions =>
            CostCompositionActions.loadCostCompositionsSuccess({
              costCompositions,
            }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              CostCompositionActions.loadCostCompositionsFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadCostComposition = createEffect(
  (actions$ = inject(Actions), service = inject(CostCompositionService)) =>
    actions$.pipe(
      ofType(CostCompositionActions.loadCostComposition),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map(costComposition =>
            CostCompositionActions.loadCostCompositionSuccess({
              costComposition,
            }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              CostCompositionActions.loadCostCompositionFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const createCostComposition = createEffect(
  (actions$ = inject(Actions), service = inject(CostCompositionService)) =>
    actions$.pipe(
      ofType(CostCompositionActions.createCostComposition),
      switchMap(({ request }) =>
        service.create(request).pipe(
          map(costComposition =>
            CostCompositionActions.createCostCompositionSuccess({
              costComposition,
            }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              CostCompositionActions.createCostCompositionFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const updateCostComposition = createEffect(
  (actions$ = inject(Actions), service = inject(CostCompositionService)) =>
    actions$.pipe(
      ofType(CostCompositionActions.updateCostComposition),
      switchMap(({ id, request }) =>
        service.update(id, request).pipe(
          map(costComposition =>
            CostCompositionActions.updateCostCompositionSuccess({
              costComposition,
            }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              CostCompositionActions.updateCostCompositionFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deleteCostComposition = createEffect(
  (actions$ = inject(Actions), service = inject(CostCompositionService)) =>
    actions$.pipe(
      ofType(CostCompositionActions.deleteCostComposition),
      switchMap(({ id }) =>
        service.delete(id).pipe(
          map(() =>
            CostCompositionActions.deleteCostCompositionSuccess({ id }),
          ),
          catchError((e: HttpErrorResponse) =>
            of(
              CostCompositionActions.deleteCostCompositionFailure({
                error: errorMsg(e),
              }),
            ),
          ),
        ),
      ),
    ),
  { functional: true },
);
