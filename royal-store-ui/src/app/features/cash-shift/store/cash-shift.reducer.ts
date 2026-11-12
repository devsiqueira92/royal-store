import { createFeature, createReducer, on } from '@ngrx/store';
import { CashShift } from '../interfaces/cash-shift.interface';
import { CashMovementResponse } from '../interfaces/cash-movement-response.interface';
import { CashShiftListItem } from '../interfaces/cash-shift-list-item.interface';
import { CashShiftDetails } from '../interfaces/cash-shift-details.interface';
import { CashShiftActions } from './cash-shift.actions';

export interface CashShiftState {
  shifts: CashShiftListItem[];
  isLoadingShifts: boolean;
  shiftDetails: CashShiftDetails | null;
  isLoadingShiftDetails: boolean;
  current: CashShift | null;
  currentLoaded: boolean;
  isOpening: boolean;
  isClosing: boolean;
  movements: CashMovementResponse[];
  movementsShiftId: string | null;
  isLoadingMovements: boolean;
  isRecordingMovement: boolean;
  lastRecordedMovement: CashMovementResponse | null;
  error: string | null;
}

export const initialCashShiftState: CashShiftState = {
  shifts: [],
  isLoadingShifts: false,
  shiftDetails: null,
  isLoadingShiftDetails: false,
  current: null,
  currentLoaded: false,
  isOpening: false,
  isClosing: false,
  movements: [],
  movementsShiftId: null,
  isLoadingMovements: false,
  isRecordingMovement: false,
  lastRecordedMovement: null,
  error: null,
};

export const cashShiftFeature = createFeature({
  name: 'cashShift',
  reducer: createReducer(
    initialCashShiftState,
    on(CashShiftActions.loadShifts, (state): CashShiftState => ({
      ...state,
      isLoadingShifts: true,
      error: null,
    })),
    on(CashShiftActions.loadShiftsSuccess, (state, { shifts }): CashShiftState => ({
      ...state,
      shifts,
      isLoadingShifts: false,
    })),
    on(CashShiftActions.loadShiftsFailure, (state, { error }): CashShiftState => ({
      ...state,
      isLoadingShifts: false,
      error,
    })),
    on(CashShiftActions.loadShiftDetails, (state, { shiftId }): CashShiftState => ({
      ...state,
      shiftDetails: state.shiftDetails?.id === shiftId ? state.shiftDetails : null,
      isLoadingShiftDetails: true,
      error: null,
    })),
    on(CashShiftActions.loadShiftDetailsSuccess, (state, { shiftDetails }): CashShiftState => ({
      ...state,
      shiftDetails,
      isLoadingShiftDetails: false,
    })),
    on(CashShiftActions.loadShiftDetailsFailure, (state, { error }): CashShiftState => ({
      ...state,
      isLoadingShiftDetails: false,
      error,
    })),
    on(CashShiftActions.clearShiftDetails, (state): CashShiftState => ({
      ...state,
      shiftDetails: null,
    })),
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
    // `currentLoaded` means "the lookup finished", not "a shift exists". The
    // shell renders nothing until it flips, so leaving it false on failure made
    // a single failed request (API down, 500, dropped connection) hide the
    // entire authenticated app with no way out. The error still surfaces
    // through the HTTP error interceptor.
    on(CashShiftActions.loadCurrentFailure, (state, { error }): CashShiftState => ({
      ...state,
      currentLoaded: true,
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
    on(CashShiftActions.loadMovements, (state, { shiftId }): CashShiftState => ({
      ...state,
      movements: state.movementsShiftId === shiftId ? state.movements : [],
      isLoadingMovements: true,
      error: null,
    })),
    on(CashShiftActions.loadMovementsSuccess, (state, { shiftId, movements }): CashShiftState => ({
      ...state,
      movements,
      movementsShiftId: shiftId,
      isLoadingMovements: false,
    })),
    on(CashShiftActions.loadMovementsFailure, (state, { error }): CashShiftState => ({
      ...state,
      isLoadingMovements: false,
      error,
    })),
    on(
      CashShiftActions.recordSupplement,
      CashShiftActions.recordWithdrawal,
      (state): CashShiftState => ({
        ...state,
        isRecordingMovement: true,
        lastRecordedMovement: null,
        error: null,
      }),
    ),
    on(CashShiftActions.recordCashMovementSuccess, (state, { movement }): CashShiftState => ({
      ...state,
      current: state.current ? applyMovement(state.current, movement) : null,
      // API returns movements oldest first; append only when the loaded list is this shift's.
      movements:
        state.movementsShiftId === movement.cashShiftId
          ? [...state.movements, movement]
          : state.movements,
      isRecordingMovement: false,
      lastRecordedMovement: movement,
    })),
    on(CashShiftActions.recordCashMovementFailure, (state, { error }): CashShiftState => ({
      ...state,
      isRecordingMovement: false,
      error,
    })),
    on(CashShiftActions.clearLastRecordedMovement, (state): CashShiftState => ({
      ...state,
      lastRecordedMovement: null,
    })),
  ),
});

// Mirrors the API's expected cash formula: startingCash + cashSales + supplements - withdrawals.
function applyMovement(cashShift: CashShift, movement: CashMovementResponse): CashShift {
  if (movement.type === 'supplement') {
    return {
      ...cashShift,
      totalSupplements: cashShift.totalSupplements + movement.amount,
      expectedCash: cashShift.expectedCash + movement.amount,
    };
  }

  return {
    ...cashShift,
    totalWithdrawals: cashShift.totalWithdrawals + movement.amount,
    expectedCash: cashShift.expectedCash - movement.amount,
  };
}
