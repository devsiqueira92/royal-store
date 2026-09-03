import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Product } from '../interfaces/product.interface';
import { CreateServiceRequest } from '../interfaces/create-service-request.interface';
import { UpdateServiceRequest } from '../interfaces/update-service-request.interface';

/**
 * Write client for the `api/services` endpoint group. Reads, disable and
 * variant/photo sub-resources are kind-agnostic and stay on ProductService
 * (see contracts/services-api.md).
 */
@Injectable({
  providedIn: 'root',
})
export class ServiceService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/services`;

  create(request: CreateServiceRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  update(id: string, request: UpdateServiceRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }
}
