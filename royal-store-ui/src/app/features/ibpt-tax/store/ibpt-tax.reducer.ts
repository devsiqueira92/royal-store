import { createFeature, createReducer, on } from '@ngrx/store';
import { IbptTax } from '../interfaces/ibpt-tax.interface';
import { IbptTaxActions } from './ibpt-tax.actions';

export interface IbptTaxState {
  results: IbptTax[];
  isLoading: boolean;
  error: string | null;
}

export const initialIbptTaxState: IbptTaxState = {
  results: [],
  isLoading: false,
  error: null,
};

export const ibptTaxFeature = createFeature({
  name: 'ibptTax',
  reducer: createReducer(
    initialIbptTaxState,
    on(IbptTaxActions.searchIbptTaxes, (state): IbptTaxState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      IbptTaxActions.searchIbptTaxesSuccess,
      (state, { results }): IbptTaxState => ({
        ...state,
        results,
        isLoading: false,
      }),
    ),
    on(
      IbptTaxActions.searchIbptTaxesFailure,
      (state, { error }): IbptTaxState => ({
        ...state,
        results: [],
        isLoading: false,
        error,
      }),
    ),
    on(IbptTaxActions.clearIbptTaxes, (state): IbptTaxState => ({
      ...state,
      results: [],
      isLoading: false,
      error: null,
    })),
  ),
});
