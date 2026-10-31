import { createSelector } from '@ngrx/store';
import { taxProfileTemplateFeature } from './tax-profile-template.reducer';

export const {
  selectTemplates,
  selectIsLoading,
  selectError,
  selectSelectedTemplateId,
} = taxProfileTemplateFeature;

export const selectSelectedTemplate = createSelector(
  selectTemplates,
  selectSelectedTemplateId,
  (templates, id) => templates.find(template => template.id === id) ?? null,
);
