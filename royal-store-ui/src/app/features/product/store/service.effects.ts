import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { ServiceService } from '../services/service.service';
import { ServiceActions } from './service.actions';

export const createService = createEffect(
  (actions$ = inject(Actions), serviceService = inject(ServiceService)) => {
    return actions$.pipe(
      ofType(ServiceActions.createService),
      switchMap(({ request }) =>
        serviceService.create(request).pipe(
          map(product => ServiceActions.createServiceSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ServiceActions.createServiceFailure({
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

export const saveServiceEdit = createEffect(
  (actions$ = inject(Actions), serviceService = inject(ServiceService)) => {
    return actions$.pipe(
      ofType(ServiceActions.saveServiceEdit),
      switchMap(({ id: productId, request }) => {
        return serviceService.update(productId, request).pipe(
          map(product => ServiceActions.updateServiceSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ServiceActions.updateServiceFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        );
      }),
    );
  },
  {
    functional: true,
  },
);
