import { Routes } from '@angular/router';
import { SalesComponent } from './pages/sales/sales.component';
import { SalesHistoryComponent } from './pages/sales-history/sales-history.component';

export const SALE_ROUTES: Routes = [
  { path: '', component: SalesComponent },
  { path: 'history', component: SalesHistoryComponent },
];
