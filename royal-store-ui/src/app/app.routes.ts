import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './core/layout/components/admin-layout/admin-layout.component';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard], // Se não estiver logado, o guard barra aqui
    children: [
      {
        path: 'products',
        loadChildren: () =>
          import('./features/product/product.routes').then(
            m => m.PRODUCT_ROUTES,
          ),
      },
      {
        path: 'categories',
        loadChildren: () =>
          import('./features/category/category.routes').then(
            m => m.CATEGORY_ROUTES,
          ),
      },
      {
        path: 'sellers',
        loadChildren: () =>
          import('./features/seller/seller.routes').then(m => m.SELLER_ROUTES),
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
  { path: '**', redirectTo: '' },
];
