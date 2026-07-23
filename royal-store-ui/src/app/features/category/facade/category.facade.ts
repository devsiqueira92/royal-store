import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateCategoryRequest } from '../interfaces/create-category-request.interface';
import { UpdateCategoryRequest } from '../interfaces/update-category-request.interface';
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

  createCategory(request: CreateCategoryRequest): void {
    this.store.dispatch(CategoryActions.createCategory({ request }));
  }

  updateCategory(id: string, request: UpdateCategoryRequest): void {
    this.store.dispatch(CategoryActions.updateCategory({ id, request }));
  }

  disableCategory(id: string): void {
    this.store.dispatch(CategoryActions.disableCategory({ id }));
  }

  clearSelectedCategory(): void {
    this.store.dispatch(CategoryActions.clearSelectedCategory());
  }
}
