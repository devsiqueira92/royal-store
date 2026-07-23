import { SyncStatus } from './sync-status.enum';

export interface SaleSyncItemResult {
  saleId: string;
  status: SyncStatus;
  error?: string;
}
