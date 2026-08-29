import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Seller } from '../interfaces/seller.interface';
import { CreateSellerRequest } from '../interfaces/create-seller-request.interface';
import { UpdateSellerRequest } from '../interfaces/update-seller-request.interface';
import { UpdateSellerPermissionsRequest } from '../interfaces/update-seller-permissions-request.interface';
import { SellerPermissions } from '../interfaces/seller-permissions.interface';
import { GetSellersParams } from '../interfaces/get-sellers-params.interface';

@Injectable({
  providedIn: 'root',
})
export class SellerService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/sellers`;

  create(request: CreateSellerRequest): Observable<Seller> {
    return this.http.post<Seller>(this.apiUrl, request);
  }

  update(id: string, request: UpdateSellerRequest): Observable<Seller> {
    return this.http.put<Seller>(`${this.apiUrl}/${id}`, request);
  }

  updatePermissions(
    id: string,
    request: UpdateSellerPermissionsRequest,
  ): Observable<SellerPermissions> {
    return this.http.put<SellerPermissions>(
      `${this.apiUrl}/${id}/permissions`,
      request,
    );
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  getAll(params?: GetSellersParams): Observable<Seller[]> {
    let httpParams = new HttpParams();
    if (params?.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    return this.http.get<Seller[]>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<Seller> {
    return this.http.get<Seller>(`${this.apiUrl}/${id}`);
  }
}
