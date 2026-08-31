import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';

export const SaleActions = createActionGroup({
  source: 'Sale',
  events: {
    'Sync Sales': props<{ request: SyncSalesRequest }>(),
    'Sync Sales Success': props<{ response: SyncSalesResponse }>(),
    'Sync Sales Failure': props<{ error: string }>(),
    'Clear Sync Result': emptyProps(),
    'Load Pos Products': props<{ categoryId?: string }>(),
    'Load Pos Products Success': props<{ products: PosProduct[] }>(),
    'Load Pos Products Failure': props<{ error: string }>(),
  },
});
