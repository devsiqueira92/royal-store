import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CurrentSeller,
  UpdateMyProfileRequest,
} from '../interfaces/current-seller.interface';

@Injectable({
  providedIn: 'root',
})
export class CurrentSellerService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/sellers`;

  getCurrent(): Observable<CurrentSeller> {
    return this.http.get<CurrentSeller>(`${this.apiUrl}/me`);
  }

  updateMyProfile(request: UpdateMyProfileRequest): Observable<CurrentSeller> {
    return this.http.put<CurrentSeller>(`${this.apiUrl}/me`, request);
  }
}
