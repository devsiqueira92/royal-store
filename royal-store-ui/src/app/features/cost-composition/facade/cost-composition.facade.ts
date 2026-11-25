import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import {
  CreateCostCompositionRequest,
  UpdateCostCompositionRequest,
} from '../interfaces/cost-composition.interface';
import { CostCompositionActions } from '../store/cost-composition.actions';
import {
  selectCostCompositions,
  selectError,
  selectIsLoading,
  selectIsLoadingSelected,
  selectSelectedCostComposition,
} from '../store/cost-composition.selectors';

@Injectable({ providedIn: 'root' })
export class CostCompositionFacade {
  private readonly store = inject(Store);

  costCompositions$ = this.store.select(selectCostCompositions);
  selectedCostComposition$ = this.store.select(selectSelectedCostComposition);
  isLoading$ = this.store.select(selectIsLoading);
  isLoadingSelected$ = this.store.select(selectIsLoadingSelected);
  error$ = this.store.select(selectError);

  loadCostCompositions(): void {
    this.store.dispatch(CostCompositionActions.loadCostCompositions());
  }

  create(request: CreateCostCompositionRequest): void {
    this.store.dispatch(
      CostCompositionActions.createCostComposition({ request }),
    );
  }

  update(id: string, request: UpdateCostCompositionRequest): void {
    this.store.dispatch(
      CostCompositionActions.updateCostComposition({ id, request }),
    );
  }

  delete(id: string): void {
    this.store.dispatch(CostCompositionActions.deleteCostComposition({ id }));
  }

  /** Loads the full list (with lines) into the selected slot. */
  loadCostComposition(id: string): void {
    this.store.dispatch(CostCompositionActions.loadCostComposition({ id }));
  }

  clearSelectedCostComposition(): void {
    this.store.dispatch(CostCompositionActions.clearSelectedCostComposition());
  }
}
