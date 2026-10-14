export interface CashShiftListItem {
  id: string;
  userId: string | null;
  userName: string | null;
  startingCash: number;
  openedAt: string;
  closedAt: string | null;
  totalSales: number;
  totalSupplements: number;
  totalWithdrawals: number;
  cashSales: number;
  creditCardSales: number;
  cardSales: number;
  salesCount: number;
  numberOfMovements: number;
}
