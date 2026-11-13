import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ExpenseCategory, FinancialAccount } from '../interfaces/expense.interface';
import {
  ExpenseCategoryRequest,
  FinancialAccountRequest,
} from '../interfaces/expense-request.interface';

@Injectable({
  providedIn: 'root',
})
export class ExpenseCategoryService {
  private readonly http = inject(HttpClient);
  private readonly categoriesUrl = `${environment.apiUrl}/api/expense-categories`;
  private readonly accountsUrl = `${environment.apiUrl}/api/financial-accounts`;

  /** Returns the forest — each root carries its descendants in `children`. */
  getAll(includeInactive = false): Observable<ExpenseCategory[]> {
    const params = new HttpParams().set('include-inactive', includeInactive);
    return this.http.get<ExpenseCategory[]>(this.categoriesUrl, { params });
  }

  create(request: ExpenseCategoryRequest): Observable<ExpenseCategory> {
    return this.http.post<ExpenseCategory>(this.categoriesUrl, request);
  }

  update(id: string, request: ExpenseCategoryRequest): Observable<ExpenseCategory> {
    return this.http.put<ExpenseCategory>(`${this.categoriesUrl}/${id}`, request);
  }

  activate(id: string): Observable<ExpenseCategory> {
    return this.http.patch<ExpenseCategory>(`${this.categoriesUrl}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<ExpenseCategory> {
    return this.http.patch<ExpenseCategory>(`${this.categoriesUrl}/${id}/deactivate`, {});
  }

  /**
   * Only an unused category can go. One referenced by an expense, a template or
   * a child comes back as a 409 telling the user to deactivate it instead —
   * which the global error toast shows as-is.
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
  }

  /**
   * Where money leaves from. Needed before a payment can be registered, so it
   * is loaded alongside the categories rather than on demand.
   *
   * With a period, each account also carries what left it in that window
   * (`outflowInPeriod`); without one the API returns null for it, because an
   * account stores no balance.
   *
   * @param outflowFrom `yyyy-MM-dd`, inclusive.
   * @param outflowTo `yyyy-MM-dd`, inclusive.
   */
  getFinancialAccounts(
    includeInactive = false,
    outflowFrom: string | null = null,
    outflowTo: string | null = null,
  ): Observable<FinancialAccount[]> {
    let params = new HttpParams().set('include-inactive', includeInactive);

    if (outflowFrom && outflowTo) {
      params = params.set('outflow-from', outflowFrom).set('outflow-to', outflowTo);
    }

    return this.http.get<FinancialAccount[]>(this.accountsUrl, { params });
  }

  createAccount(request: FinancialAccountRequest): Observable<FinancialAccount> {
    return this.http.post<FinancialAccount>(this.accountsUrl, request);
  }

  updateAccount(id: string, request: FinancialAccountRequest): Observable<FinancialAccount> {
    return this.http.put<FinancialAccount>(`${this.accountsUrl}/${id}`, request);
  }

  activateAccount(id: string): Observable<FinancialAccount> {
    return this.http.patch<FinancialAccount>(`${this.accountsUrl}/${id}/activate`, {});
  }

  deactivateAccount(id: string): Observable<FinancialAccount> {
    return this.http.patch<FinancialAccount>(`${this.accountsUrl}/${id}/deactivate`, {});
  }

  /**
   * Only an account no money ever left can go. One referenced by a payment, an
   * expense or a template comes back as a 409 — its payment history would
   * otherwise lose the answer to "where did this go?".
   */
  deleteAccount(id: string): Observable<void> {
    return this.http.delete<void>(`${this.accountsUrl}/${id}`);
  }
}
