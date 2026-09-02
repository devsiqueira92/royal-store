import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { IbptTax } from '../../../core/ibpt-tax/interfaces/ibpt-tax.interface';

@Injectable({
  providedIn: 'root',
})
export class IbptTaxService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/ibpt-taxes`;

  /**
   * @param type 0 = NCM (products), 1 = NBS (services)
   */
  search(q: string, type: number): Observable<IbptTax[]> {
    const params = new HttpParams().set('q', q).set('type', type);
    return this.http.get<IbptTax[]>(this.apiUrl, { params });
  }
}
