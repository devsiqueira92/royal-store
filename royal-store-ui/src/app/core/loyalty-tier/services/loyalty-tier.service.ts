import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';
import {
  CreateLoyaltyTierRequest,
  UpdateLoyaltyTierRequest,
} from '../interfaces/loyalty-tier-request.interface';

@Injectable({
  providedIn: 'root',
})
export class LoyaltyTierService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/loyalty-tiers`;

  getAll(): Observable<LoyaltyTier[]> {
    return this.http.get<LoyaltyTier[]>(this.apiUrl);
  }

  getById(id: string): Observable<LoyaltyTier> {
    return this.http.get<LoyaltyTier>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateLoyaltyTierRequest): Observable<LoyaltyTier> {
    return this.http.post<LoyaltyTier>(this.apiUrl, request);
  }

  update(
    id: string,
    request: UpdateLoyaltyTierRequest,
  ): Observable<LoyaltyTier> {
    return this.http.put<LoyaltyTier>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }
}
