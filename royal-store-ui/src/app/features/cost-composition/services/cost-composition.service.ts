import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CostComposition,
  CreateCostCompositionRequest,
  UpdateCostCompositionRequest,
} from '../interfaces/cost-composition.interface';

@Injectable({ providedIn: 'root' })
export class CostCompositionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/cost-compositions`;

  getAll(): Observable<CostComposition[]> {
    return this.http.get<CostComposition[]>(this.apiUrl);
  }

  getUnlinked(): Observable<CostComposition[]> {
    return this.http.get<CostComposition[]>(`${this.apiUrl}/unlinked`);
  }

  getById(id: string): Observable<CostComposition> {
    return this.http.get<CostComposition>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateCostCompositionRequest): Observable<CostComposition> {
    return this.http.post<CostComposition>(this.apiUrl, request);
  }

  update(
    id: string,
    request: UpdateCostCompositionRequest,
  ): Observable<CostComposition> {
    return this.http.put<CostComposition>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
