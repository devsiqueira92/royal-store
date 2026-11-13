import { FormControl } from '@angular/forms';
import { FinancialAccountType, RecurrenceFrequency } from './expense.interface';

/** Write models of `api/expenses` and `api/expense-recurrences`. */

export interface CreateExpenseRequest {
  description: string;
  payeeName: string;
  supplierId: string | null;
  expenseCategoryId: string;
  costCenterId: string | null;
  financialAccountId: string | null;
  amount: number;
  /** `yyyy-MM-dd`, always the first day of the competence month. */
  competenceMonth: string;
  dueDate: string;
  isEstimated: boolean;
}

export type UpdateExpenseRequest = Omit<CreateExpenseRequest, 'isEstimated'>;

export interface RegisterExpensePaymentRequest {
  amount: number;
  paidAt: string;
  financialAccountId: string;
  methodNote: string | null;
  notes: string | null;
}

export interface ReverseExpensePaymentRequest {
  reason: string;
}

export interface CreateExpenseRecurrenceRequest {
  description: string;
  payeeName: string;
  supplierId: string | null;
  expenseCategoryId: string;
  costCenterId: string | null;
  financialAccountId: string | null;
  frequency: RecurrenceFrequency;
  dueDay: number;
  /** How many days before the due date the bill is created. */
  leadDays: number;
  amount: number;
  isVariableAmount: boolean;
  startDate: string;
  endDate: string | null;
  maxOccurrences: number | null;
}

/**
 * The drawer's form. `repeatMonthly` is UI-only: when it is on, the submit
 * creates a recurrence TEMPLATE instead of a single expense, which is why the
 * three fields below it only exist in that branch.
 */
export interface ExpenseFormControls {
  description: FormControl<string | null>;
  payeeName: FormControl<string | null>;
  expenseCategoryId: FormControl<string | null>;
  amount: FormControl<number | null>;
  dueDate: FormControl<string | null>;
  repeatMonthly: FormControl<boolean>;
  isVariableAmount: FormControl<boolean>;
  leadDays: FormControl<number>;
}

/** Write model of `api/expense-categories` (create and update share it). */
export interface ExpenseCategoryRequest {
  name: string;
  parentId: string | null;
  /** A hex value such as `#0f766e`, or null. The API refuses anything else. */
  color: string | null;
  displayOrder: number;
}

export interface ExpenseCategoryFormControls {
  name: FormControl<string | null>;
  parentId: FormControl<string | null>;
  color: FormControl<string | null>;
  /**
   * Typed as a string too, and not out of laziness: the shared input field
   * binds `[type]` as a property, so Angular's selector for
   * `input[type=number]` never matches and the default (string) value accessor
   * is used. The form coerces it on submit — sending `"3"` for an `int` is a
   * 400 out of the deserializer.
   */
  displayOrder: FormControl<number | string>;
}

/**
 * Write model of `api/financial-accounts` (create and update share it).
 *
 * No balance of any kind: an account records where money left from, and the
 * API computes the outflow of a period from the payments themselves.
 */
export interface FinancialAccountRequest {
  name: string;
  type: FinancialAccountType;
}

export interface FinancialAccountFormControls {
  name: FormControl<string | null>;
  type: FormControl<FinancialAccountType>;
}
