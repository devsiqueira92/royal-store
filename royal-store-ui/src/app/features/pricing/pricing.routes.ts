import { Routes } from '@angular/router';

export const PRICING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/price-calculator/price-calculator.component').then(
        (m) => m.PriceCalculatorComponent,
      ),
  },
];
