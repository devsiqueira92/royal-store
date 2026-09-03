import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  TaxProfileTemplate,
  TaxProfileTemplateTargetItemType,
} from '../interfaces/tax-profile-template.interface';
import {
  CreateTaxProfileTemplateRequest,
  UpdateTaxProfileTemplateRequest,
} from '../interfaces/tax-profile-template-request.interface';

@Injectable({
  providedIn: 'root',
})
export class TaxProfileTemplateService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/tax-profile-templates`;

  create(
    request: CreateTaxProfileTemplateRequest,
  ): Observable<TaxProfileTemplate> {
    return this.http.post<TaxProfileTemplate>(this.apiUrl, request);
  }

  update(
    id: string,
    request: UpdateTaxProfileTemplateRequest,
  ): Observable<TaxProfileTemplate> {
    return this.http.put<TaxProfileTemplate>(`${this.apiUrl}/${id}`, request);
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  getAll(
    type?: TaxProfileTemplateTargetItemType,
  ): Observable<TaxProfileTemplate[]> {
    let params = new HttpParams();
    if (type !== undefined) {
      params = params.set('type', type);
    }

    return this.http.get<TaxProfileTemplate[]>(this.apiUrl, { params });
  }
}
