import { Routes } from '@angular/router';
import { ExpensesComponent } from './pages/expenses/expenses.component';
import { ExpenseCategoriesComponent } from './pages/expense-categories/expense-categories.component';
import { FinancialAccountsComponent } from './pages/financial-accounts/financial-accounts.component';

export const EXPENSE_ROUTES: Routes = [
  { path: '', component: ExpensesComponent },
  // The two supporting classifications sit under the module they belong to
  // rather than beside the product categories at /categories — different
  // trees, different permissions.
  { path: 'categories', component: ExpenseCategoriesComponent },
  { path: 'financial-accounts', component: FinancialAccountsComponent },
];
