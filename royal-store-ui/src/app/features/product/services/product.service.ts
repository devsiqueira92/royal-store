import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Product } from '../interfaces/product.interface';
import { CreateProductRequest } from '../interfaces/create-product-request.interface';
import { UpdateProductRequest } from '../interfaces/update-product-request.interface';
import { GetProductsParams } from '../interfaces/get-products-params.interface';
import { STOCK_STATUS_API_VALUE } from '../interfaces/product.interface';
import {
  GetProductsSummaryParams,
  ProductsSummary,
} from '../interfaces/products-summary.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/products`;

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }

  // NOTE: getAll/getById/disable are kind-agnostic — they also serve Services
  // (see contracts/services-api.md), so ServiceService reuses this client
  // for reads and doesn't duplicate them.

  disable(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/disable`, {});
  }

  enable(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/enable`, {});
  }

  getAll(params?: GetProductsParams): Observable<Product[]> {
    let httpParams = new HttpParams();
    if (params?.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }
    if (params?.categoryId !== undefined) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params?.stockStatus !== undefined) {
      httpParams = httpParams.set(
        'stockStatus',
        STOCK_STATUS_API_VALUE[params.stockStatus],
      );
    }
    if (params?.supplierId !== undefined) {
      httpParams = httpParams.set('supplierId', params.supplierId);
    }
    if (params?.search !== undefined) {
      httpParams = httpParams.set('search', params.search);
    }

    return this.http.get<Product[]>(this.apiUrl, { params: httpParams });
  }

  getSummary(
    params?: GetProductsSummaryParams,
  ): Observable<ProductsSummary> {
    let httpParams = new HttpParams();
    if (params?.categoryId !== undefined) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params?.supplierId !== undefined) {
      httpParams = httpParams.set('supplierId', params.supplierId);
    }

    return this.http.get<ProductsSummary>(`${this.apiUrl}/summary`, {
      params: httpParams,
    });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  removeVariant(variantId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/variants/${variantId}`);
  }
}
