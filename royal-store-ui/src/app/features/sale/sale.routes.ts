import { Routes } from '@angular/router';
import { SalesComponent } from './pages/sales/sales.component';
import { SaleCheckoutComponent } from './pages/sale-checkout/sale-checkout.component';
import { SalesHistoryComponent } from './pages/sales-history/sales-history.component';
import { SaleDetailsComponent } from './pages/sale-details/sale-details.component';
import { saleCartNotEmptyGuard } from './guards/sale-cart-not-empty.guard';

export const SALE_ROUTES: Routes = [
  { path: '', component: SalesComponent },
  {
    path: 'checkout',
    component: SaleCheckoutComponent,
    canActivate: [saleCartNotEmptyGuard],
  },
  { path: 'history', component: SalesHistoryComponent },
  { path: 'history/:id', component: SaleDetailsComponent },
];
