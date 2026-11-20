import { createFeature, createReducer, on } from '@ngrx/store';
import { Label } from '../interfaces/label-format.interface';
import { LabelActions as A } from './label.actions';

export interface LabelState {
  labels: Label[];
  selectedLabel: Label | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: LabelState = {
  labels: [],
  selectedLabel: null,
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

/** Inserts or replaces `label` (matched by template id) and refreshes the selection. */
const upsert = (state: LabelState, label: Label): LabelState => {
  const id = label.template.id;
  const exists = state.labels.some((l) => l.template.id === id);
  return {
    ...state,
    labels: exists
      ? state.labels.map((l) => (l.template.id === id ? label : l))
      : [...state.labels, label],
    selectedLabel: state.selectedLabel?.template.id === id ? label : state.selectedLabel,
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
      labels: state.labels.map((l) => (l.template.id === id ? withActive(l, isActive) : l)),
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
      labels: state.labels.filter((l) => l.template.id !== id),
      selectedLabel: state.selectedLabel?.template.id === id ? null : state.selectedLabel,
      isLoading: false,
    })),
    on(A.deleteLabelFailure, failed),

    on(A.selectLabel, (state, { id }): LabelState => ({
      ...state,
      selectedLabel: state.labels.find((l) => l.template.id === id) ?? null,
    })),
    on(A.clearSelectedLabel, (state): LabelState => ({ ...state, selectedLabel: null })),
  ),
});
