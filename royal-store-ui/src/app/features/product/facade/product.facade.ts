import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateProductRequest } from '../interfaces/create-product-request.interface';
import { UpdateProductRequest } from '../interfaces/update-product-request.interface';
import { GetProductsParams } from '../interfaces/get-products-params.interface';
import { ProductActions } from '../store/product.actions';
import {
  selectProducts,
  selectSelectedProduct,
  selectIsLoading,
  selectError,
  productVariants,
} from '../store/product.selectors';

@Injectable({
  providedIn: 'root',
})
export class ProductFacade {
  private store = inject(Store);

  products$ = this.store.select(selectProducts);
  selectedProduct$ = this.store.select(selectSelectedProduct);
  productVariants$ = this.store.select(productVariants);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadProducts(params?: GetProductsParams): void {
    this.store.dispatch(ProductActions.loadProducts({ params }));
  }

  loadProduct(id: string): void {
    this.store.dispatch(ProductActions.loadProduct({ id }));
  }

  createProduct(request: CreateProductRequest): void {
    this.store.dispatch(ProductActions.createProduct({ request }));
  }

  saveProductEdit(id: string, request: UpdateProductRequest): void {
    this.store.dispatch(ProductActions.saveProductEdit({ id, request }));
  }

  disableProduct(id: string): void {
    this.store.dispatch(ProductActions.disableProduct({ id }));
  }

  enableProduct(id: string): void {
    this.store.dispatch(ProductActions.enableProduct({ id }));
  }

  removeProductVariant(variantId: string): void {
    this.store.dispatch(ProductActions.removeProductVariant({ variantId }));
  }

  clearSelectedProduct(): void {
    this.store.dispatch(ProductActions.clearSelectedProduct());
  }
}
