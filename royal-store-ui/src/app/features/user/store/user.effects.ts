import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { UserService } from '../services/user.service';
import { UserActions } from './user.actions';

export const createUser = createEffect(
  (actions$ = inject(Actions), userService = inject(UserService)) => {
    return actions$.pipe(
      ofType(UserActions.createUser),
      switchMap(({ request }) =>
        userService.create(request).pipe(
          map(response => UserActions.createUserSuccess({ response })),
          catchError((error: HttpErrorResponse) =>
            of(
              UserActions.createUserFailure({
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
