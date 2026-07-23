import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Seller } from '../interfaces/seller.interface';
import { CreateSellerRequest } from '../interfaces/create-seller-request.interface';
import { UpdateSellerRequest } from '../interfaces/update-seller-request.interface';
import { GetSellersParams } from '../interfaces/get-sellers-params.interface';

export const SellerActions = createActionGroup({
  source: 'Seller',
  events: {
    'Load Sellers': props<{ params?: GetSellersParams }>(),
    'Load Sellers Success': props<{ sellers: Seller[] }>(),
    'Load Sellers Failure': props<{ error: string }>(),
    'Load Seller': props<{ id: string }>(),
    'Load Seller Success': props<{ seller: Seller }>(),
    'Load Seller Failure': props<{ error: string }>(),
    'Create Seller': props<{ request: CreateSellerRequest }>(),
    'Create Seller Success': props<{ seller: Seller }>(),
    'Create Seller Failure': props<{ error: string }>(),
    'Update Seller': props<{ id: string; request: UpdateSellerRequest }>(),
    'Update Seller Success': props<{ seller: Seller }>(),
    'Update Seller Failure': props<{ error: string }>(),
    'Deactivate Seller': props<{ id: string }>(),
    'Deactivate Seller Success': props<{ id: string }>(),
    'Deactivate Seller Failure': props<{ error: string }>(),
    'Clear Selected Seller': emptyProps(),
  },
});
