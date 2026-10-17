import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { OpenCashShiftRequest } from '../interfaces/open-cash-shift-request.interface';
import { CloseCashShiftRequest } from '../interfaces/close-cash-shift-request.interface';
import { RecordCashMovementRequest } from '../interfaces/record-cash-movement-request.interface';
import { CashShiftActions } from '../store/cash-shift.actions';
import {
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
} from '../store/cash-shift.selectors';

@Injectable({
  providedIn: 'root',
})
export class CashShiftFacade {
  private store = inject(Store);

  shifts$ = this.store.select(selectShifts);
  isLoadingShifts$ = this.store.select(selectIsLoadingShifts);
  shiftDetails$ = this.store.select(selectShiftDetails);
  isLoadingShiftDetails$ = this.store.select(selectIsLoadingShiftDetails);
  current$ = this.store.select(selectCurrent);
  currentLoaded$ = this.store.select(selectCurrentLoaded);
  isOpening$ = this.store.select(selectIsOpening);
  isClosing$ = this.store.select(selectIsClosing);
  movements$ = this.store.select(selectMovements);
  movementsShiftId$ = this.store.select(selectMovementsShiftId);
  isLoadingMovements$ = this.store.select(selectIsLoadingMovements);
  isRecordingMovement$ = this.store.select(selectIsRecordingMovement);
  lastRecordedMovement$ = this.store.select(selectLastRecordedMovement);
  error$ = this.store.select(selectError);

  loadShifts(): void {
    this.store.dispatch(CashShiftActions.loadShifts());
  }

  loadShiftDetails(shiftId: string): void {
    this.store.dispatch(CashShiftActions.loadShiftDetails({ shiftId }));
  }

  clearShiftDetails(): void {
    this.store.dispatch(CashShiftActions.clearShiftDetails());
  }

  loadCurrent(): void {
    this.store.dispatch(CashShiftActions.loadCurrent());
  }

  refreshCurrent(): void {
    this.store.dispatch(CashShiftActions.refreshCurrent());
  }

  openCashShift(request: OpenCashShiftRequest): void {
    this.store.dispatch(CashShiftActions.openCashShift({ request }));
  }

  closeCashShift(request: CloseCashShiftRequest): void {
    this.store.dispatch(CashShiftActions.closeCashShift({ request }));
  }

  loadMovements(shiftId: string): void {
    this.store.dispatch(CashShiftActions.loadMovements({ shiftId }));
  }

  recordSupplement(request: RecordCashMovementRequest): void {
    this.store.dispatch(CashShiftActions.recordSupplement({ request }));
  }

  recordWithdrawal(request: RecordCashMovementRequest): void {
    this.store.dispatch(CashShiftActions.recordWithdrawal({ request }));
  }

  clearLastRecordedMovement(): void {
    this.store.dispatch(CashShiftActions.clearLastRecordedMovement());
  }
}
