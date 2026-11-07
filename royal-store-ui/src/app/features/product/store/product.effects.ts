import { inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, concat, concatMap, from, map, of, switchMap } from 'rxjs';
import { ProductService } from '../services/product.service';
import { ProductActions } from './product.actions';

export const loadProducts = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.loadProducts),
      switchMap(({ params }) =>
        productService.getAll(params).pipe(
          map(products => ProductActions.loadProductsSuccess({ products })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.loadProductsFailure({
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

export const loadProductsSummary = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.loadProductsSummary),
      switchMap(({ params }) =>
        productService.getSummary(params).pipe(
          map(summary => ProductActions.loadProductsSummarySuccess({ summary })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.loadProductsSummaryFailure({
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

export const loadProduct = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.loadProduct),
      switchMap(({ id }) =>
        productService.getById(id).pipe(
          map(product => ProductActions.loadProductSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.loadProductFailure({
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

export const createProduct = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.createProduct),
      switchMap(({ request }) =>
        productService.create(request).pipe(
          map(product => ProductActions.createProductSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.createProductFailure({
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

export const saveProductEdit = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.saveProductEdit),
      switchMap(({ id: productId, request }) => {
        return productService.update(productId, request).pipe(
          map(product => ProductActions.updateProductSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.updateProductFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        );
      }),
    );
  },
  {
    functional: true,
  },
);

export const removeProductVariant = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.removeProductVariant),
      concatMap(({ variantId }) =>
        productService.removeVariant(variantId).pipe(
          map(() => ProductActions.removeProductVariantSuccess({ variantId })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.removeProductVariantFailure({
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

export const enableProduct = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.enableProduct),
      concatMap(({ id }) =>
        productService.enable(id).pipe(
          map(() => ProductActions.enableProductSuccess({ id })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.enableProductFailure({
                id,
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

export const disableProduct = createEffect(
  (actions$ = inject(Actions), productService = inject(ProductService)) => {
    return actions$.pipe(
      ofType(ProductActions.disableProduct),
      concatMap(({ id }) =>
        productService.disable(id).pipe(
          map(() => ProductActions.disableProductSuccess({ id })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.disableProductFailure({
                id,
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
