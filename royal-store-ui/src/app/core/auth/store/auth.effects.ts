import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { AuthActions } from './auth.actions';

const EMAIL_NOT_VERIFIED_CODE = 'auth.email_not_verified';

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
          catchError((error: HttpErrorResponse) => {
            if (error.error?.code === EMAIL_NOT_VERIFIED_CODE) {
              return of(
                AuthActions.loginUnverified({ email: request.email }),
              );
            }

            return of(
              AuthActions.loginFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            );
          }),
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

export const loginUnverifiedRedirect = createEffect(
  (actions$ = inject(Actions), router = inject(Router)) => {
    return actions$.pipe(
      ofType(AuthActions.loginUnverified),
      tap(() => router.navigate(['/auth/verify-email'])),
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
          map(() => AuthActions.registerSuccess({ email: request.email })),
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
  (actions$ = inject(Actions), router = inject(Router)) => {
    return actions$.pipe(
      ofType(AuthActions.registerSuccess),
      tap(() => router.navigate(['/auth/verify-email'])),
    );
  },
  {
    functional: true,
    dispatch: false,
  },
);

export const verifyEmail = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.verifyEmail),
      switchMap(({ userId, token }) =>
        authService.verifyEmail(userId, token).pipe(
          map(() => AuthActions.verifyEmailSuccess()),
          catchError((error: HttpErrorResponse) =>
            of(
              AuthActions.verifyEmailFailure({
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

export const resendConfirmationEmail = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) => {
    return actions$.pipe(
      ofType(AuthActions.resendConfirmationEmail),
      switchMap(({ email }) =>
        authService.resendConfirmationEmail({ email }).pipe(
          map(response =>
            AuthActions.resendConfirmationEmailSuccess({
              message: response.message,
            }),
          ),
          catchError((error: HttpErrorResponse) =>
            of(
              AuthActions.resendConfirmationEmailFailure({
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

export const resendConfirmationEmailNotify = createEffect(
  (
    actions$ = inject(Actions),
    notificationService = inject(NotificationService),
  ) => {
    return actions$.pipe(
      ofType(AuthActions.resendConfirmationEmailSuccess),
      tap(({ message }) =>
        notificationService.success({
          summary: 'Verification email sent',
          detail: message,
        }),
      ),
    );
  },
  {
    functional: true,
    dispatch: false,
  },
);
