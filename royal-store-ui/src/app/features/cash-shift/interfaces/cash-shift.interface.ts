import { CashShiftStatus } from '@app/shared/enum/cash-shift-status.enum';
import { CashShiftOverview } from './cash-shift-overview.interface';

export interface CashShift extends CashShiftOverview {
  status: CashShiftStatus;
  over: number | null;
}
