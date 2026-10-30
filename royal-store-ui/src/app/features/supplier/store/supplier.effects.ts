import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { SupplierService } from '../services/supplier.service';
import { SupplierActions } from './supplier.actions';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadSuppliers = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.loadSuppliers),
      switchMap(() =>
        service.getAll().pipe(
          map(suppliers => SupplierActions.loadSuppliersSuccess({ suppliers })),
          catchError(error =>
            of(SupplierActions.loadSuppliersFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const loadSupplier = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.loadSupplier),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map(supplier => SupplierActions.loadSupplierSuccess({ supplier })),
          catchError(error =>
            of(SupplierActions.loadSupplierFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const createSupplier = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.createSupplier),
      switchMap(({ request }) =>
        service.create(request).pipe(
          map(supplier => SupplierActions.createSupplierSuccess({ supplier })),
          catchError(error =>
            of(SupplierActions.createSupplierFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const updateSupplier = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.updateSupplier),
      switchMap(({ id, request }) =>
        service.update(id, request).pipe(
          map(supplier => SupplierActions.updateSupplierSuccess({ supplier })),
          catchError(error =>
            of(SupplierActions.updateSupplierFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const activateSupplier = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.activateSupplier),
      switchMap(({ id }) =>
        service.activate(id).pipe(
          map(() => SupplierActions.activateSupplierSuccess({ id })),
          catchError(error =>
            of(SupplierActions.activateSupplierFailure({ error: toError(error) })),
          ),
        ),
      ),
    );
  },
  { functional: true },
);

export const deactivateSupplier = createEffect(
  (actions$ = inject(Actions), service = inject(SupplierService)) => {
    return actions$.pipe(
      ofType(SupplierActions.deactivateSupplier),
      switchMap(({ id }) =>
        service.deactivate(id).pipe(
          map(() => SupplierActions.deactivateSupplierSuccess({ id })),
          catchError(error =>
            of(
              SupplierActions.deactivateSupplierFailure({
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
