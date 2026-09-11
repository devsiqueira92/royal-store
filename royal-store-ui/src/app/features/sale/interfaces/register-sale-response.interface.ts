import { SyncStatus } from './sync-status.enum';

export interface RegisterSaleResponse {
  saleId: string;
  status: SyncStatus;
  cashbackApplied: number;
  cashbackEarned: number;
  paymentDiscountApplied: number;
  loyaltyDiscountApplied: number;
  taxAmount: number;
  totalAmount: number;
}
