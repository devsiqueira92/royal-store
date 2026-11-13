import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CashFlowProjection,
  Expense,
  ExpenseMonth,
  ExpensesByCategoryReport,
} from '../interfaces/expense.interface';
import {
  CreateExpenseRequest,
  RegisterExpensePaymentRequest,
  ReverseExpensePaymentRequest,
  UpdateExpenseRequest,
} from '../interfaces/expense-request.interface';

@Injectable({
  providedIn: 'root',
})
export class ExpenseService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/expenses`;

  /**
   * The month view. The status/category filters the API also accepts are left
   * unused on purpose: the whole month is fetched once and narrowed in the
   * page, so a chip click costs no round trip and the chip counts stay exact
   * instead of being read off a filtered page.
   */
  getMonth(competenceMonth: string, pageSize = 200): Observable<ExpenseMonth> {
    const params = new HttpParams()
      .set('competence-month', competenceMonth)
      .set('page-size', pageSize);

    return this.http.get<ExpenseMonth>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Expense> {
    return this.http.get<Expense>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateExpenseRequest): Observable<Expense> {
    return this.http.post<Expense>(this.apiUrl, request);
  }

  update(id: string, request: UpdateExpenseRequest): Observable<Expense> {
    return this.http.put<Expense>(`${this.apiUrl}/${id}`, request);
  }

  cancel(id: string): Observable<Expense> {
    return this.http.patch<Expense>(`${this.apiUrl}/${id}/cancel`, {});
  }

  registerPayment(expenseId: string, request: RegisterExpensePaymentRequest): Observable<Expense> {
    return this.http.post<Expense>(`${this.apiUrl}/${expenseId}/payments`, request);
  }

  reversePayment(
    expenseId: string,
    paymentId: string,
    request: ReverseExpensePaymentRequest,
  ): Observable<Expense> {
    return this.http.patch<Expense>(
      `${this.apiUrl}/${expenseId}/payments/${paymentId}/reverse`,
      request,
    );
  }

  /** Management P&L: `months` competence months ending at `competenceMonth`. */
  getByCategoryReport(competenceMonth: string, months = 6): Observable<ExpensesByCategoryReport> {
    const params = new HttpParams().set('competence-month', competenceMonth).set('months', months);

    return this.http.get<ExpensesByCategoryReport>(`${this.apiUrl}/reports/by-category`, {
      params,
    });
  }

  getCashFlowProjection(weeks = 8): Observable<CashFlowProjection> {
    const params = new HttpParams().set('weeks', weeks);
    return this.http.get<CashFlowProjection>(`${this.apiUrl}/reports/cash-flow`, { params });
  }
}
