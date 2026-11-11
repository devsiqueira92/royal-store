import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SyncSalesRequest } from '../interfaces/sync-sales-request.interface';
import { SyncSalesResponse } from '../interfaces/sync-sales-response.interface';
import { PosProduct } from '../interfaces/pos-product.interface';
import { RegisterSaleRequest } from '../interfaces/register-sale-request.interface';
import { RegisterSaleResponse } from '../interfaces/register-sale-response.interface';
import { SaleListItem } from '../interfaces/sale-list-item.interface';
import { GetSalesListParams } from '../interfaces/get-sales-list-params.interface';
import { SaleDetails } from '../interfaces/sale-details.interface';

@Injectable({
  providedIn: 'root',
})
export class SaleService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/sales`;

  sync(request: SyncSalesRequest): Observable<SyncSalesResponse> {
    return this.http.post<SyncSalesResponse>(`${this.apiUrl}/sync`, request);
  }

  registerSale(request: RegisterSaleRequest): Observable<RegisterSaleResponse> {
    return this.http.post<RegisterSaleResponse>(this.apiUrl, request);
  }

  getProducts(categoryId?: string): Observable<PosProduct[]> {
    let params = new HttpParams();
    if (categoryId) {
      params = params.set('category-id', categoryId);
    }

    return this.http.get<PosProduct[]>(`${this.apiUrl}/products`, { params });
  }

  getAll(params?: GetSalesListParams): Observable<SaleListItem[]> {
    let httpParams = new HttpParams();
    if (params?.sellerName) {
      httpParams = httpParams.set('seller-name', params.sellerName);
    }
    if (params?.customerName) {
      httpParams = httpParams.set('customer-name', params.customerName);
    }
    if (params?.soldFrom) {
      httpParams = httpParams.set('sold-from', params.soldFrom);
    }
    if (params?.soldTo) {
      httpParams = httpParams.set('sold-to', params.soldTo);
    }
    if (params?.fulfillmentMode) {
      httpParams = httpParams.set('fulfillment-mode', params.fulfillmentMode);
    }

    return this.http.get<SaleListItem[]>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<SaleDetails> {
    return this.http.get<SaleDetails>(`${this.apiUrl}/${id}`);
  }
}
