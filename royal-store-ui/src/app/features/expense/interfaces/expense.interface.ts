/**
 * Read models of the operating-expenses module (`api/expenses`).
 *
 * Dates arrive as plain `yyyy-MM-dd` strings (the API uses `DateOnly`), and the
 * competence month as `yyyy-MM`. They are kept as strings on purpose: parsing
 * them into `Date` would drag the browser timezone into a value that has none,
 * which is how a bill due on the 1st starts showing as the previous month.
 */

/** Mirrors `ExpenseResponse.ToStatusName` on the API. */
export type ExpenseStatus = 'pending' | 'partially-paid' | 'paid' | 'cancelled';

/** Mirrors `ExpenseRecurrenceResponse.ToFrequencyName`. */
export type RecurrenceFrequency = 'weekly' | 'monthly' | 'quarterly' | 'semiannually' | 'yearly';

export type FinancialAccountType = 'cash' | 'bank-account' | 'digital-wallet';

export interface ExpenseCategorySummary {
  id: string;
  name: string;
  color: string | null;
}

export interface ExpenseCategory extends ExpenseCategorySummary {
  parentId: string | null;
  isActive: boolean;
  displayOrder: number;
  children: ExpenseCategory[];
}

export interface CostCenter {
  id: string;
  name: string;
  code: string | null;
  isActive: boolean;
}

export interface FinancialAccount {
  id: string;
  name: string;
  type: FinancialAccountType;
  isActive: boolean;
  outflowInPeriod: number | null;
}

export interface ExpensePayment {
  id: string;
  expenseId: string;
  financialAccountId: string;
  financialAccountName: string | null;
  amount: number;
  paidAt: string;
  methodNote: string | null;
  notes: string | null;
  isReversed: boolean;
  reversedAt: string | null;
  reversalReason: string | null;
  registeredBy: string;
  registeredAt: string;
}

export interface ExpenseAttachment {
  id: string;
  expenseId: string;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
  attachedBy: string;
  attachedAt: string;
}

/** One row of the competence-month list. */
export interface ExpenseListItem {
  id: string;
  dueDate: string;
  description: string;
  payeeName: string;
  category: ExpenseCategorySummary;
  amount: number;
  settledAmount: number;
  remainingBalance: number;
  status: ExpenseStatus;
  isOverdue: boolean;
  daysOverdue: number;
  isEstimated: boolean;
  isRecurring: boolean;
}

/** One expense in full, as returned by create, update, cancel and the payment operations. */
export interface Expense {
  id: string;
  description: string;
  payeeName: string;
  supplierId: string | null;
  category: ExpenseCategorySummary;
  costCenterId: string | null;
  financialAccountId: string | null;
  amount: number;
  settledAmount: number;
  remainingBalance: number;
  isEstimated: boolean;
  competenceMonth: string;
  dueDate: string;
  status: ExpenseStatus;
  isOverdue: boolean;
  daysOverdue: number;
  isRecurring: boolean;
  expenseRecurrenceId: string | null;
  notes: string | null;
  payments: ExpensePayment[];
  attachments: ExpenseAttachment[];
}

/**
 * The four headline figures. Always computed over the WHOLE competence month by
 * the API, never over the filtered list — which is why the cards sit above the
 * filters and do not move when one is applied.
 */
export interface ExpenseMonthSummary {
  total: number;
  expenseCount: number;
  paid: number;
  paidPercentage: number;
  pending: number;
  pendingCount: number;
  overdue: number;
  overdueCount: number;
}

export interface ExpenseMonth {
  competenceMonth: string;
  summary: ExpenseMonthSummary;
  items: ExpenseListItem[];
  listedTotal: number;
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ExpenseRecurrence {
  id: string;
  description: string;
  payeeName: string;
  supplierId: string | null;
  category: ExpenseCategorySummary;
  costCenterId: string | null;
  financialAccountId: string | null;
  frequency: RecurrenceFrequency;
  dueDay: number;
  leadDays: number;
  amount: number;
  isVariableAmount: boolean;
  isActive: boolean;
  startDate: string;
  endDate: string | null;
  maxOccurrences: number | null;
  occurrencesGenerated: number;
  generatedThrough: string | null;
  nextBillDate: string | null;
  generatesOn: string | null;
  lastRunAt: string | null;
}

export interface ExpenseRecurrenceList {
  items: ExpenseRecurrence[];
  activeCount: number;
  totalCount: number;
  monthlyTotal: number;
}

export interface RecurrenceGenerationResult {
  templatesEvaluated: number;
  expensesCreated: number;
  cyclesSkipped: number;
  generatedThrough: string | null;
  runAt: string;
}

/**
 * One category's row of the management P&L. A `null` in `values` means NOTHING
 * was recorded that month — rendered as a dash, never as a zero.
 */
export interface ExpenseCategoryReportRow {
  categoryId: string;
  name: string;
  color: string | null;
  values: (number | null)[];
  share: number;
}

export interface ExpensesByCategoryReport {
  months: string[];
  categories: ExpenseCategoryReportRow[];
  totals: number[];
}

export interface CashFlowWeek {
  weekStart: string;
  weekEnd: string;
  overdue: number;
  committed: number;
  projected: number;
}

export interface CashFlowBucket {
  total: number;
  committed: number;
  projected: number;
}

export interface CashFlowProjection {
  overdue: number;
  next30Days: CashFlowBucket;
  fullHorizon: CashFlowBucket;
  weeks: CashFlowWeek[];
}

/**
 * What the list chips filter by. `overdue` is not a status on the API — it is
 * derived from the due date, which is what lets a bill be both partially paid
 * and overdue — so it is offered here as its own choice alongside the statuses.
 */
export type ExpenseStatusFilter = 'all' | 'pending' | 'partially-paid' | 'paid' | 'overdue';

/** One chip: its filter value, its label key and how many rows it would show. */
export interface ExpenseFilterChip {
  value: ExpenseStatusFilter;
  labelKey: string;
  count: number;
}

/**
 * A category as the management screen lists it: the forest flattened to rows,
 * each carrying how deep it sits so the table can indent it.
 *
 * Flattened rather than rendered as a `p-treetable` because the depth here is
 * two levels in practice (a group and its categories), and a flat table keeps
 * the row actions, the status switch and the sort order of a plain list.
 */
export interface ExpenseCategoryRow extends ExpenseCategory {
  /** `0` for a root, `1` for its children, and so on. */
  depth: number;
  /** Direct children. A category with any cannot be deleted by the API. */
  childCount: number;
  /** `Occupation › Rent`, for the parent select and the delete confirmation. */
  path: string;
}
