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
  selectIsLoadingMovements,
  selectIsRecordingMovement,
  selectLastRecordedMovement,
  selectError,
} = cashShiftFeature;
