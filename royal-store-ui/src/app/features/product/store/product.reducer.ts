import { createFeature, createReducer, on } from '@ngrx/store';
import { Product } from '../interfaces/product.interface';
import { ProductActions } from './product.actions';

export interface ProductState {
  products: Product[];
  selectedProduct: Product | null;
  isLoading: boolean;
  error: string | null;
}

export const initialProductState: ProductState = {
  products: [],
  selectedProduct: null,
  isLoading: false,
  error: null,
};

export const productFeature = createFeature({
  name: 'product',
  reducer: createReducer(
    initialProductState,
    on(ProductActions.loadProducts, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ProductActions.loadProductsSuccess, (state, { products }): ProductState => ({
      ...state,
      products,
      isLoading: false,
    })),
    on(ProductActions.loadProductsFailure, (state, { error }): ProductState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(ProductActions.loadProduct, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ProductActions.loadProductSuccess, (state, { product }): ProductState => ({
      ...state,
      selectedProduct: product,
      isLoading: false,
    })),
    on(ProductActions.loadProductFailure, (state, { error }): ProductState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(ProductActions.createProduct, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ProductActions.createProductSuccess, (state, { product }): ProductState => ({
      ...state,
      products: [...state.products, product],
      isLoading: false,
    })),
    on(ProductActions.createProductFailure, (state, { error }): ProductState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(ProductActions.saveProductEdit, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ProductActions.updateProductSuccess, (state, { product }): ProductState => ({
      ...state,
      products: state.products.map(p => (p.id === product.id ? product : p)),
      selectedProduct:
        state.selectedProduct?.id === product.id ? product : state.selectedProduct,
      isLoading: false,
    })),
    on(ProductActions.updateProductFailure, (state, { error }): ProductState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(ProductActions.removeProductVariantSuccess, (state, { productId, variantId }): ProductState => {
      const removeVariant = (product: Product) =>
        product.id === productId
          ? {
              ...product,
              variants: product.variants.filter(v => v.id !== variantId),
            }
          : product;

      return {
        ...state,
        products: state.products.map(removeVariant),
        selectedProduct:
          state.selectedProduct && removeVariant(state.selectedProduct),
      };
    }),
    on(ProductActions.removeProductVariantFailure, (state, { error }): ProductState => ({
      ...state,
      error,
    })),
    on(ProductActions.disableProduct, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(ProductActions.disableProductSuccess, (state, { id }): ProductState => ({
      ...state,
      products: state.products.map(p => (p.id === id ? { ...p, isActive: false } : p)),
      selectedProduct:
        state.selectedProduct?.id === id
          ? { ...state.selectedProduct, isActive: false }
          : state.selectedProduct,
      isLoading: false,
    })),
    on(ProductActions.disableProductFailure, (state, { error }): ProductState => ({
      ...state,
      isLoading: false,
      error,
    })),
    on(ProductActions.clearSelectedProduct, (state): ProductState => ({
      ...state,
      selectedProduct: null,
    })),
  ),
});
