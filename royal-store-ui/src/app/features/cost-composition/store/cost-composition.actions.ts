import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CostComposition,
  CreateCostCompositionRequest,
  UpdateCostCompositionRequest,
} from '../interfaces/cost-composition.interface';

export const CostCompositionActions = createActionGroup({
  source: 'CostComposition',
  events: {
    'Load Cost Compositions': emptyProps(),
    'Load Cost Compositions Success': props<{
      costCompositions: CostComposition[];
    }>(),
    'Load Cost Compositions Failure': props<{ error: string }>(),

    'Create Cost Composition': props<{
      request: CreateCostCompositionRequest;
    }>(),
    'Create Cost Composition Success': props<{
      costComposition: CostComposition;
    }>(),
    'Create Cost Composition Failure': props<{ error: string }>(),

    'Update Cost Composition': props<{
      id: string;
      request: UpdateCostCompositionRequest;
    }>(),
    'Update Cost Composition Success': props<{
      costComposition: CostComposition;
    }>(),
    'Update Cost Composition Failure': props<{ error: string }>(),

    'Delete Cost Composition': props<{ id: string }>(),
    'Delete Cost Composition Success': props<{ id: string }>(),
    'Delete Cost Composition Failure': props<{ error: string }>(),

    'Select Cost Composition': props<{ id: string }>(),
    'Clear Selected Cost Composition': emptyProps(),
  },
});
