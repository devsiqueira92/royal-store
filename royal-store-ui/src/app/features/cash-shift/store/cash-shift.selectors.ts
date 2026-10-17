import { cashShiftFeature } from './cash-shift.reducer';

export const {
  selectShifts,
  selectIsLoadingShifts,
  selectShiftDetails,
  selectIsLoadingShiftDetails,
  selectCurrent,
  selectCurrentLoaded,
  selectIsOpening,
  selectIsClosing,
  selectMovements,
  selectMovementsShiftId,
  selectIsLoadingMovements,
  selectIsRecordingMovement,
  selectLastRecordedMovement,
  selectError,
} = cashShiftFeature;
