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
    'Update Product': props<{ id: string; request: UpdateProductRequest }>(),
    'Update Product Success': props<{ product: Product }>(),
    'Update Product Failure': props<{ error: string }>(),
    'Disable Product': props<{ id: string }>(),
    'Disable Product Success': props<{ id: string }>(),
    'Disable Product Failure': props<{ error: string }>(),
    'Clear Selected Product': emptyProps(),
  },
});
