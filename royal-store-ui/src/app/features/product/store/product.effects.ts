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
      concatMap(({ id: productId, request, toRemove }) => {
        const updateProduct$ = productService.update(productId, request).pipe(
          map(product => ProductActions.updateProductSuccess({ product })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.updateProductFailure({
                error: error.error?.message ?? 'error.unknowError',
              }),
            ),
          ),
        );

        // Removal isn't part of the batch update contract — it must go
        // through the standalone DELETE endpoint, one variant at a time,
        // after the batch (whose `version` was captured before any removal)
        // has been applied.
        const removes$ = from(toRemove).pipe(
          concatMap(variantId =>
            productService.removeVariant(productId, variantId).pipe(
              map(() =>
                ProductActions.removeProductVariantSuccess({
                  productId,
                  variantId,
                }),
              ),
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

        return concat(updateProduct$, removes$);
      }),
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
      switchMap(({ id }) =>
        productService.disable(id).pipe(
          map(() => ProductActions.disableProductSuccess({ id })),
          catchError((error: HttpErrorResponse) =>
            of(
              ProductActions.disableProductFailure({
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
