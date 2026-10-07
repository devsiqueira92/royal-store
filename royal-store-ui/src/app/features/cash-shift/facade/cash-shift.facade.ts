import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { OpenCashShiftRequest } from '../interfaces/open-cash-shift-request.interface';
import { CloseCashShiftRequest } from '../interfaces/close-cash-shift-request.interface';
import { CashShiftActions } from '../store/cash-shift.actions';
import {
  selectCurrent,
  selectCurrentLoaded,
  selectIsOpening,
  selectIsClosing,
  selectError,
} from '../store/cash-shift.selectors';

@Injectable({
  providedIn: 'root',
})
export class CashShiftFacade {
  private store = inject(Store);

  current$ = this.store.select(selectCurrent);
  currentLoaded$ = this.store.select(selectCurrentLoaded);
  isOpening$ = this.store.select(selectIsOpening);
  isClosing$ = this.store.select(selectIsClosing);
  error$ = this.store.select(selectError);

  loadCurrent(): void {
    this.store.dispatch(CashShiftActions.loadCurrent());
  }

  openCashShift(request: OpenCashShiftRequest): void {
    this.store.dispatch(CashShiftActions.openCashShift({ request }));
  }

  closeCashShift(request: CloseCashShiftRequest): void {
    this.store.dispatch(CashShiftActions.closeCashShift({ request }));
  }
}
