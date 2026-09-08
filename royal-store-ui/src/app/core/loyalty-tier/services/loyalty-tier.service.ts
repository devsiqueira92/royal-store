import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoyaltyTier } from '../interfaces/loyalty-tier.interface';

@Injectable({
  providedIn: 'root',
})
export class LoyaltyTierService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/loyalty-tiers`;

  getAll(): Observable<LoyaltyTier[]> {
    return this.http.get<LoyaltyTier[]>(this.apiUrl);
  }
}
