import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Category } from '../interfaces/category.interface';
import { CreateCategoryRequest } from '../interfaces/create-category-request.interface';
import { UpdateCategoryRequest } from '../interfaces/update-category-request.interface';

export const CategoryActions = createActionGroup({
  source: 'Category',
  events: {
    'Load Categories': emptyProps(),
    'Load Categories Success': props<{ categories: Category[] }>(),
    'Load Categories Failure': props<{ error: string }>(),
    'Load Category': props<{ id: string }>(),
    'Load Category Success': props<{ category: Category }>(),
    'Load Category Failure': props<{ error: string }>(),
    'Create Category': props<{ request: CreateCategoryRequest }>(),
    'Create Category Success': props<{ category: Category }>(),
    'Create Category Failure': props<{ error: string }>(),
    'Update Category': props<{ id: string; request: UpdateCategoryRequest }>(),
    'Update Category Success': props<{ category: Category }>(),
    'Update Category Failure': props<{ error: string }>(),
    'Disable Category': props<{ id: string }>(),
    'Disable Category Success': props<{ id: string }>(),
    'Disable Category Failure': props<{ error: string }>(),
    'Clear Selected Category': emptyProps(),
  },
});
