import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreatePriceListRequest,
  PriceList,
  SetPriceListRuleRequest,
} from '../interfaces/price-list.interface';

@Injectable({ providedIn: 'root' })
export class PriceListService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/price-lists`;

  getAll(isActive?: boolean): Observable<PriceList[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('is-active', isActive);
    }
    return this.http.get<PriceList[]>(this.apiUrl, { params });
  }

  create(request: CreatePriceListRequest): Observable<PriceList> {
    return this.http.post<PriceList>(this.apiUrl, request);
  }

  // rename(id: string, request: RenamePriceListRequest): Observable<PriceList> {
  //   return this.http.put<PriceList>(`${this.apiUrl}/${id}`, request);
  // }

  setDefault(id: string): Observable<PriceList> {
    return this.http.patch<PriceList>(`${this.apiUrl}/${id}/default`, {});
  }

  activate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  setRule(id: string, request: SetPriceListRuleRequest): Observable<PriceList> {
    return this.http.put<PriceList>(`${this.apiUrl}/${id}/rule`, request);
  }

  clearRule(id: string): Observable<PriceList> {
    return this.http.delete<PriceList>(`${this.apiUrl}/${id}/rule`);
  }
}
