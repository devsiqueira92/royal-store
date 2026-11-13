import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CashFlowProjection,
  Expense,
  ExpenseCategory,
  ExpenseMonth,
  ExpenseRecurrence,
  ExpenseRecurrenceList,
  ExpensesByCategoryReport,
  FinancialAccount,
  RecurrenceGenerationResult,
} from '../interfaces/expense.interface';
import {
  CreateExpenseRecurrenceRequest,
  CreateExpenseRequest,
  ExpenseCategoryRequest,
  FinancialAccountRequest,
  RegisterExpensePaymentRequest,
  UpdateExpenseRequest,
} from '../interfaces/expense-request.interface';

export const ExpenseActions = createActionGroup({
  source: 'Expense',
  events: {
    /** `competenceMonth` is `yyyy-MM`; the reducer keeps it so a reload uses the same month. */
    'Load Month': props<{ competenceMonth: string }>(),
    'Load Month Success': props<{ month: ExpenseMonth }>(),
    'Load Month Failure': props<{ error: string }>(),

    /**
     * `includeInactive` is off everywhere a bill is recorded and on in the
     * category management screen, which has to show what it can reactivate.
     */
    'Load Categories': props<{ includeInactive: boolean }>(),
    'Load Categories Success': props<{ categories: ExpenseCategory[] }>(),
    'Load Categories Failure': props<{ error: string }>(),

    /**
     * `outflowFrom`/`outflowTo` (`yyyy-MM-dd`) ask the API for each account's
     * outflow in that window; null leaves it out, and the figure comes back null.
     */
    'Load Financial Accounts': props<{
      includeInactive: boolean;
      outflowFrom: string | null;
      outflowTo: string | null;
    }>(),
    'Load Financial Accounts Success': props<{ accounts: FinancialAccount[] }>(),
    'Load Financial Accounts Failure': props<{ error: string }>(),

    'Load Expense': props<{ id: string }>(),
    'Load Expense Success': props<{ expense: Expense }>(),
    'Load Expense Failure': props<{ error: string }>(),

    'Create Expense': props<{ request: CreateExpenseRequest }>(),
    'Create Expense Success': props<{ expense: Expense }>(),
    'Create Expense Failure': props<{ error: string }>(),

    'Update Expense': props<{ id: string; request: UpdateExpenseRequest }>(),
    'Update Expense Success': props<{ expense: Expense }>(),
    'Update Expense Failure': props<{ error: string }>(),

    'Cancel Expense': props<{ id: string }>(),
    'Cancel Expense Success': props<{ expense: Expense }>(),
    'Cancel Expense Failure': props<{ error: string }>(),

    'Register Payment': props<{ expenseId: string; request: RegisterExpensePaymentRequest }>(),
    'Register Payment Success': props<{ expense: Expense }>(),
    'Register Payment Failure': props<{ error: string }>(),

    'Load Recurrences': emptyProps(),
    'Load Recurrences Success': props<{ recurrences: ExpenseRecurrenceList }>(),
    'Load Recurrences Failure': props<{ error: string }>(),

    'Create Recurrence': props<{ request: CreateExpenseRecurrenceRequest }>(),
    'Create Recurrence Success': props<{ recurrence: ExpenseRecurrence }>(),
    'Create Recurrence Failure': props<{ error: string }>(),

    'Set Recurrence Active': props<{ id: string; active: boolean }>(),
    'Set Recurrence Active Success': props<{ recurrence: ExpenseRecurrence }>(),
    'Set Recurrence Active Failure': props<{ error: string }>(),

    'Generate Recurrences': emptyProps(),
    'Generate Recurrences Success': props<{ result: RecurrenceGenerationResult }>(),
    'Generate Recurrences Failure': props<{ error: string }>(),

    'Load Category Report': props<{ competenceMonth: string; months: number }>(),
    'Load Category Report Success': props<{ report: ExpensesByCategoryReport }>(),
    'Load Category Report Failure': props<{ error: string }>(),

    'Load Cash Flow': props<{ weeks: number }>(),
    'Load Cash Flow Success': props<{ projection: CashFlowProjection }>(),
    'Load Cash Flow Failure': props<{ error: string }>(),

    'Create Account': props<{ request: FinancialAccountRequest }>(),
    'Create Account Success': props<{ account: FinancialAccount }>(),
    'Create Account Failure': props<{ error: string }>(),

    'Update Account': props<{ id: string; request: FinancialAccountRequest }>(),
    'Update Account Success': props<{ account: FinancialAccount }>(),
    'Update Account Failure': props<{ error: string }>(),

    'Set Account Active': props<{ id: string; active: boolean }>(),
    'Set Account Active Success': props<{ account: FinancialAccount }>(),
    'Set Account Active Failure': props<{ error: string }>(),

    'Delete Account': props<{ id: string }>(),
    'Delete Account Success': props<{ id: string }>(),
    'Delete Account Failure': props<{ error: string }>(),

    /** The row the drawer edits — there is no GET by id on the API. */
    'Select Account': props<{ account: FinancialAccount }>(),
    'Clear Selected Account': emptyProps(),

    'Create Category': props<{ request: ExpenseCategoryRequest }>(),
    'Create Category Success': props<{ category: ExpenseCategory }>(),
    'Create Category Failure': props<{ error: string }>(),

    'Update Category': props<{ id: string; request: ExpenseCategoryRequest }>(),
    'Update Category Success': props<{ category: ExpenseCategory }>(),
    'Update Category Failure': props<{ error: string }>(),

    'Set Category Active': props<{ id: string; active: boolean }>(),
    'Set Category Active Success': props<{ category: ExpenseCategory }>(),
    'Set Category Active Failure': props<{ error: string }>(),

    'Delete Category': props<{ id: string }>(),
    'Delete Category Success': props<{ id: string }>(),
    'Delete Category Failure': props<{ error: string }>(),

    /**
     * The row the drawer edits. There is no GET by id on the API — the list
     * already carries the whole category — so the chosen row is put in the
     * store instead of being refetched.
     */
    'Select Category': props<{ category: ExpenseCategory }>(),
    'Clear Selected Category': emptyProps(),

    'Clear Selected Expense': emptyProps(),
  },
});
