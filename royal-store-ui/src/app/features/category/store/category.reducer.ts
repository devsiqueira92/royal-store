import { createFeature, createReducer, on } from '@ngrx/store';
import { Category } from '../interfaces/category.interface';
import { CategoryActions } from './category.actions';

export interface CategoryState {
  categories: Category[];
  selectedCategory: Category | null;
  isLoading: boolean;
  error: string | null;
}

export const initialCategoryState: CategoryState = {
  categories: [],
  selectedCategory: null,
  isLoading: false,
  error: null,
};

export const categoryFeature = createFeature({
  name: 'category',
  reducer: createReducer(
    initialCategoryState,
    on(CategoryActions.loadCategories, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.loadCategoriesSuccess, (state, { categories }): CategoryState => ({
      ...state,
      categories,
      isLoading: false,
    })),
    on(CategoryActions.loadCategoriesFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.loadCategory, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.loadCategorySuccess, (state, { category }): CategoryState => ({
      ...state,
      selectedCategory: category,
      isLoading: false,
    })),
    on(CategoryActions.loadCategoryFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.createCategory, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.createCategorySuccess, (state, { category }): CategoryState => ({
      ...state,
      categories: [...state.categories, category],
      isLoading: false,
    })),
    on(CategoryActions.createCategoryFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.updateCategory, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.updateCategorySuccess, (state, { category }): CategoryState => ({
      ...state,
      categories: state.categories.map(c => (c.id === category.id ? category : c)),
      selectedCategory:
        state.selectedCategory?.id === category.id ? category : state.selectedCategory,
      isLoading: false,
    })),
    on(CategoryActions.updateCategoryFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.disableCategory, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.disableCategorySuccess, (state, { id }): CategoryState => ({
      ...state,
      categories: state.categories.map(c => (c.id === id ? { ...c, isActive: false } : c)),
      selectedCategory:
        state.selectedCategory?.id === id
          ? { ...state.selectedCategory, isActive: false }
          : state.selectedCategory,
      isLoading: false,
    })),
    on(CategoryActions.disableCategoryFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.deleteCategory, (state): CategoryState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(CategoryActions.deleteCategorySuccess, (state, { id }): CategoryState => ({
      ...state,
      categories: state.categories.filter(c => c.id !== id),
      selectedCategory:
        state.selectedCategory?.id === id ? null : state.selectedCategory,
      isLoading: false,
    })),
    on(CategoryActions.deleteCategoryFailure, (state, { error }): CategoryState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(CategoryActions.clearSelectedCategory, (state): CategoryState => ({
      ...state,
      selectedCategory: null,
    })),
  ),
});
