import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { ExpenseActions } from '../store/expense.actions';
import { ExpenseCategory, FinancialAccount } from '../interfaces/expense.interface';
import {
  selectAccountsQuery,
  selectCashFlow,
  selectCategoryReport,
  selectCategoryRows,
  selectCompetenceMonth,
  selectError,
  selectFinancialAccounts,
  selectIsGenerating,
  selectIsLoading,
  selectIsLoadingAccounts,
  selectIsLoadingCategories,
  selectIsLoadingReports,
  selectIsSaving,
  selectLastGeneration,
  selectMonthItems,
  selectMonthSummary,
  selectRecurrences,
  selectSelectableCategories,
  selectSelectedAccount,
  selectSelectedCategory,
  selectSelectedExpense,
} from '../store/expense.selectors';
import {
  CreateExpenseRecurrenceRequest,
  CreateExpenseRequest,
  ExpenseCategoryRequest,
  FinancialAccountRequest,
  RegisterExpensePaymentRequest,
  UpdateExpenseRequest,
} from '../interfaces/expense-request.interface';

@Injectable({
  providedIn: 'root',
})
export class ExpenseFacade {
  private readonly store = inject(Store);

  readonly competenceMonth$ = this.store.select(selectCompetenceMonth);
  readonly items$ = this.store.select(selectMonthItems);
  readonly summary$ = this.store.select(selectMonthSummary);
  readonly selectedExpense$ = this.store.select(selectSelectedExpense);
  /** Only the leaves — what a bill can be filed under. */
  readonly categories$ = this.store.select(selectSelectableCategories);
  /** The whole tree as table rows, including the inactive ones when asked for. */
  readonly categoryRows$ = this.store.select(selectCategoryRows);
  readonly selectedCategory$ = this.store.select(selectSelectedCategory);
  readonly isLoadingCategories$ = this.store.select(selectIsLoadingCategories);
  readonly financialAccounts$ = this.store.select(selectFinancialAccounts);
  readonly selectedAccount$ = this.store.select(selectSelectedAccount);
  readonly accountsQuery$ = this.store.select(selectAccountsQuery);
  readonly isLoadingAccounts$ = this.store.select(selectIsLoadingAccounts);
  readonly recurrences$ = this.store.select(selectRecurrences);
  readonly lastGeneration$ = this.store.select(selectLastGeneration);
  readonly categoryReport$ = this.store.select(selectCategoryReport);
  readonly cashFlow$ = this.store.select(selectCashFlow);
  readonly isLoading$ = this.store.select(selectIsLoading);
  readonly isSaving$ = this.store.select(selectIsSaving);
  readonly isGenerating$ = this.store.select(selectIsGenerating);
  readonly isLoadingReports$ = this.store.select(selectIsLoadingReports);
  readonly error$ = this.store.select(selectError);

  /** @param competenceMonth `yyyy-MM`. */
  loadMonth(competenceMonth: string): void {
    this.store.dispatch(ExpenseActions.loadMonth({ competenceMonth }));
  }

  /** @param includeInactive on only where a deactivated category must still be visible. */
  loadCategories(includeInactive = false): void {
    this.store.dispatch(ExpenseActions.loadCategories({ includeInactive }));
  }

  createCategory(request: ExpenseCategoryRequest): void {
    this.store.dispatch(ExpenseActions.createCategory({ request }));
  }

  updateCategory(id: string, request: ExpenseCategoryRequest): void {
    this.store.dispatch(ExpenseActions.updateCategory({ id, request }));
  }

  setCategoryActive(id: string, active: boolean): void {
    this.store.dispatch(ExpenseActions.setCategoryActive({ id, active }));
  }

  deleteCategory(id: string): void {
    this.store.dispatch(ExpenseActions.deleteCategory({ id }));
  }

  selectCategory(category: ExpenseCategory): void {
    this.store.dispatch(ExpenseActions.selectCategory({ category }));
  }

  clearSelectedCategory(): void {
    this.store.dispatch(ExpenseActions.clearSelectedCategory());
  }

  /**
   * @param includeInactive on only where a deactivated account must stay visible.
   * @param outflowFrom `yyyy-MM-dd` — with `outflowTo`, asks the API for what
   * left each account in that window. Both null leaves the figure out.
   */
  loadFinancialAccounts(
    includeInactive = false,
    outflowFrom: string | null = null,
    outflowTo: string | null = null,
  ): void {
    this.store.dispatch(
      ExpenseActions.loadFinancialAccounts({ includeInactive, outflowFrom, outflowTo }),
    );
  }

  createAccount(request: FinancialAccountRequest): void {
    this.store.dispatch(ExpenseActions.createAccount({ request }));
  }

  updateAccount(id: string, request: FinancialAccountRequest): void {
    this.store.dispatch(ExpenseActions.updateAccount({ id, request }));
  }

  setAccountActive(id: string, active: boolean): void {
    this.store.dispatch(ExpenseActions.setAccountActive({ id, active }));
  }

  deleteAccount(id: string): void {
    this.store.dispatch(ExpenseActions.deleteAccount({ id }));
  }

  selectAccount(account: FinancialAccount): void {
    this.store.dispatch(ExpenseActions.selectAccount({ account }));
  }

  clearSelectedAccount(): void {
    this.store.dispatch(ExpenseActions.clearSelectedAccount());
  }

  loadExpense(id: string): void {
    this.store.dispatch(ExpenseActions.loadExpense({ id }));
  }

  createExpense(request: CreateExpenseRequest): void {
    this.store.dispatch(ExpenseActions.createExpense({ request }));
  }

  updateExpense(id: string, request: UpdateExpenseRequest): void {
    this.store.dispatch(ExpenseActions.updateExpense({ id, request }));
  }

  cancelExpense(id: string): void {
    this.store.dispatch(ExpenseActions.cancelExpense({ id }));
  }

  registerPayment(
    expenseId: string,
    request: RegisterExpensePaymentRequest,
  ): void {
    this.store.dispatch(ExpenseActions.registerPayment({ expenseId, request }));
  }

  loadRecurrences(): void {
    this.store.dispatch(ExpenseActions.loadRecurrences());
  }

  createRecurrence(request: CreateExpenseRecurrenceRequest): void {
    this.store.dispatch(ExpenseActions.createRecurrence({ request }));
  }

  setRecurrenceActive(id: string, active: boolean): void {
    this.store.dispatch(ExpenseActions.setRecurrenceActive({ id, active }));
  }

  generateRecurrences(): void {
    this.store.dispatch(ExpenseActions.generateRecurrences());
  }

  loadCategoryReport(competenceMonth: string, months = 6): void {
    this.store.dispatch(
      ExpenseActions.loadCategoryReport({ competenceMonth, months }),
    );
  }

  loadCashFlow(weeks = 8): void {
    this.store.dispatch(ExpenseActions.loadCashFlow({ weeks }));
  }

  clearSelectedExpense(): void {
    this.store.dispatch(ExpenseActions.clearSelectedExpense());
  }
}
