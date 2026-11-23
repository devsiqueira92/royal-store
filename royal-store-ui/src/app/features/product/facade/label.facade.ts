import { inject, Injectable } from '@angular/core';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { CreateLabelRequest, UpdateLabelRequest } from '../interfaces/label-format.interface';
import { LabelActions } from '../store/label.actions';
import {
  selectActiveLabels,
  selectLabelConfigurations,
  selectLabelConfigurationsIsLoading,
  selectLabelPrintOptions,
  selectLabelPrintOptionsIsLoading,
  selectLabelSelectedConfiguration,
  selectLabelError,
  selectLabelIsLoading,
  selectLabels,
  selectSelectedLabel,
} from '../store/label.selectors';

@Injectable({ providedIn: 'root' })
export class LabelFacade {
  private readonly store = inject(Store);
  private readonly actions$ = inject(Actions);

  labels$ = this.store.select(selectLabels);
  activeLabels$ = this.store.select(selectActiveLabels);
  selectedLabel$ = this.store.select(selectSelectedLabel);
  configurations = this.store.selectSignal(selectLabelConfigurations);
  isLoadingConfigurations = this.store.selectSignal(selectLabelConfigurationsIsLoading);
  selectedConfiguration = this.store.selectSignal(selectLabelSelectedConfiguration);
  printOptions = this.store.selectSignal(selectLabelPrintOptions);
  isLoadingPrintOptions = this.store.selectSignal(selectLabelPrintOptionsIsLoading);
  isLoading$ = this.store.select(selectLabelIsLoading);
  error$ = this.store.select(selectLabelError);

  /** Emits the saved label after a successful create or update. */
  saved$ = this.actions$.pipe(
    ofType(LabelActions.createLabelSuccess, LabelActions.updateLabelSuccess),
    map(({ label }) => label),
  );

  /** `isActive` omitted → every label. */
  loadLabels(isActive?: boolean): void {
    this.store.dispatch(LabelActions.loadLabels({ isActive }));
  }

  /** `GET api/labels/configurations`: system configurations + store's active labels. */
  loadConfigurations(): void {
    this.store.dispatch(LabelActions.loadConfigurations());
  }

  /** `GET api/labels/print-options`: store's active labels (default first) + system configurations. */
  loadPrintOptions(): void {
    this.store.dispatch(LabelActions.loadPrintOptions());
  }

  /** Full label of a configuration picked as starting point. */
  loadConfiguration(id: string): void {
    this.store.dispatch(LabelActions.loadConfiguration({ id }));
  }

  clearSelectedConfiguration(): void {
    this.store.dispatch(LabelActions.clearSelectedConfiguration());
  }

  loadLabel(id: string): void {
    this.store.dispatch(LabelActions.loadLabel({ id }));
  }

  create(request: CreateLabelRequest): void {
    this.store.dispatch(LabelActions.createLabel({ request }));
  }

  update(id: string, request: UpdateLabelRequest): void {
    this.store.dispatch(LabelActions.updateLabel({ id, request }));
  }

  activate(id: string): void {
    this.store.dispatch(LabelActions.setLabelActive({ id, isActive: true }));
  }

  deactivate(id: string): void {
    this.store.dispatch(LabelActions.setLabelActive({ id, isActive: false }));
  }

  delete(id: string): void {
    this.store.dispatch(LabelActions.deleteLabel({ id }));
  }

  clearSelectedLabel(): void {
    this.store.dispatch(LabelActions.clearSelectedLabel());
  }
}
