import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Supplier } from '../interfaces/supplier.interface';
import { SupplierRequest } from '../interfaces/supplier-request.interface';

@Injectable({
  providedIn: 'root',
})
export class SupplierService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/suppliers`;

  getAll(isActive?: boolean): Observable<Supplier[]> {
    const params =
      isActive === undefined ? undefined : new HttpParams().set('isActive', isActive);
    return this.http.get<Supplier[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.apiUrl}/${id}`);
  }

  create(request: SupplierRequest): Observable<Supplier> {
    return this.http.post<Supplier>(this.apiUrl, request);
  }

  update(id: string, request: SupplierRequest): Observable<Supplier> {
    return this.http.put<Supplier>(`${this.apiUrl}/${id}`, request);
  }

  activate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }
}
