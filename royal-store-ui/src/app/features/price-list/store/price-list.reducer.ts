import { createFeature, createReducer, on } from '@ngrx/store';
import { PriceList } from '../interfaces/price-list.interface';
import { PriceListActions } from './price-list.actions';

export interface PriceListState {
  priceLists: PriceList[];
  selectedPriceList: PriceList | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: PriceListState = {
  priceLists: [],
  selectedPriceList: null,
  isLoading: false,
  error: null,
};

export const priceListFeature = createFeature({
  name: 'priceList',
  reducer: createReducer(
    initialState,

    on(PriceListActions.loadPriceLists, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.loadPriceListsSuccess,
      (state, { priceLists }): PriceListState => ({
        ...state,
        priceLists,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.loadPriceListsFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.createPriceList, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.createPriceListSuccess,
      (state, { priceList }): PriceListState => ({
        ...state,
        priceLists: [...state.priceLists, priceList],
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.createPriceListFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    // on(PriceListActions.renamePriceList, (state): PriceListState => ({
    //   ...state, isLoading: true, error: null,
    // })),

    on(PriceListActions.setDefaultPriceList, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.setDefaultPriceListSuccess,
      (state, { priceList }): PriceListState => ({
        ...state,
        priceLists: state.priceLists.map(p =>
          p.id === priceList.id ? priceList : { ...p, isDefault: false },
        ),
        selectedPriceList:
          state.selectedPriceList?.id === priceList.id
            ? priceList
            : state.selectedPriceList,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.setDefaultPriceListFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.activatePriceList, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.activatePriceListSuccess,
      (state, { id }): PriceListState => ({
        ...state,
        priceLists: state.priceLists.map(p =>
          p.id === id ? { ...p, isActive: true } : p,
        ),
        selectedPriceList:
          state.selectedPriceList?.id === id
            ? { ...state.selectedPriceList, isActive: true }
            : state.selectedPriceList,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.activatePriceListFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.deactivatePriceList, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.deactivatePriceListSuccess,
      (state, { id }): PriceListState => ({
        ...state,
        priceLists: state.priceLists.map(p =>
          p.id === id ? { ...p, isActive: false } : p,
        ),
        selectedPriceList:
          state.selectedPriceList?.id === id
            ? { ...state.selectedPriceList, isActive: false }
            : state.selectedPriceList,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.deactivatePriceListFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.deletePriceList, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.deletePriceListSuccess,
      (state, { id }): PriceListState => ({
        ...state,
        priceLists: state.priceLists.filter(p => p.id !== id),
        selectedPriceList:
          state.selectedPriceList?.id === id ? null : state.selectedPriceList,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.deletePriceListFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.setPriceListRule, (state): PriceListState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PriceListActions.setPriceListRuleSuccess,
      (state, { priceList }): PriceListState => ({
        ...state,
        priceLists: state.priceLists.map(p =>
          p.id === priceList.id ? priceList : p,
        ),
        selectedPriceList:
          state.selectedPriceList?.id === priceList.id
            ? priceList
            : state.selectedPriceList,
        isLoading: false,
      }),
    ),
    on(
      PriceListActions.setPriceListRuleFailure,
      (state, { error }): PriceListState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(PriceListActions.selectPriceList, (state, { id }): PriceListState => ({
      ...state,
      selectedPriceList: state.priceLists.find(p => p.id === id) ?? null,
    })),
    on(PriceListActions.clearSelectedPriceList, (state): PriceListState => ({
      ...state,
      selectedPriceList: null,
    })),
  ),
});
