export type RecordedCashMovementType = 'supplement' | 'withdrawal';

export interface CashMovementResponse {
  id: string;
  cashShiftId: string;
  type: RecordedCashMovementType;
  amount: number;
  description: string | null;
  userId: string | null;
  movedAt: string;
}
