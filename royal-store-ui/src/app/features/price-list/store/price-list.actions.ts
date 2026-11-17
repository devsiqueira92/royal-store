import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CreatePriceListRequest,
  PriceList,
  SetPriceListRuleRequest,
} from '../interfaces/price-list.interface';

export const PriceListActions = createActionGroup({
  source: 'PriceList',
  events: {
    'Load Price Lists': emptyProps(),
    'Load Price Lists Success': props<{ priceLists: PriceList[] }>(),
    'Load Price Lists Failure': props<{ error: string }>(),

    'Create Price List': props<{ request: CreatePriceListRequest }>(),
    'Create Price List Success': props<{ priceList: PriceList }>(),
    'Create Price List Failure': props<{ error: string }>(),

    'Set Default Price List': props<{ id: string }>(),
    'Set Default Price List Success': props<{ priceList: PriceList }>(),
    'Set Default Price List Failure': props<{ error: string }>(),

    'Activate Price List': props<{ id: string }>(),
    'Activate Price List Success': props<{ id: string }>(),
    'Activate Price List Failure': props<{ error: string }>(),

    'Deactivate Price List': props<{ id: string }>(),
    'Deactivate Price List Success': props<{ id: string }>(),
    'Deactivate Price List Failure': props<{ error: string }>(),

    'Delete Price List': props<{ id: string }>(),
    'Delete Price List Success': props<{ id: string }>(),
    'Delete Price List Failure': props<{ error: string }>(),

    'Set Price List Rule': props<{
      id: string;
      request: SetPriceListRuleRequest;
    }>(),
    'Set Price List Rule Success': props<{ priceList: PriceList }>(),
    'Set Price List Rule Failure': props<{ error: string }>(),

    'Select Price List': props<{ id: string }>(),
    'Clear Selected Price List': emptyProps(),
  },
});
