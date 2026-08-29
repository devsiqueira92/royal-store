import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { PermissionService } from '../services/permission.service';
import { PermissionActions } from './permission.actions';

export const loadPermissionCatalog = createEffect(
  (
    actions$ = inject(Actions),
    permissionService = inject(PermissionService),
  ) => {
    return actions$.pipe(
      ofType(PermissionActions.loadPermissionCatalog),
      switchMap(() =>
        permissionService.getCatalog().pipe(
          map(({ resources }) =>
            PermissionActions.loadPermissionCatalogSuccess({ resources }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              PermissionActions.loadPermissionCatalogFailure({
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
