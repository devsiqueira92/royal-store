import { createSelector } from '@ngrx/store';
import { labelFeature } from './label.reducer';

export const {
  selectLabels,
  selectSelectedLabel,
  selectConfigurations: selectLabelConfigurations,
  selectIsLoadingConfigurations: selectLabelConfigurationsIsLoading,
  selectSelectedConfiguration: selectLabelSelectedConfiguration,
  selectPrintOptions: selectLabelPrintOptions,
  selectIsLoadingPrintOptions: selectLabelPrintOptionsIsLoading,
  selectIsLoading: selectLabelIsLoading,
  selectError: selectLabelError,
} = labelFeature;

export const selectActiveLabels = createSelector(selectLabels, (labels) =>
  labels.filter((l) => l.isActive),
);
