import { createFeature, createReducer, on } from '@ngrx/store';
import { TaxProfileTemplate } from '../interfaces/tax-profile-template.interface';
import { TaxProfileTemplateActions } from './tax-profile-template.actions';

export interface TaxProfileTemplateState {
  templates: TaxProfileTemplate[];
  isLoading: boolean;
  error: string | null;
}

export const initialTaxProfileTemplateState: TaxProfileTemplateState = {
  templates: [],
  isLoading: false,
  error: null,
};

export const taxProfileTemplateFeature = createFeature({
  name: 'taxProfileTemplate',
  reducer: createReducer(
    initialTaxProfileTemplateState,
    on(TaxProfileTemplateActions.loadTemplates, (state): TaxProfileTemplateState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      TaxProfileTemplateActions.loadTemplatesSuccess,
      (state, { templates }): TaxProfileTemplateState => ({
        ...state,
        templates,
        isLoading: false,
      }),
    ),
    on(
      TaxProfileTemplateActions.loadTemplatesFailure,
      (state, { error }): TaxProfileTemplateState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(TaxProfileTemplateActions.createTemplate, (state): TaxProfileTemplateState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      TaxProfileTemplateActions.createTemplateSuccess,
      (state, { template }): TaxProfileTemplateState => ({
        ...state,
        templates: [...state.templates, template],
        isLoading: false,
      }),
    ),
    on(
      TaxProfileTemplateActions.createTemplateFailure,
      (state, { error }): TaxProfileTemplateState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(TaxProfileTemplateActions.updateTemplate, (state): TaxProfileTemplateState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      TaxProfileTemplateActions.updateTemplateSuccess,
      (state, { template }): TaxProfileTemplateState => ({
        ...state,
        templates: state.templates.map(t => (t.id === template.id ? template : t)),
        isLoading: false,
      }),
    ),
    on(
      TaxProfileTemplateActions.updateTemplateFailure,
      (state, { error }): TaxProfileTemplateState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      TaxProfileTemplateActions.deactivateTemplate,
      (state): TaxProfileTemplateState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),
    on(
      TaxProfileTemplateActions.deactivateTemplateSuccess,
      (state, { id }): TaxProfileTemplateState => ({
        ...state,
        templates: state.templates.map(t =>
          t.id === id ? { ...t, isActive: false } : t,
        ),
        isLoading: false,
      }),
    ),
    on(
      TaxProfileTemplateActions.deactivateTemplateFailure,
      (state, { error }): TaxProfileTemplateState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(TaxProfileTemplateActions.clearTemplatesError, (state): TaxProfileTemplateState => ({
      ...state,
      error: null,
    })),
  ),
});
