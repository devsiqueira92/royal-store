import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { TaxProfileTemplateService } from '../services/tax-profile-template.service';
import { TaxProfileTemplateActions } from './tax-profile-template.actions';

export const loadTemplates = createEffect(
  (
    actions$ = inject(Actions),
    templateService = inject(TaxProfileTemplateService),
  ) => {
    return actions$.pipe(
      ofType(TaxProfileTemplateActions.loadTemplates),
      switchMap(({ itemType }) =>
        templateService.getAll(itemType).pipe(
          map(templates =>
            TaxProfileTemplateActions.loadTemplatesSuccess({ templates }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              TaxProfileTemplateActions.loadTemplatesFailure({
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

export const createTemplate = createEffect(
  (
    actions$ = inject(Actions),
    templateService = inject(TaxProfileTemplateService),
  ) => {
    return actions$.pipe(
      ofType(TaxProfileTemplateActions.createTemplate),
      switchMap(({ request }) =>
        templateService.create(request).pipe(
          map(template =>
            TaxProfileTemplateActions.createTemplateSuccess({ template }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              TaxProfileTemplateActions.createTemplateFailure({
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

export const updateTemplate = createEffect(
  (
    actions$ = inject(Actions),
    templateService = inject(TaxProfileTemplateService),
  ) => {
    return actions$.pipe(
      ofType(TaxProfileTemplateActions.updateTemplate),
      switchMap(({ id, request }) =>
        templateService.update(id, request).pipe(
          map(template =>
            TaxProfileTemplateActions.updateTemplateSuccess({ template }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              TaxProfileTemplateActions.updateTemplateFailure({
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

export const deactivateTemplate = createEffect(
  (
    actions$ = inject(Actions),
    templateService = inject(TaxProfileTemplateService),
  ) => {
    return actions$.pipe(
      ofType(TaxProfileTemplateActions.deactivateTemplate),
      switchMap(({ id }) =>
        templateService.deactivate(id).pipe(
          map(() =>
            TaxProfileTemplateActions.deactivateTemplateSuccess({ id }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              TaxProfileTemplateActions.deactivateTemplateFailure({
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
