import { CashShiftStatus } from '@app/shared/enum/cash-shift-status.enum';

export interface CashShift {
  id: string;
  status: CashShiftStatus;
  openedAt: string;
  closedAt: string | null;
  startingCash: number;
  totalSales: number;
  totalSupplements: number;
  totalWithdrawals: number;
  expectedCash: number;
  cashSales: number;
  creditCardSales: number;
  debitCardSales: number;
  cardSales: number;
  actualEndingCash: number | null;
  over: number | null;
  salesCount: number;
}
