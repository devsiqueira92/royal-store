import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CashShift } from '../interfaces/cash-shift.interface';
import { OpenCashShiftRequest } from '../interfaces/open-cash-shift-request.interface';
import { CloseCashShiftRequest } from '../interfaces/close-cash-shift-request.interface';

export const CashShiftActions = createActionGroup({
  source: 'CashShift',
  events: {
    'Load Current': emptyProps(),
    'Load Current Success': props<{ cashShift: CashShift | null }>(),
    'Load Current Failure': props<{ error: string }>(),
    'Open Cash Shift': props<{ request: OpenCashShiftRequest }>(),
    'Open Cash Shift Success': props<{ cashShift: CashShift }>(),
    'Open Cash Shift Failure': props<{ error: string }>(),
    'Close Cash Shift': props<{ request: CloseCashShiftRequest }>(),
    'Close Cash Shift Success': props<{ cashShift: CashShift }>(),
    'Close Cash Shift Failure': props<{ error: string }>(),
  },
});
