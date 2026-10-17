import { CashShiftOverview } from './cash-shift-overview.interface';

export interface CashShiftDetails extends CashShiftOverview {
  userId: string | null;
  userName: string | null;
  expectedEndingCash: number | null;
}
