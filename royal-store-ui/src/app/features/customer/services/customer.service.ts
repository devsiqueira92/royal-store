import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Customer } from '../interfaces/customer.interface';
import { CreateCustomerRequest } from '../interfaces/create-customer-request.interface';
import { UpdateCustomerRequest } from '../interfaces/update-customer-request.interface';
import { CashbackEntry } from '../interfaces/cashback-entry.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/customers`;

  create(request: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<Customer>(this.apiUrl, request);
  }

  update(id: string, request: UpdateCustomerRequest): Observable<Customer> {
    return this.http.put<Customer>(`${this.apiUrl}/${id}`, request);
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  reactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/reactivate`, {});
  }

  getAll(): Observable<Customer[]> {
    return this.http.get<Customer[]>(this.apiUrl);
  }

  getById(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${id}`);
  }

  getCashbackHistory(id: string): Observable<CashbackEntry[]> {
    return this.http.get<CashbackEntry[]>(
      `${this.apiUrl}/${id}/cashback-history`,
    );
  }

  // Usable balance: what the customer can actually spend right now (never negative).
  getCashbackBalance(id: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${id}/cashback-balance`);
  }
}
