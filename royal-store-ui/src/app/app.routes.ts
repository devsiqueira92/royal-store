import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './core/layout/components/admin-layout/admin-layout.component';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: AdminLayoutComponent,
    // canActivate: [authGuard], // Se não estiver logado, o guard barra aqui
    children: [
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
