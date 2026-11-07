import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CategoryRequest } from '../interfaces/category-request.interface';
import { CategoryActions } from '../store/category.actions';
import {
  selectCategories,
  selectSelectedCategory,
  selectIsLoading,
  selectError,
} from '../store/category.selectors';

@Injectable({
  providedIn: 'root',
})
export class CategoryFacade {
  private store = inject(Store);

  categories$ = this.store.select(selectCategories);
  selectedCategory$ = this.store.select(selectSelectedCategory);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadCategories(): void {
    this.store.dispatch(CategoryActions.loadCategories());
  }

  loadCategory(id: string): void {
    this.store.dispatch(CategoryActions.loadCategory({ id }));
  }

  createCategory(request: CategoryRequest): void {
    this.store.dispatch(CategoryActions.createCategory({ request }));
  }

  updateCategory(id: string, request: CategoryRequest): void {
    this.store.dispatch(CategoryActions.updateCategory({ id, request }));
  }

  disableCategory(id: string): void {
    this.store.dispatch(CategoryActions.disableCategory({ id }));
  }

  deleteCategory(id: string): void {
    this.store.dispatch(CategoryActions.deleteCategory({ id }));
  }

  clearSelectedCategory(): void {
    this.store.dispatch(CategoryActions.clearSelectedCategory());
  }
}
