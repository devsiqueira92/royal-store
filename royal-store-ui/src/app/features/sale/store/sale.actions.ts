import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';
import { RegisterSaleRequest } from '../interfaces/register-sale-request.interface';
import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { GetSalesListParams } from '../interfaces/get-sales-list-params.interface';
import { SaleListItem } from '../interfaces/sale-list-item.interface';
import { SaleDetails } from '../interfaces/sale-details.interface';
import { SaleReceipt } from '../interfaces/sale-receipt.interface';

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
    'Load Sales': props<{ params?: GetSalesListParams }>(),
    'Load Sales Success': props<{ sales: SaleListItem[] }>(),
    'Load Sales Failure': props<{ error: string }>(),
    'Load Sale Details': props<{ saleId: string }>(),
    'Load Sale Details Success': props<{ saleDetails: SaleDetails }>(),
    'Load Sale Details Failure': props<{ error: string }>(),
    'Clear Sale Details': emptyProps(),
    'Load Sale Receipt': props<{ saleId: string }>(),
    'Load Sale Receipt Success': props<{ saleReceipt: SaleReceipt }>(),
    'Load Sale Receipt Failure': props<{ error: string }>(),
    'Clear Sale Receipt': emptyProps(),
    'Register Sale': props<{ request: RegisterSaleRequest }>(),
    'Register Sale Success': props<{ response: RegisterSaleResponse }>(),
    'Register Sale Failure': props<{ error: string }>(),
    'Clear Register Sale Result': emptyProps(),
  },
});
