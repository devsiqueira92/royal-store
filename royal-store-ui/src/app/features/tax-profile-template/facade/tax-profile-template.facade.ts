import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { TaxProfileTemplateTargetItemType } from '../interfaces/tax-profile-template.interface';
import {
  CreateTaxProfileTemplateRequest,
  UpdateTaxProfileTemplateRequest,
} from '../interfaces/tax-profile-template-request.interface';
import { TaxProfileTemplateActions } from '../store/tax-profile-template.actions';
import {
  selectTemplates,
  selectIsLoading,
  selectError,
} from '../store/tax-profile-template.selectors';

@Injectable({
  providedIn: 'root',
})
export class TaxProfileTemplateFacade {
  private store = inject(Store);

  templates$ = this.store.select(selectTemplates);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadTemplates(itemType?: TaxProfileTemplateTargetItemType): void {
    this.store.dispatch(TaxProfileTemplateActions.loadTemplates({ itemType }));
  }

  createTemplate(request: CreateTaxProfileTemplateRequest): void {
    this.store.dispatch(TaxProfileTemplateActions.createTemplate({ request }));
  }

  updateTemplate(id: string, request: UpdateTaxProfileTemplateRequest): void {
    this.store.dispatch(
      TaxProfileTemplateActions.updateTemplate({ id, request }),
    );
  }

  deactivateTemplate(id: string): void {
    this.store.dispatch(TaxProfileTemplateActions.deactivateTemplate({ id }));
  }
}
