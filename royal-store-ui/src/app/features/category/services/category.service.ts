import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { delay, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Category } from '../interfaces/category.interface';
import { CategoryRequest } from '../interfaces/category-request.interface';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/categories`;

  create(request: CategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, request);
  }

  update(id: string, request: CategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${id}`, request);
  }

  disable(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/disable`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  getById(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`); //.pipe(delay(3000));
  }
}
