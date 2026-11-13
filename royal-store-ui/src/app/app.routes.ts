import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './core/layout/components/admin-layout/admin-layout.component';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  // Public marketing page. `pathMatch: 'full'` keeps it on the root URL only,
  // so every other path still falls through to the authenticated shell below.
  {
    path: 'homepage',
    pathMatch: 'full',
    loadChildren: () => import('./features/landing/landing.routes').then((m) => m.LANDING_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard], // Se não estiver logado, o guard barra aqui
    children: [
      // Default screen of the authenticated area. It lives here instead of a
      // `router.navigate` in the layout constructor, so deep links and the back
      // button keep working.
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      {
        path: 'products',
        loadChildren: () =>
          import('./features/product/product.routes').then((m) => m.PRODUCT_ROUTES),
      },
      {
        path: 'categories',
        loadChildren: () =>
          import('./features/category/category.routes').then((m) => m.CATEGORY_ROUTES),
      },
      {
        path: 'sellers',
        loadChildren: () => import('./features/seller/seller.routes').then((m) => m.SELLER_ROUTES),
      },
      {
        path: 'customers',
        loadChildren: () =>
          import('./features/customer/customer.routes').then((m) => m.CUSTOMER_ROUTES),
      },
      {
        path: 'sales',
        loadChildren: () => import('./features/sale/sale.routes').then((m) => m.SALE_ROUTES),
      },
      {
        path: 'expenses',
        loadChildren: () =>
          import('./features/expense/expense.routes').then((m) => m.EXPENSE_ROUTES),
      },
      {
        path: 'cash-shift',
        loadChildren: () =>
          import('./features/cash-shift/cash-shift.routes').then((m) => m.CASH_SHIFT_ROUTES),
      },
      {
        path: 'payment-methods',
        loadChildren: () =>
          import('./features/payment-method/payment-method.routes').then(
            (m) => m.PAYMENT_METHOD_ROUTES,
          ),
      },
      {
        path: 'loyalty',
        loadChildren: () =>
          import('./features/loyalty/loyalty.routes').then((m) => m.LOYALTY_ROUTES),
      },
      {
        path: 'profile',
        loadChildren: () =>
          import('./features/profile/profile.routes').then((m) => m.PROFILE_ROUTES),
      },
      {
        path: 'suppliers',
        loadChildren: () =>
          import('./features/supplier/supplier.routes').then((m) => m.SUPPLIER_ROUTES),
      },
      {
        path: 'tax-profile-templates',
        loadChildren: () =>
          import('./features/tax-profile-template/tax-profile-template.routes').then(
            (m) => m.TAX_PROFILE_TEMPLATE_ROUTES,
          ),
      },
      //   {
      //     path: '',
      //     redirectTo: 'dashboard',
      //     pathMatch: 'full',
      //   },
      //   {
      //     path: 'dashboard',
      //     loadComponent: () =>
      //       import('./features/dashboard/dashboard.component').then(
      //         m => m.DashboardComponent,
      //       ),
      //   },
      //   {
      //     path: 'customers',
      //     loadComponent: () =>
      //       import('./features/customers/customers.component').then(
      //         m => m.CustomersComponent,
      //       ),
      //   },
    ],
  },

  // Rota de fallback (404 ou redirecionamento)
  { path: '**', redirectTo: 'homepage' },
];
