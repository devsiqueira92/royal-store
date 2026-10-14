import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CashShift } from '../interfaces/cash-shift.interface';
import { OpenCashShiftRequest } from '../interfaces/open-cash-shift-request.interface';
import { CloseCashShiftRequest } from '../interfaces/close-cash-shift-request.interface';
import { RecordCashMovementRequest } from '../interfaces/record-cash-movement-request.interface';
import { CashMovementResponse } from '../interfaces/cash-movement-response.interface';
import { CashShiftListItem } from '../interfaces/cash-shift-list-item.interface';
import { CashShiftDetails } from '../interfaces/cash-shift-details.interface';

export const CashShiftActions = createActionGroup({
  source: 'CashShift',
  events: {
    'Load Shifts': emptyProps(),
    'Load Shifts Success': props<{ shifts: CashShiftListItem[] }>(),
    'Load Shifts Failure': props<{ error: string }>(),
    'Load Shift Details': props<{ shiftId: string }>(),
    'Load Shift Details Success': props<{ shiftDetails: CashShiftDetails }>(),
    'Load Shift Details Failure': props<{ error: string }>(),
    'Clear Shift Details': emptyProps(),
    'Load Current': emptyProps(),
    'Refresh Current': emptyProps(),
    'Load Current Success': props<{ cashShift: CashShift | null }>(),
    'Load Current Failure': props<{ error: string }>(),
    'Open Cash Shift': props<{ request: OpenCashShiftRequest }>(),
    'Open Cash Shift Success': props<{ cashShift: CashShift }>(),
    'Open Cash Shift Failure': props<{ error: string }>(),
    'Close Cash Shift': props<{ request: CloseCashShiftRequest }>(),
    'Close Cash Shift Success': props<{ cashShift: CashShift }>(),
    'Close Cash Shift Failure': props<{ error: string }>(),
    'Load Movements': props<{ shiftId: string }>(),
    'Load Movements Success': props<{ shiftId: string; movements: CashMovementResponse[] }>(),
    'Load Movements Failure': props<{ error: string }>(),
    'Record Supplement': props<{ request: RecordCashMovementRequest }>(),
    'Record Withdrawal': props<{ request: RecordCashMovementRequest }>(),
    'Record Cash Movement Success': props<{ movement: CashMovementResponse }>(),
    'Record Cash Movement Failure': props<{ error: string }>(),
    'Clear Last Recorded Movement': emptyProps(),
  },
});
