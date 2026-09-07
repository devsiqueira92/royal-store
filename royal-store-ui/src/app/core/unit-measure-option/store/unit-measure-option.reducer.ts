import { createFeature, createReducer, on } from '@ngrx/store';
import { UnitMeasureOption } from '../interfaces/unit-measure-option.interface';
import { UnitMeasureOptionActions } from './unit-measure-option.actions';

export interface UnitMeasureOptionState {
  unitMeasureOptions: UnitMeasureOption[];
  isLoading: boolean;
  error: string | null;
}

export const initialUnitMeasureOptionState: UnitMeasureOptionState = {
  unitMeasureOptions: [],
  isLoading: false,
  error: null,
};

export const unitMeasureOptionFeature = createFeature({
  name: 'unitMeasureOption',
  reducer: createReducer(
    initialUnitMeasureOptionState,
    on(UnitMeasureOptionActions.loadUnitMeasureOptions, (state): UnitMeasureOptionState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      UnitMeasureOptionActions.loadUnitMeasureOptionsSuccess,
      (state, { unitMeasureOptions }): UnitMeasureOptionState => ({
        ...state,
        unitMeasureOptions,
        isLoading: false,
      }),
    ),
    on(
      UnitMeasureOptionActions.loadUnitMeasureOptionsFailure,
      (state, { error }): UnitMeasureOptionState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
  ),
});
