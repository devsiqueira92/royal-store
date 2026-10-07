import { createFeature, createReducer, on } from '@ngrx/store';
import { CashShift } from '../interfaces/cash-shift.interface';
import { CashShiftActions } from './cash-shift.actions';

export interface CashShiftState {
  current: CashShift | null;
  currentLoaded: boolean;
  isOpening: boolean;
  isClosing: boolean;
  error: string | null;
}

export const initialCashShiftState: CashShiftState = {
  current: null,
  currentLoaded: false,
  isOpening: false,
  isClosing: false,
  error: null,
};

export const cashShiftFeature = createFeature({
  name: 'cashShift',
  reducer: createReducer(
    initialCashShiftState,
    on(CashShiftActions.loadCurrent, (state): CashShiftState => ({
      ...state,
      currentLoaded: false,
      error: null,
    })),
    on(CashShiftActions.loadCurrentSuccess, (state, { cashShift }): CashShiftState => ({
      ...state,
      current: cashShift,
      currentLoaded: true,
    })),
    on(CashShiftActions.loadCurrentFailure, (state, { error }): CashShiftState => ({
      ...state,
      error,
    })),
    on(CashShiftActions.openCashShift, (state): CashShiftState => ({
      ...state,
      isOpening: true,
      error: null,
    })),
    on(CashShiftActions.openCashShiftSuccess, (state, { cashShift }): CashShiftState => ({
      ...state,
      current: cashShift,
      isOpening: false,
    })),
    on(CashShiftActions.openCashShiftFailure, (state, { error }): CashShiftState => ({
      ...state,
      isOpening: false,
      error,
    })),
    on(CashShiftActions.closeCashShift, (state): CashShiftState => ({
      ...state,
      isClosing: true,
      error: null,
    })),
    on(CashShiftActions.closeCashShiftSuccess, (state): CashShiftState => ({
      ...state,
      current: null,
      isClosing: false,
    })),
    on(CashShiftActions.closeCashShiftFailure, (state, { error }): CashShiftState => ({
      ...state,
      isClosing: false,
      error,
    })),
  ),
});
