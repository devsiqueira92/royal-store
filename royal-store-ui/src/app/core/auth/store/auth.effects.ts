import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AuthActions } from './auth.actions';

export const checkAuthProfile = createEffect(
  (
    actions$ = inject(Actions),
    authService = inject(AuthService),
    router = inject(Router),
  ) => {
    return actions$.pipe(
      ofType(AuthActions.checkAuthProfile),
      switchMap(() =>
        authService.getUserProfile().pipe(
          map(user => AuthActions.checkAuthProfileSuccess({ user })),
          catchError(() => {
            router.navigate(['/auth/login']);
            return of(AuthActions.checkAuthProfileFailure());
          }),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const login = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ request }) =>
        authService.login(request).pipe(
          map(response => AuthActions.loginSuccess({ response })),
          catchError((error: HttpErrorResponse) =>
            of(
              AuthActions.loginFailure({
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

export const loginSuccessProfile = createEffect(
  (actions$ = inject(Actions)) => {
    return actions$.pipe(
      ofType(AuthActions.loginSuccess),
      map(() => AuthActions.checkAuthProfile()),
    );
  },
  {
    functional: true,
  },
);

export const loginSuccessRedirect = createEffect(
  (actions$ = inject(Actions), router = inject(Router)) => {
    return actions$.pipe(
      ofType(AuthActions.loginSuccess),
      tap(() => router.navigate(['/'])),
    );
  },
  {
    functional: true,
    dispatch: false,
  },
);
