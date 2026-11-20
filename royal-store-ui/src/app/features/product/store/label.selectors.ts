import { createSelector } from '@ngrx/store';
import { labelFeature } from './label.reducer';

export const {
  selectLabels,
  selectSelectedLabel,
  selectIsLoading: selectLabelIsLoading,
  selectError: selectLabelError,
} = labelFeature;

export const selectActiveLabels = createSelector(selectLabels, (labels) =>
  labels.filter((l) => l.template.isActive),
);
