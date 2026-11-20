import { Routes } from '@angular/router';
import { ProductsComponent } from './pages/products/products.component';

export const PRODUCT_ROUTES: Routes = [
  { path: '', component: ProductsComponent },
  {
    path: 'labels',
    loadComponent: () =>
      import('./pages/label-print/label-print.component').then(
        m => m.LabelPrintComponent,
      ),
  },
  {
    path: 'labels/templates',
    loadComponent: () =>
      import('./pages/label-list/label-list.component').then(
        m => m.LabelListComponent,
      ),
  },
  {
    path: 'labels/designer',
    loadComponent: () =>
      import('./pages/label-designer/label-designer.component').then(
        m => m.LabelDesignerComponent,
      ),
  },
  {
    path: 'labels/designer/:id',
    loadComponent: () =>
      import('./pages/label-designer/label-designer.component').then(
        m => m.LabelDesignerComponent,
      ),
  },
];
