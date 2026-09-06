import { createFeature, createReducer, on } from '@ngrx/store';
import { OriginOption } from '../interfaces/origin-option.interface';
import { OriginOptionActions } from './origin-option.actions';

export interface OriginOptionState {
  originOptions: OriginOption[];
  isLoading: boolean;
  error: string | null;
}

export const initialOriginOptionState: OriginOptionState = {
  originOptions: [],
  isLoading: false,
  error: null,
};

export const originOptionFeature = createFeature({
  name: 'originOption',
  reducer: createReducer(
    initialOriginOptionState,
    on(OriginOptionActions.loadOriginOptions, (state): OriginOptionState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      OriginOptionActions.loadOriginOptionsSuccess,
      (state, { originOptions }): OriginOptionState => ({
        ...state,
        originOptions,
        isLoading: false,
      }),
    ),
    on(
      OriginOptionActions.loadOriginOptionsFailure,
      (state, { error }): OriginOptionState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
  ),
});
