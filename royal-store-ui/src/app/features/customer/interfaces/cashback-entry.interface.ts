/** Lowercased `CashbackEntryState` as serialized by the API. */
export type CashbackEntryState = 'available' | 'used';

export interface CashbackEntry {
  id: string;
  grantedAmount: number;
  remainingAmount: number;
  saleId: string;
  earnedAt: string;
  expiresAt: string;
  state: CashbackEntryState;
}
