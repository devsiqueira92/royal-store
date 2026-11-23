import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Options } from '@app/shared/interceptors/options.interface';
import {
  CreateLabelRequest,
  Label,
  LabelSummary,
  UpdateLabelRequest,
} from '../interfaces/label-format.interface';

@Injectable({ providedIn: 'root' })
export class LabelService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/labels`;

  /** `isActive` omitted → every label. */
  getAll(isActive?: boolean): Observable<LabelSummary[]> {
    let params = new HttpParams();
    if (isActive != null) {
      params = params.set('is-active', isActive);
    }
    return this.http.get<LabelSummary[]>(this.apiUrl, { params });
  }

  /** System configurations + the store's active labels (`code` = label id). */
  getConfigurations(): Observable<Options[]> {
    return this.http.get<Options[]>(`${this.apiUrl}/configurations`);
  }

  /** Store's active labels (default first) + system configurations, for printing (`code` = label id). */
  getPrintOptions(): Observable<Options[]> {
    return this.http.get<Options[]>(`${this.apiUrl}/print-options`);
  }

  getById(id: string): Observable<Label> {
    return this.http.get<Label>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateLabelRequest): Observable<Label> {
    return this.http.post<Label>(this.apiUrl, request);
  }

  update(id: string, request: UpdateLabelRequest): Observable<Label> {
    return this.http.put<Label>(`${this.apiUrl}/${id}`, request);
  }

  activate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, null);
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, null);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
