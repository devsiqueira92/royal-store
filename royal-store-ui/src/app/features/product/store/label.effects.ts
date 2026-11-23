import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, concatMap, map, mergeMap, of, switchMap } from 'rxjs';
import { LabelService } from '../services/label.service';
import { LabelActions } from './label.actions';

const errorMsg = (e: HttpErrorResponse): string => e.error?.message ?? 'error.unknowError';

export const loadLabels = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.loadLabels),
      switchMap(({ isActive }) =>
        service.getAll(isActive).pipe(
          map((labels) => LabelActions.loadLabelsSuccess({ labels })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.loadLabelsFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadConfigurations = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.loadConfigurations),
      switchMap(() =>
        service.getConfigurations().pipe(
          map((configurations) => LabelActions.loadConfigurationsSuccess({ configurations })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.loadConfigurationsFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadPrintOptions = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.loadPrintOptions),
      switchMap(() =>
        service.getPrintOptions().pipe(
          map((printOptions) => LabelActions.loadPrintOptionsSuccess({ printOptions })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.loadPrintOptionsFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadConfiguration = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.loadConfiguration),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map((label) => LabelActions.loadConfigurationSuccess({ label })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.loadConfigurationFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadLabel = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.loadLabel),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map((label) => LabelActions.loadLabelSuccess({ label })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.loadLabelFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const createLabel = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.createLabel),
      concatMap(({ request }) =>
        service.create(request).pipe(
          map((label) => LabelActions.createLabelSuccess({ label })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.createLabelFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const updateLabel = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.updateLabel),
      concatMap(({ id, request }) =>
        service.update(id, request).pipe(
          map((label) => LabelActions.updateLabelSuccess({ label })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.updateLabelFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const setLabelActive = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.setLabelActive),
      mergeMap(({ id, isActive }) =>
        (isActive ? service.activate(id) : service.deactivate(id)).pipe(
          map(() => LabelActions.setLabelActiveSuccess({ id, isActive })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.setLabelActiveFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deleteLabel = createEffect(
  (actions$ = inject(Actions), service = inject(LabelService)) =>
    actions$.pipe(
      ofType(LabelActions.deleteLabel),
      mergeMap(({ id }) =>
        service.delete(id).pipe(
          map(() => LabelActions.deleteLabelSuccess({ id })),
          catchError((e: HttpErrorResponse) =>
            of(LabelActions.deleteLabelFailure({ error: errorMsg(e) })),
          ),
        ),
      ),
    ),
  { functional: true },
);
