import { createSelector } from '@ngrx/store';
import { expenseFeature } from './expense.reducer';
import {
  ExpenseCategory,
  ExpenseCategoryRow,
  ExpenseCategorySummary,
} from '../interfaces/expense.interface';

export const {
  selectCompetenceMonth,
  selectMonth,
  selectSelectedExpense,
  selectCategories,
  selectSelectedCategory,
  selectCategoriesIncludeInactive,
  selectIsLoadingCategories,
  selectFinancialAccounts,
  selectSelectedAccount,
  selectAccountsQuery,
  selectIsLoadingAccounts,
  selectRecurrences,
  selectLastGeneration,
  selectCategoryReport,
  selectCashFlow,
  selectIsLoading,
  selectIsSaving,
  selectIsGenerating,
  selectIsLoadingReports,
  selectError,
} = expenseFeature;

export const selectMonthItems = createSelector(selectMonth, (month) => month?.items ?? []);

/**
 * The forest flattened into table rows, parents immediately followed by their
 * children, each row carrying its depth and its full path. The order is the
 * API's: `displayOrder`, then name.
 */
export const selectCategoryRows = createSelector(
  selectCategories,
  (categories): ExpenseCategoryRow[] => {
    const rows: ExpenseCategoryRow[] = [];

    const walk = (nodes: ExpenseCategory[], depth: number, prefix: string): void => {
      // 16 matches the API's own guard: a cycle in stored data must not hang
      // the screen, even though the domain refuses to create one.
      if (depth > 16) {
        return;
      }

      for (const node of nodes) {
        const path = prefix ? `${prefix} › ${node.name}` : node.name;

        rows.push({ ...node, depth, childCount: node.children.length, path });
        walk(node.children, depth + 1, path);
      }
    };

    walk(categories, 0, '');

    return rows;
  },
);

export const selectMonthSummary = createSelector(selectMonth, (month) => month?.summary ?? null);

/**
 * The category forest flattened to the leaves a bill can actually be filed
 * under, plus the roots that have none. A parent with children is a grouping in
 * the reports (its figures include every descendant's), so offering it as a
 * target would create expenses that double-count in its own report row.
 */
export const selectSelectableCategories = createSelector(
  selectCategories,
  (categories): ExpenseCategorySummary[] => {
    const flatten = (nodes: ExpenseCategory[], depth = 0): ExpenseCategorySummary[] =>
      nodes.flatMap((node) =>
        node.children.length && depth < 16
          ? flatten(node.children, depth + 1)
          : [{ id: node.id, name: node.name, color: node.color }],
      );

    return flatten(categories);
  },
);
