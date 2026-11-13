import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { catchError, concatMap, filter, map, of, switchMap, withLatestFrom } from 'rxjs';
import { ExpenseService } from '../services/expense.service';
import { ExpenseCategoryService } from '../services/expense-category.service';
import { ExpenseRecurrenceService } from '../services/expense-recurrence.service';
import { ExpenseActions } from './expense.actions';
import {
  selectAccountsQuery,
  selectCategoriesIncludeInactive,
  selectCompetenceMonth,
} from './expense.selectors';

const toError = (error: HttpErrorResponse): string => error.error?.message ?? 'error.unknowError';

export const loadMonth = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadMonth),
      switchMap(({ competenceMonth }) =>
        service.getMonth(competenceMonth).pipe(
          map((month) => ExpenseActions.loadMonthSuccess({ month })),
          catchError((error) => of(ExpenseActions.loadMonthFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const loadCategories = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadCategories),
      switchMap(({ includeInactive }) =>
        service.getAll(includeInactive).pipe(
          map((categories) => ExpenseActions.loadCategoriesSuccess({ categories })),
          catchError((error) =>
            of(ExpenseActions.loadCategoriesFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadFinancialAccounts = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadFinancialAccounts),
      switchMap(({ includeInactive, outflowFrom, outflowTo }) =>
        service.getFinancialAccounts(includeInactive, outflowFrom, outflowTo).pipe(
          map((accounts) => ExpenseActions.loadFinancialAccountsSuccess({ accounts })),
          catchError((error) =>
            of(ExpenseActions.loadFinancialAccountsFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const createAccount = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.createAccount),
      concatMap(({ request }) =>
        service.createAccount(request).pipe(
          map((account) => ExpenseActions.createAccountSuccess({ account })),
          catchError((error) => of(ExpenseActions.createAccountFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const updateAccount = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.updateAccount),
      concatMap(({ id, request }) =>
        service.updateAccount(id, request).pipe(
          map((account) => ExpenseActions.updateAccountSuccess({ account })),
          catchError((error) => of(ExpenseActions.updateAccountFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const setAccountActive = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.setAccountActive),
      concatMap(({ id, active }) =>
        (active ? service.activateAccount(id) : service.deactivateAccount(id)).pipe(
          map((account) => ExpenseActions.setAccountActiveSuccess({ account })),
          catchError((error) =>
            of(ExpenseActions.setAccountActiveFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deleteAccount = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.deleteAccount),
      concatMap(({ id }) =>
        service.deleteAccount(id).pipe(
          map(() => ExpenseActions.deleteAccountSuccess({ id })),
          catchError((error) => of(ExpenseActions.deleteAccountFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

/** Re-reads the accounts after a write, with the query the list was loaded with. */
export const reloadAccountsAfterWrite = createEffect(
  (actions$ = inject(Actions), store = inject(Store)) =>
    actions$.pipe(
      ofType(
        ExpenseActions.createAccountSuccess,
        ExpenseActions.updateAccountSuccess,
        ExpenseActions.setAccountActiveSuccess,
        ExpenseActions.deleteAccountSuccess,
      ),
      withLatestFrom(store.select(selectAccountsQuery)),
      map(([, query]) => ExpenseActions.loadFinancialAccounts(query)),
    ),
  { functional: true },
);

export const createCategory = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.createCategory),
      concatMap(({ request }) =>
        service.create(request).pipe(
          map((category) => ExpenseActions.createCategorySuccess({ category })),
          catchError((error) =>
            of(ExpenseActions.createCategoryFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const updateCategory = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.updateCategory),
      concatMap(({ id, request }) =>
        service.update(id, request).pipe(
          map((category) => ExpenseActions.updateCategorySuccess({ category })),
          catchError((error) =>
            of(ExpenseActions.updateCategoryFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const setCategoryActive = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.setCategoryActive),
      concatMap(({ id, active }) =>
        (active ? service.activate(id) : service.deactivate(id)).pipe(
          map((category) => ExpenseActions.setCategoryActiveSuccess({ category })),
          catchError((error) =>
            of(ExpenseActions.setCategoryActiveFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const deleteCategory = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseCategoryService)) =>
    actions$.pipe(
      ofType(ExpenseActions.deleteCategory),
      concatMap(({ id }) =>
        service.delete(id).pipe(
          map(() => ExpenseActions.deleteCategorySuccess({ id })),
          catchError((error) =>
            of(ExpenseActions.deleteCategoryFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

/**
 * Re-reads the tree after any category write, with the same `includeInactive`
 * the list was loaded with. A rename can change a node's position, and a
 * re-parent reshapes two branches, so patching the forest in the reducer would
 * mean re-implementing the API's tree assembly client-side.
 */
export const reloadCategoriesAfterWrite = createEffect(
  (actions$ = inject(Actions), store = inject(Store)) =>
    actions$.pipe(
      ofType(
        ExpenseActions.createCategorySuccess,
        ExpenseActions.updateCategorySuccess,
        ExpenseActions.setCategoryActiveSuccess,
        ExpenseActions.deleteCategorySuccess,
      ),
      withLatestFrom(store.select(selectCategoriesIncludeInactive)),
      map(([, includeInactive]) => ExpenseActions.loadCategories({ includeInactive })),
    ),
  { functional: true },
);

export const loadExpense = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadExpense),
      switchMap(({ id }) =>
        service.getById(id).pipe(
          map((expense) => ExpenseActions.loadExpenseSuccess({ expense })),
          catchError((error) => of(ExpenseActions.loadExpenseFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const createExpense = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.createExpense),
      // `concatMap`: two bills recorded in quick succession must both reach the
      // API, which `switchMap` would silently cancel.
      concatMap(({ request }) =>
        service.create(request).pipe(
          map((expense) => ExpenseActions.createExpenseSuccess({ expense })),
          catchError((error) => of(ExpenseActions.createExpenseFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const updateExpense = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.updateExpense),
      concatMap(({ id, request }) =>
        service.update(id, request).pipe(
          map((expense) => ExpenseActions.updateExpenseSuccess({ expense })),
          catchError((error) => of(ExpenseActions.updateExpenseFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const cancelExpense = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.cancelExpense),
      concatMap(({ id }) =>
        service.cancel(id).pipe(
          map((expense) => ExpenseActions.cancelExpenseSuccess({ expense })),
          catchError((error) => of(ExpenseActions.cancelExpenseFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

export const registerPayment = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.registerPayment),
      concatMap(({ expenseId, request }) =>
        service.registerPayment(expenseId, request).pipe(
          map((expense) => ExpenseActions.registerPaymentSuccess({ expense })),
          catchError((error) =>
            of(ExpenseActions.registerPaymentFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadRecurrences = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseRecurrenceService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadRecurrences),
      switchMap(() =>
        service.getAll().pipe(
          map((recurrences) => ExpenseActions.loadRecurrencesSuccess({ recurrences })),
          catchError((error) =>
            of(ExpenseActions.loadRecurrencesFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const createRecurrence = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseRecurrenceService)) =>
    actions$.pipe(
      ofType(ExpenseActions.createRecurrence),
      concatMap(({ request }) =>
        service.create(request).pipe(
          map((recurrence) => ExpenseActions.createRecurrenceSuccess({ recurrence })),
          catchError((error) =>
            of(ExpenseActions.createRecurrenceFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const setRecurrenceActive = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseRecurrenceService)) =>
    actions$.pipe(
      ofType(ExpenseActions.setRecurrenceActive),
      concatMap(({ id, active }) =>
        (active ? service.activate(id) : service.deactivate(id)).pipe(
          map((recurrence) => ExpenseActions.setRecurrenceActiveSuccess({ recurrence })),
          catchError((error) =>
            of(ExpenseActions.setRecurrenceActiveFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const generateRecurrences = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseRecurrenceService)) =>
    actions$.pipe(
      ofType(ExpenseActions.generateRecurrences),
      concatMap(() =>
        service.generate().pipe(
          map((result) => ExpenseActions.generateRecurrencesSuccess({ result })),
          catchError((error) =>
            of(ExpenseActions.generateRecurrencesFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadCategoryReport = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadCategoryReport),
      switchMap(({ competenceMonth, months }) =>
        service.getByCategoryReport(competenceMonth, months).pipe(
          map((report) => ExpenseActions.loadCategoryReportSuccess({ report })),
          catchError((error) =>
            of(ExpenseActions.loadCategoryReportFailure({ error: toError(error) })),
          ),
        ),
      ),
    ),
  { functional: true },
);

export const loadCashFlow = createEffect(
  (actions$ = inject(Actions), service = inject(ExpenseService)) =>
    actions$.pipe(
      ofType(ExpenseActions.loadCashFlow),
      switchMap(({ weeks }) =>
        service.getCashFlowProjection(weeks).pipe(
          map((projection) => ExpenseActions.loadCashFlowSuccess({ projection })),
          catchError((error) => of(ExpenseActions.loadCashFlowFailure({ error: toError(error) }))),
        ),
      ),
    ),
  { functional: true },
);

/**
 * Anything that can move a bill into, out of, or around the visible month ends
 * with a fresh read of it. The alternative — patching the list locally — would
 * have to re-derive the four summary cards, overdue flags and the listed total
 * client-side, and any drift there shows up as figures that do not add up.
 */
export const reloadMonthAfterWrite = createEffect(
  (actions$ = inject(Actions), store = inject(Store)) =>
    actions$.pipe(
      ofType(
        ExpenseActions.createExpenseSuccess,
        ExpenseActions.cancelExpenseSuccess,
        ExpenseActions.registerPaymentSuccess,
        ExpenseActions.generateRecurrencesSuccess,
      ),
      withLatestFrom(store.select(selectCompetenceMonth)),
      filter(([, competenceMonth]) => !!competenceMonth),
      map(([, competenceMonth]) => ExpenseActions.loadMonth({ competenceMonth: competenceMonth! })),
    ),
  { functional: true },
);

/** A new template changes what the recurrences tab and the projection show. */
export const reloadRecurrencesAfterWrite = createEffect(
  (actions$ = inject(Actions)) =>
    actions$.pipe(
      ofType(ExpenseActions.createRecurrenceSuccess, ExpenseActions.generateRecurrencesSuccess),
      map(() => ExpenseActions.loadRecurrences()),
    ),
  { functional: true },
);
