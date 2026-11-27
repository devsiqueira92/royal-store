import { createFeature, createReducer, on } from '@ngrx/store';
import {
  CostComposition,
  CostCompositionSummary,
  toCostCompositionSummary,
} from '../interfaces/cost-composition.interface';
import { CostCompositionActions as A } from './cost-composition.actions';

export interface CostCompositionState {
  costCompositions: CostCompositionSummary[];
  selectedCostComposition: CostComposition | null;
  isLoading: boolean;
  isLoadingSelected: boolean;
  error: string | null;
}

const initialState: CostCompositionState = {
  costCompositions: [],
  selectedCostComposition: null,
  isLoading: false,
  isLoadingSelected: false,
  error: null,
};

const pending = (state: CostCompositionState): CostCompositionState => ({
  ...state,
  isLoading: true,
  error: null,
});

const failed = (
  state: CostCompositionState,
  { error }: { error: string },
): CostCompositionState => ({ ...state, isLoading: false, error });

export const costCompositionFeature = createFeature({
  name: 'costComposition',
  reducer: createReducer(
    initialState,

    on(A.loadCostCompositions, pending),
    on(
      A.loadCostCompositionsSuccess,
      (state, { costCompositions }): CostCompositionState => ({
        ...state,
        costCompositions,
        isLoading: false,
      }),
    ),
    on(A.loadCostCompositionsFailure, failed),

    on(A.createCostComposition, pending),
    on(
      A.createCostCompositionSuccess,
      (state, { costComposition }): CostCompositionState => ({
        ...state,
        costCompositions: [
          ...state.costCompositions,
          toCostCompositionSummary(costComposition),
        ],
        isLoading: false,
      }),
    ),
    on(A.createCostCompositionFailure, failed),

    on(A.updateCostComposition, pending),
    on(
      A.updateCostCompositionSuccess,
      (state, { costComposition }): CostCompositionState => ({
        ...state,
        costCompositions: state.costCompositions.map(c =>
          c.id === costComposition.id
            ? toCostCompositionSummary(costComposition, c.productsCount)
            : c,
        ),
        selectedCostComposition:
          state.selectedCostComposition?.id === costComposition.id
            ? costComposition
            : state.selectedCostComposition,
        isLoading: false,
      }),
    ),
    on(A.updateCostCompositionFailure, failed),

    on(A.deleteCostComposition, pending),
    on(
      A.deleteCostCompositionSuccess,
      (state, { id }): CostCompositionState => ({
        ...state,
        costCompositions: state.costCompositions.filter(c => c.id !== id),
        selectedCostComposition:
          state.selectedCostComposition?.id === id
            ? null
            : state.selectedCostComposition,
        isLoading: false,
      }),
    ),
    on(A.deleteCostCompositionFailure, failed),

    on(
      A.loadCostComposition,
      (state): CostCompositionState => ({
        ...state,
        selectedCostComposition: null,
        isLoadingSelected: true,
        error: null,
      }),
    ),
    on(
      A.loadCostCompositionSuccess,
      (state, { costComposition }): CostCompositionState => ({
        ...state,
        selectedCostComposition: costComposition,
        isLoadingSelected: false,
      }),
    ),
    on(
      A.loadCostCompositionFailure,
      (state, { error }): CostCompositionState => ({
        ...state,
        isLoadingSelected: false,
        error,
      }),
    ),
    on(
      A.clearSelectedCostComposition,
      (state): CostCompositionState => ({
        ...state,
        selectedCostComposition: null,
        isLoadingSelected: false,
      }),
    ),
  ),
});
