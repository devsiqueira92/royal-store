export type CashMovementType = 'sale' | 'supplement' | 'withdrawal';

export interface CashMovement {
  id: string;
  type: CashMovementType;
  amount: number;
  description: string;
  occurredAt: string;
}
