import { cashShiftFeature } from './cash-shift.reducer';

export const {
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
