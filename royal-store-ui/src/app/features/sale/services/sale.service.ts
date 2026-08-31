import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';

@Injectable({
  providedIn: 'root',
})
export class SaleService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/sales`;

  sync(request: SyncSalesRequest): Observable<SyncSalesResponse> {
    return this.http.post<SyncSalesResponse>(`${this.apiUrl}/sync`, request);
  }

  getProducts(categoryId?: string): Observable<PosProduct[]> {
    let params = new HttpParams();
    if (categoryId) {
      params = params.set('category-id', categoryId);
    }

    return this.http.get<PosProduct[]>(`${this.apiUrl}/products`, { params });
  }
}
