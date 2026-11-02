import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Product } from '../interfaces/product.interface';
import { CreateProductRequest } from '../interfaces/create-product-request.interface';
import { UpdateProductRequest } from '../interfaces/update-product-request.interface';
import { GetProductsParams } from '../interfaces/get-products-params.interface';

export const ProductActions = createActionGroup({
  source: 'Product',
  events: {
    'Load Products': props<{ params?: GetProductsParams }>(),
    'Load Products Success': props<{ products: Product[] }>(),
    'Load Products Failure': props<{ error: string }>(),
    'Load Product': props<{ id: string }>(),
    'Load Product Success': props<{ product: Product }>(),
    'Load Product Failure': props<{ error: string }>(),
    'Create Product': props<{ request: CreateProductRequest }>(),
    'Create Product Success': props<{ product: Product }>(),
    'Create Product Failure': props<{ error: string }>(),
    'Update Product Success': props<{ product: Product }>(),
    'Update Product Failure': props<{ error: string }>(),
    // Enable/disable are applied optimistically so the toggle and the list
    // react at once; the failure carries the id to roll the flag back.
    'Disable Product': props<{ id: string }>(),
    'Disable Product Success': props<{ id: string }>(),
    'Disable Product Failure': props<{ id: string; error: string }>(),
    'Enable Product': props<{ id: string }>(),
    'Enable Product Success': props<{ id: string }>(),
    'Enable Product Failure': props<{ id: string; error: string }>(),
    // Sends product fields plus variant adds/updates in a single batched
    // call (backend `PUT /api/products/{id}`). Removal is not part of the
    // batch contract: it is applied immediately through the standalone
    // `DELETE /api/products/variants/{variantId}` endpoint.
    'Save Product Edit': props<{
      id: string;
      request: UpdateProductRequest;
    }>(),
    'Remove Product Variant': props<{ variantId: string }>(),
    'Remove Product Variant Success': props<{ variantId: string }>(),
    'Remove Product Variant Failure': props<{ error: string }>(),
    'Clear Selected Product': emptyProps(),
    'Set Selected Label Products': props<{ products: Product[] }>(),
    'Clear Selected Label Products': emptyProps(),
  },
});
