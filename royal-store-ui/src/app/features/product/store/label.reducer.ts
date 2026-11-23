import { createFeature, createReducer, on } from '@ngrx/store';
import { Label, LabelSummary } from '../interfaces/label-format.interface';
import { Options } from '@app/shared/interceptors/options.interface';
import { LabelActions as A } from './label.actions';

export interface LabelState {
  /** Listing rows (`GET api/labels`). */
  labels: LabelSummary[];
  /** Full label (`GET api/labels/{id}`). */
  selectedLabel: Label | null;
  /** "Start from" options (`GET api/labels/configurations`). */
  configurations: Options[];
  isLoadingConfigurations: boolean;
  /** Labels a print can use (`GET api/labels/print-options`). */
  printOptions: Options[];
  isLoadingPrintOptions: boolean;
  /** Configuration picked as starting point (`GET api/labels/{id}`); kept apart from `selectedLabel`. */
  selectedConfiguration: Label | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: LabelState = {
  labels: [],
  selectedLabel: null,
  configurations: [],
  isLoadingConfigurations: false,
  selectedConfiguration: null,
  printOptions: [],
  isLoadingPrintOptions: false,
  isLoading: false,
  error: null,
};

const pending = (state: LabelState): LabelState => ({
  ...state,
  isLoading: true,
  error: null,
});

const failed = (state: LabelState, { error }: { error: string }): LabelState => ({
  ...state,
  isLoading: false,
  error,
});

const toSummary = ({ format, template }: Label): LabelSummary => ({
  id: template.id,
  name: template.name,
  paperType: format.paperType,
  labelWidth: format.labelWidth,
  labelHeight: format.labelHeight,
  columns: format.columns,
  isActive: template.isActive,
  updatedAt: template.updatedAt,
});

/** Inserts or replaces the listing row of `label` (matched by id) and refreshes the selection. */
const upsert = (state: LabelState, label: Label): LabelState => {
  const summary = toSummary(label);
  const exists = state.labels.some((l) => l.id === summary.id);
  return {
    ...state,
    labels: exists
      ? state.labels.map((l) => (l.id === summary.id ? summary : l))
      : [...state.labels, summary],
    selectedLabel: state.selectedLabel?.template.id === summary.id ? label : state.selectedLabel,
    isLoading: false,
  };
};

const withActive = (label: Label, isActive: boolean): Label => ({
  ...label,
  template: { ...label.template, isActive },
});

export const labelFeature = createFeature({
  name: 'label',
  reducer: createReducer(
    initialState,

    on(A.loadLabels, pending),
    on(A.loadLabelsSuccess, (state, { labels }): LabelState => ({
      ...state,
      labels,
      isLoading: false,
    })),
    on(A.loadLabelsFailure, failed),

    on(A.loadConfigurations, (state): LabelState => ({
      ...state,
      isLoadingConfigurations: true,
      error: null,
    })),
    on(A.loadConfigurationsSuccess, (state, { configurations }): LabelState => ({
      ...state,
      configurations,
      isLoadingConfigurations: false,
    })),
    on(A.loadConfigurationsFailure, (state, { error }): LabelState => ({
      ...state,
      isLoadingConfigurations: false,
      error,
    })),

    on(A.loadPrintOptions, (state): LabelState => ({
      ...state,
      isLoadingPrintOptions: true,
      error: null,
    })),
    on(A.loadPrintOptionsSuccess, (state, { printOptions }): LabelState => ({
      ...state,
      printOptions,
      isLoadingPrintOptions: false,
    })),
    on(A.loadPrintOptionsFailure, (state, { error }): LabelState => ({
      ...state,
      isLoadingPrintOptions: false,
      error,
    })),

    on(A.loadConfiguration, (state): LabelState => ({
      ...state,
      isLoadingConfigurations: true,
      error: null,
    })),
    on(A.loadConfigurationSuccess, (state, { label }): LabelState => ({
      ...state,
      selectedConfiguration: label,
      isLoadingConfigurations: false,
    })),
    on(A.loadConfigurationFailure, (state, { error }): LabelState => ({
      ...state,
      isLoadingConfigurations: false,
      error,
    })),
    on(A.clearSelectedConfiguration, (state): LabelState => ({
      ...state,
      selectedConfiguration: null,
    })),

    on(A.loadLabel, pending),
    on(A.loadLabelSuccess, (state, { label }): LabelState => ({
      ...upsert(state, label),
      selectedLabel: label,
    })),
    on(A.loadLabelFailure, failed),

    on(A.createLabel, pending),
    on(A.createLabelSuccess, (state, { label }): LabelState => upsert(state, label)),
    on(A.createLabelFailure, failed),

    on(A.updateLabel, pending),
    on(A.updateLabelSuccess, (state, { label }): LabelState => upsert(state, label)),
    on(A.updateLabelFailure, failed),

    on(A.setLabelActive, pending),
    on(A.setLabelActiveSuccess, (state, { id, isActive }): LabelState => ({
      ...state,
      labels: state.labels.map((l) => (l.id === id ? { ...l, isActive } : l)),
      selectedLabel:
        state.selectedLabel?.template.id === id
          ? withActive(state.selectedLabel, isActive)
          : state.selectedLabel,
      isLoading: false,
    })),
    on(A.setLabelActiveFailure, failed),

    on(A.deleteLabel, pending),
    on(A.deleteLabelSuccess, (state, { id }): LabelState => ({
      ...state,
      labels: state.labels.filter((l) => l.id !== id),
      selectedLabel: state.selectedLabel?.template.id === id ? null : state.selectedLabel,
      isLoading: false,
    })),
    on(A.deleteLabelFailure, failed),

    on(A.clearSelectedLabel, (state): LabelState => ({ ...state, selectedLabel: null })),
  ),
});
