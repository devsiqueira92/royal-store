export interface CashShiftDetails {
  id: string;
  userId: string | null;
  userName: string | null;
  startingCash: number;
  openedAt: string;
  closedAt: string | null;
  totalSales: number;
  totalSupplements: number;
  totalWithdrawals: number;
  expectedCash: number;
  cashSales: number;
  creditCardSales: number;
  debitCardSales: number;
  cardSales: number;
  salesCount: number;
  expectedEndingCash: number | null;
  actualEndingCash: number | null;
}
