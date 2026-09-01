import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { IbptTax } from '../interfaces/ibpt-tax.interface';

@Injectable({
  providedIn: 'root',
})
export class IbptTaxService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/ibpt-taxes`;

  search(q: string): Observable<IbptTax[]> {
    const params = new HttpParams().set('q', q);
    return this.http.get<IbptTax[]>(this.apiUrl, { params });
  }
}
