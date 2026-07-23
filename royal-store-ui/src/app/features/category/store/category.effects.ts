import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { CategoryService } from '../services/category.service';
import { CategoryActions } from './category.actions';

export const loadCategories = createEffect(
  (actions$ = inject(Actions), categoryService = inject(CategoryService)) => {
    return actions$.pipe(
      ofType(CategoryActions.loadCategories),
      switchMap(() =>
        categoryService.getAll().pipe(
          map(categories => CategoryActions.loadCategoriesSuccess({ categories })),
          catchError((error: HttpErrorResponse) =>
            of(
              CategoryActions.loadCategoriesFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const loadCategory = createEffect(
  (actions$ = inject(Actions), categoryService = inject(CategoryService)) => {
    return actions$.pipe(
      ofType(CategoryActions.loadCategory),
      switchMap(({ id }) =>
        categoryService.getById(id).pipe(
          map(category => CategoryActions.loadCategorySuccess({ category })),
          catchError((error: HttpErrorResponse) =>
            of(
              CategoryActions.loadCategoryFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const createCategory = createEffect(
  (actions$ = inject(Actions), categoryService = inject(CategoryService)) => {
    return actions$.pipe(
      ofType(CategoryActions.createCategory),
      switchMap(({ request }) =>
        categoryService.create(request).pipe(
          map(category => CategoryActions.createCategorySuccess({ category })),
          catchError((error: HttpErrorResponse) =>
            of(
              CategoryActions.createCategoryFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const updateCategory = createEffect(
  (actions$ = inject(Actions), categoryService = inject(CategoryService)) => {
    return actions$.pipe(
      ofType(CategoryActions.updateCategory),
      switchMap(({ id, request }) =>
        categoryService.update(id, request).pipe(
          map(category => CategoryActions.updateCategorySuccess({ category })),
          catchError((error: HttpErrorResponse) =>
            of(
              CategoryActions.updateCategoryFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);

export const disableCategory = createEffect(
  (actions$ = inject(Actions), categoryService = inject(CategoryService)) => {
    return actions$.pipe(
      ofType(CategoryActions.disableCategory),
      switchMap(({ id }) =>
        categoryService.disable(id).pipe(
          map(() => CategoryActions.disableCategorySuccess({ id })),
          catchError((error: HttpErrorResponse) =>
            of(
              CategoryActions.disableCategoryFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        ),
      ),
    );
  },
  {
    functional: true,
  },
);
