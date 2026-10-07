export type CashMovementDirection = 'in' | 'out';

export interface CashMovementRequest {
  direction: CashMovementDirection;
  amount: number;
  reason: string;
}
