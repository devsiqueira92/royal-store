import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { TranslateService } from '@ngx-translate/core';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { CurrentSellerService } from '../services/current-seller.service';
import { CurrentSellerActions } from './current-seller.actions';
import { NotificationService } from '@app/shared/services/notification.service';

const toError = (error: HttpErrorResponse): string =>
  error.error?.message ?? 'error.unknowError';

export const loadCurrentSeller = createEffect(
  (actions$ = inject(Actions), service = inject(CurrentSellerService)) => {
    return actions$.pipe(
      ofType(CurrentSellerActions.loadCurrentSeller),
      switchMap(() =>
        service.getCurrent().pipe(
          map(currentSeller =>
            CurrentSellerActions.loadCurrentSellerSuccess({ currentSeller }),
          ),
          catchError(error =>
            of(
              CurrentSellerActions.loadCurrentSellerFailure({
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

export const updateMyProfile = createEffect(
  (actions$ = inject(Actions), service = inject(CurrentSellerService)) => {
    return actions$.pipe(
      ofType(CurrentSellerActions.updateMyProfile),
      switchMap(({ request }) =>
        service.updateMyProfile(request).pipe(
          map(currentSeller =>
            CurrentSellerActions.updateMyProfileSuccess({ currentSeller }),
          ),
          catchError(error =>
            of(
              CurrentSellerActions.updateMyProfileFailure({
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

export const updateMyProfileNotify = createEffect(
  (
    actions$ = inject(Actions),
    notificationService = inject(NotificationService),
    translateService = inject(TranslateService),
  ) => {
    return actions$.pipe(
      ofType(CurrentSellerActions.updateMyProfileSuccess),
      tap(() =>
        notificationService.success({
          summary: translateService.instant('profile.form.updateSuccessSummary'),
          detail: translateService.instant('profile.form.updateSuccessDetail'),
        }),
      ),
    );
  },
  { functional: true, dispatch: false },
);
