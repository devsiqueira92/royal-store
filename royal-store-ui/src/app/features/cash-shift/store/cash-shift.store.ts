import {
  loadCashShifts,
  loadCashShiftDetails,
  loadCurrentCashShift,
  openCashShift,
  closeCashShift,
  loadCashShiftMovements,
  recordSupplement,
  recordWithdrawal,
} from './cash-shift.effects';

export const cashShiftStore = {
  loadCashShifts,
  loadCashShiftDetails,
  loadCurrentCashShift,
  openCashShift,
  closeCashShift,
  loadCashShiftMovements,
  recordSupplement,
  recordWithdrawal,
};
