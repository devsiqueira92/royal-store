import { createFeature, createReducer, on } from '@ngrx/store';
import {
  CashFlowProjection,
  Expense,
  ExpenseCategory,
  ExpenseMonth,
  ExpenseRecurrenceList,
  ExpensesByCategoryReport,
  FinancialAccount,
  RecurrenceGenerationResult,
} from '../interfaces/expense.interface';
import { ExpenseActions } from './expense.actions';

/** The query the financial accounts in state came from. */
export interface FinancialAccountsQuery {
  includeInactive: boolean;
  /** `yyyy-MM-dd`, or null when no outflow period was asked for. */
  outflowFrom: string | null;
  outflowTo: string | null;
}

export interface ExpenseState {
  /** `yyyy-MM` of the month currently on screen. */
  competenceMonth: string | null;
  month: ExpenseMonth | null;
  selectedExpense: Expense | null;
  categories: ExpenseCategory[];
  /** The category the management drawer is editing; null while creating. */
  selectedCategory: ExpenseCategory | null;
  /**
   * Whether the categories in state include the inactive ones. Kept so a write
   * reloads the list the user is actually looking at.
   */
  categoriesIncludeInactive: boolean;
  financialAccounts: FinancialAccount[];
  /** The account the management drawer is editing; null while creating. */
  selectedAccount: FinancialAccount | null;
  /** What the accounts in state were fetched with, so a write reloads the same view. */
  accountsQuery: FinancialAccountsQuery;
  recurrences: ExpenseRecurrenceList | null;
  /** Outcome of the last manual generator run, shown once on the recurrences tab. */
  lastGeneration: RecurrenceGenerationResult | null;
  categoryReport: ExpensesByCategoryReport | null;
  cashFlow: CashFlowProjection | null;
  isLoading: boolean;
  /**
   * Apart from `isLoading`: the categories are fetched alongside the month on
   * the expenses screen, and sharing one flag there left the month table
   * spinning after the categories came back.
   */
  isLoadingCategories: boolean;
  /** Apart from `isLoading` for the same reason as the categories flag above. */
  isLoadingAccounts: boolean;
  /** Tracked apart from `isLoading` so the month list is not blanked while a form submits. */
  isSaving: boolean;
  isGenerating: boolean;
  isLoadingReports: boolean;
  error: string | null;
}

export const initialExpenseState: ExpenseState = {
  competenceMonth: null,
  month: null,
  selectedExpense: null,
  categories: [],
  selectedCategory: null,
  categoriesIncludeInactive: false,
  financialAccounts: [],
  selectedAccount: null,
  accountsQuery: { includeInactive: false, outflowFrom: null, outflowTo: null },
  recurrences: null,
  lastGeneration: null,
  categoryReport: null,
  cashFlow: null,
  isLoading: false,
  isLoadingCategories: false,
  isLoadingAccounts: false,
  isSaving: false,
  isGenerating: false,
  isLoadingReports: false,
  error: null,
};

/**
 * Writes the saved expense back into the month list in place, so the table and
 * the four summary cards do not need a refetch for an edit that cannot change
 * which month the bill belongs to. A create, a cancel or a competence-month
 * change does move it, and those paths reload the month from the effect.
 */
const patchMonthItem = (state: ExpenseState, expense: Expense): ExpenseState => {
  if (!state.month) {
    return { ...state, selectedExpense: expense, isSaving: false };
  }

  return {
    ...state,
    selectedExpense: expense,
    isSaving: false,
    month: {
      ...state.month,
      items: state.month.items.map((item) =>
        item.id === expense.id
          ? {
              ...item,
              dueDate: expense.dueDate,
              description: expense.description,
              payeeName: expense.payeeName,
              category: expense.category,
              amount: expense.amount,
              settledAmount: expense.settledAmount,
              remainingBalance: expense.remainingBalance,
              status: expense.status,
              isOverdue: expense.isOverdue,
              daysOverdue: expense.daysOverdue,
              isEstimated: expense.isEstimated,
              isRecurring: expense.isRecurring,
            }
          : item,
      ),
    },
  };
};

export const expenseFeature = createFeature({
  name: 'expense',
  reducer: createReducer(
    initialExpenseState,

    on(ExpenseActions.loadMonth, (state, { competenceMonth }): ExpenseState => ({
      ...state,
      competenceMonth,
      isLoading: true,
      error: null,
    })),
    on(ExpenseActions.loadMonthSuccess, (state, { month }): ExpenseState => ({
      ...state,
      month,
      isLoading: false,
    })),

    on(ExpenseActions.loadCategories, (state, { includeInactive }): ExpenseState => ({
      ...state,
      categoriesIncludeInactive: includeInactive,
      isLoadingCategories: true,
      error: null,
    })),
    on(ExpenseActions.loadCategoriesSuccess, (state, { categories }): ExpenseState => ({
      ...state,
      categories,
      isLoadingCategories: false,
    })),

    on(
      ExpenseActions.createCategory,
      ExpenseActions.updateCategory,
      ExpenseActions.setCategoryActive,
      ExpenseActions.deleteCategory,
      (state): ExpenseState => ({ ...state, isSaving: true, error: null }),
    ),
    // Every category write ends in a reload of the tree (the effect below), so
    // nothing is patched in place here: a rename can move a node, and a
    // re-parent reshapes two branches at once.
    on(
      ExpenseActions.createCategorySuccess,
      ExpenseActions.updateCategorySuccess,
      ExpenseActions.setCategoryActiveSuccess,
      ExpenseActions.deleteCategorySuccess,
      (state): ExpenseState => ({ ...state, isSaving: false, selectedCategory: null }),
    ),
    on(ExpenseActions.selectCategory, (state, { category }): ExpenseState => ({
      ...state,
      selectedCategory: category,
    })),
    on(ExpenseActions.clearSelectedCategory, (state): ExpenseState => ({
      ...state,
      selectedCategory: null,
    })),
    on(
      ExpenseActions.loadFinancialAccounts,
      (state, { includeInactive, outflowFrom, outflowTo }): ExpenseState => ({
        ...state,
        accountsQuery: { includeInactive, outflowFrom, outflowTo },
        isLoadingAccounts: true,
        error: null,
      }),
    ),
    on(ExpenseActions.loadFinancialAccountsSuccess, (state, { accounts }): ExpenseState => ({
      ...state,
      financialAccounts: accounts,
      isLoadingAccounts: false,
    })),

    on(
      ExpenseActions.createAccount,
      ExpenseActions.updateAccount,
      ExpenseActions.setAccountActive,
      ExpenseActions.deleteAccount,
      (state): ExpenseState => ({ ...state, isSaving: true, error: null }),
    ),
    // Re-read rather than patched, like the categories: the list is ordered by
    // the API and `outflowInPeriod` is computed there, so a local patch would
    // have to reproduce both.
    on(
      ExpenseActions.createAccountSuccess,
      ExpenseActions.updateAccountSuccess,
      ExpenseActions.setAccountActiveSuccess,
      ExpenseActions.deleteAccountSuccess,
      (state): ExpenseState => ({ ...state, isSaving: false, selectedAccount: null }),
    ),
    on(ExpenseActions.selectAccount, (state, { account }): ExpenseState => ({
      ...state,
      selectedAccount: account,
    })),
    on(ExpenseActions.clearSelectedAccount, (state): ExpenseState => ({
      ...state,
      selectedAccount: null,
    })),

    on(ExpenseActions.loadExpenseSuccess, (state, { expense }): ExpenseState => ({
      ...state,
      selectedExpense: expense,
      isLoading: false,
    })),

    on(
      ExpenseActions.createExpense,
      ExpenseActions.updateExpense,
      ExpenseActions.cancelExpense,
      ExpenseActions.registerPayment,
      ExpenseActions.createRecurrence,
      (state): ExpenseState => ({ ...state, isSaving: true, error: null }),
    ),
    on(ExpenseActions.createExpenseSuccess, (state): ExpenseState => ({
      ...state,
      isSaving: false,
      selectedExpense: null,
    })),
    on(
      ExpenseActions.updateExpenseSuccess,
      ExpenseActions.cancelExpenseSuccess,
      ExpenseActions.registerPaymentSuccess,
      (state, { expense }) => patchMonthItem(state, expense),
    ),

    on(ExpenseActions.loadRecurrences, (state): ExpenseState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ExpenseActions.loadRecurrencesSuccess, (state, { recurrences }): ExpenseState => ({
      ...state,
      recurrences,
      isLoading: false,
    })),
    on(ExpenseActions.createRecurrenceSuccess, (state): ExpenseState => ({
      ...state,
      isSaving: false,
    })),
    on(ExpenseActions.setRecurrenceActiveSuccess, (state, { recurrence }): ExpenseState =>
      state.recurrences
        ? {
            ...state,
            recurrences: {
              ...state.recurrences,
              items: state.recurrences.items.map((item) =>
                item.id === recurrence.id ? recurrence : item,
              ),
              activeCount: state.recurrences.activeCount + (recurrence.isActive ? 1 : -1),
            },
          }
        : state,
    ),

    on(ExpenseActions.generateRecurrences, (state): ExpenseState => ({
      ...state,
      isGenerating: true,
      error: null,
    })),
    on(ExpenseActions.generateRecurrencesSuccess, (state, { result }): ExpenseState => ({
      ...state,
      isGenerating: false,
      lastGeneration: result,
    })),

    on(ExpenseActions.loadCategoryReport, ExpenseActions.loadCashFlow, (state): ExpenseState => ({
      ...state,
      isLoadingReports: true,
      error: null,
    })),
    on(ExpenseActions.loadCategoryReportSuccess, (state, { report }): ExpenseState => ({
      ...state,
      categoryReport: report,
      isLoadingReports: false,
    })),
    on(ExpenseActions.loadCashFlowSuccess, (state, { projection }): ExpenseState => ({
      ...state,
      cashFlow: projection,
      isLoadingReports: false,
    })),

    on(ExpenseActions.clearSelectedExpense, (state): ExpenseState => ({
      ...state,
      selectedExpense: null,
    })),

    // Every failure lands here. The three in-flight flags are all cleared
    // because a failed request must never leave a spinner or a disabled submit
    // button behind.
    on(
      ExpenseActions.loadMonthFailure,
      ExpenseActions.loadCategoriesFailure,
      ExpenseActions.createCategoryFailure,
      ExpenseActions.updateCategoryFailure,
      ExpenseActions.setCategoryActiveFailure,
      ExpenseActions.deleteCategoryFailure,
      ExpenseActions.loadFinancialAccountsFailure,
      ExpenseActions.createAccountFailure,
      ExpenseActions.updateAccountFailure,
      ExpenseActions.setAccountActiveFailure,
      ExpenseActions.deleteAccountFailure,
      ExpenseActions.loadExpenseFailure,
      ExpenseActions.createExpenseFailure,
      ExpenseActions.updateExpenseFailure,
      ExpenseActions.cancelExpenseFailure,
      ExpenseActions.registerPaymentFailure,
      ExpenseActions.loadRecurrencesFailure,
      ExpenseActions.createRecurrenceFailure,
      ExpenseActions.setRecurrenceActiveFailure,
      ExpenseActions.generateRecurrencesFailure,
      ExpenseActions.loadCategoryReportFailure,
      ExpenseActions.loadCashFlowFailure,
      (state, { error }): ExpenseState => ({
        ...state,
        isLoading: false,
        isLoadingCategories: false,
        isLoadingAccounts: false,
        isSaving: false,
        isGenerating: false,
        isLoadingReports: false,
        error,
      }),
    ),
  ),
});
