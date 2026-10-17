// Fields shared by the current shift and a shift loaded by id.
export interface CashShiftOverview {
  id: string;
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
  salesCount: number;
  actualEndingCash: number | null;
}
