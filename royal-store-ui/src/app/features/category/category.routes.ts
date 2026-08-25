import { Routes } from '@angular/router';
import { CategoriesComponent } from './pages/categories/categories.component';
import { CreateCategoryComponent } from './components/create-category/create-category.component';

export const CATEGORY_ROUTES: Routes = [
  {
    path: '',
    component: CategoriesComponent,
    children: [{ path: '', component: CreateCategoryComponent }],
  },
];
