import { Routes } from '@angular/router';
import { SalesComponent } from './pages/sales/sales.component';
import { SalesHistoryComponent } from './pages/sales-history/sales-history.component';
import { SaleDetailsComponent } from './pages/sale-details/sale-details.component';

export const SALE_ROUTES: Routes = [
  { path: '', component: SalesComponent },
  { path: 'history', component: SalesHistoryComponent },
  { path: 'history/:id', component: SaleDetailsComponent },
];
