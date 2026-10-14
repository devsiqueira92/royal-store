import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, concatMap, map, of, switchMap } from 'rxjs';
import { CashShiftService } from '../services/cash-shift.service';
import { CashShiftActions } from './cash-shift.actions';

export const loadCashShifts = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.loadShifts),
      switchMap(() =>
        cashShiftService.getAll().pipe(
          map(shifts => CashShiftActions.loadShiftsSuccess({ shifts })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.loadShiftsFailure({
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

export const loadCashShiftDetails = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.loadShiftDetails),
      switchMap(({ shiftId }) =>
        cashShiftService.getById(shiftId).pipe(
          map(shiftDetails => CashShiftActions.loadShiftDetailsSuccess({ shiftDetails })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.loadShiftDetailsFailure({
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

// refreshCurrent keeps currentLoaded untouched so the layout does not tear down the page.
export const loadCurrentCashShift = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.loadCurrent, CashShiftActions.refreshCurrent),
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

export const loadCashShiftMovements = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.loadMovements),
      switchMap(({ shiftId }) =>
        cashShiftService.getMovements(shiftId).pipe(
          map(movements => CashShiftActions.loadMovementsSuccess({ shiftId, movements })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.loadMovementsFailure({
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

// concatMap: writes must not be cancelled by a later dispatch.
export const recordSupplement = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.recordSupplement),
      concatMap(({ request }) =>
        cashShiftService.recordSupplement(request).pipe(
          map(movement => CashShiftActions.recordCashMovementSuccess({ movement })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.recordCashMovementFailure({
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

export const recordWithdrawal = createEffect(
  (actions$ = inject(Actions), cashShiftService = inject(CashShiftService)) => {
    return actions$.pipe(
      ofType(CashShiftActions.recordWithdrawal),
      concatMap(({ request }) =>
        cashShiftService.recordWithdrawal(request).pipe(
          map(movement => CashShiftActions.recordCashMovementSuccess({ movement })),
          catchError((error: HttpErrorResponse) =>
            of(
              CashShiftActions.recordCashMovementFailure({
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
