import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ExpenseRecurrence,
  ExpenseRecurrenceList,
  RecurrenceGenerationResult,
} from '../interfaces/expense.interface';
import { CreateExpenseRecurrenceRequest } from '../interfaces/expense-request.interface';

@Injectable({
  providedIn: 'root',
})
export class ExpenseRecurrenceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/expense-recurrences`;

  getAll(active?: boolean): Observable<ExpenseRecurrenceList> {
    const params = active === undefined ? undefined : new HttpParams().set('active', active);
    return this.http.get<ExpenseRecurrenceList>(this.apiUrl, { params });
  }

  create(request: CreateExpenseRecurrenceRequest): Observable<ExpenseRecurrence> {
    return this.http.post<ExpenseRecurrence>(this.apiUrl, request);
  }

  update(id: string, request: CreateExpenseRecurrenceRequest): Observable<ExpenseRecurrence> {
    return this.http.put<ExpenseRecurrence>(`${this.apiUrl}/${id}`, request);
  }

  activate(id: string): Observable<ExpenseRecurrence> {
    return this.http.patch<ExpenseRecurrence>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<ExpenseRecurrence> {
    return this.http.patch<ExpenseRecurrence>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  /**
   * Runs the generator now. Dispatches the same command as the nightly worker,
   * so a manual run can never behave differently from the scheduled one.
   */
  generate(): Observable<RecurrenceGenerationResult> {
    return this.http.post<RecurrenceGenerationResult>(`${this.apiUrl}/generate`, {});
  }
}
