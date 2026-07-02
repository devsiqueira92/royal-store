import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../../../shared/services/notification.service';
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

export const register = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.register),
      switchMap(({ request }) =>
        authService.register(request).pipe(
          map(() => AuthActions.registerSuccess()),
          catchError((error: HttpErrorResponse) =>
            of(
              AuthActions.registerFailure({
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

export const registerSuccessRedirect = createEffect(
  (
    actions$ = inject(Actions),
    router = inject(Router),
    notificationService = inject(NotificationService),
  ) => {
    return actions$.pipe(
      ofType(AuthActions.registerSuccess),
      tap(() => {
        notificationService.success({
          summary: 'Account created',
          detail: 'Your account was created. Please check your email to verify it.',
        });
        router.navigate(['/auth/login']);
      }),
    );
  },
  {
    functional: true,
    dispatch: false,
  },
);
