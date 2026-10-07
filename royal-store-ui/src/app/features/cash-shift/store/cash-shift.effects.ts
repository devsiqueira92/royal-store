import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { CashShiftService } from '../services/cash-shift.service';
import { CashShiftActions } from './cash-shift.actions';

export const loadCurrentCashShift = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.loadCurrent),
      switchMap(() =>
        cashShiftService.getCurrent().pipe(
          map(cashShift => CashShiftActions.loadCurrentSuccess({ cashShift })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.loadCurrentFailure({
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

export const openCashShift = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.openCashShift),
      switchMap(({ request }) =>
        cashShiftService.open(request).pipe(
          map(cashShift => CashShiftActions.openCashShiftSuccess({ cashShift })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.openCashShiftFailure({
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

export const closeCashShift = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.closeCashShift),
      switchMap(({ request }) =>
        cashShiftService.close(request).pipe(
          map(cashShift => CashShiftActions.closeCashShiftSuccess({ cashShift })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.closeCashShiftFailure({
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
