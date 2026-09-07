import { createFeature, createReducer, on } from '@ngrx/store';
import { Product } from '../interfaces/product.interface';
import { ProductActions } from './product.actions';
import { ServiceActions } from './service.actions';

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
    on(
      ProductActions.loadProductsSuccess,
      (state, { products }): ProductState => ({
        ...state,
        products,
        isLoading: false,
      }),
    ),
    on(
      ProductActions.loadProductsFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(ProductActions.loadProduct, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),

    on(
      ProductActions.loadProductSuccess,
      (state, { product }): ProductState => ({
        ...state,
        selectedProduct: product,
        isLoading: false,
      }),
    ),

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

    on(
      ProductActions.createProductSuccess,
      (state, { product }): ProductState => ({
        ...state,
        products: [...state.products, product],
        isLoading: false,
      }),
    ),

    on(
      ProductActions.createProductFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(ProductActions.saveProductEdit, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),

    on(
      ProductActions.updateProductSuccess,
      (state, { product }): ProductState => ({
        ...state,
        products: state.products.map(p => (p.id === product.id ? product : p)),
        selectedProduct:
          state.selectedProduct?.id === product.id
            ? product
            : state.selectedProduct,
        isLoading: false,
      }),
    ),
    on(
      ProductActions.updateProductFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(ServiceActions.createService, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),

    on(
      ServiceActions.createServiceSuccess,
      (state, { product }): ProductState => ({
        ...state,
        products: [...state.products, product],
        isLoading: false,
      }),
    ),
    on(
      ServiceActions.createServiceFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(ServiceActions.saveServiceEdit, (state): ProductState => ({
      ...state,
      isLoading: true,
      error: null,
    })),

    on(
      ServiceActions.updateServiceSuccess,
      (state, { product }): ProductState => ({
        ...state,
        products: state.products.map(p => (p.id === product.id ? product : p)),
        selectedProduct:
          state.selectedProduct?.id === product.id
            ? product
            : state.selectedProduct,
        isLoading: false,
      }),
    ),
    on(
      ServiceActions.updateServiceFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      ProductActions.removeProductVariantSuccess,
      (state, { variantId }): ProductState => {
        const products = state.products.map(p => ({
          ...p,
          variants: p.variants.filter(v => v.id !== variantId),
        }));

        const variants = state.selectedProduct?.variants.filter(
          p => p.id !== variantId,
        );

        const newSelectedProduct: Product | null = {
          ...state.selectedProduct!,
          variants: variants!,
        };

        return {
          ...state,
          products: products,
          selectedProduct: newSelectedProduct,
          error: null,
        };
      },
    ),
    on(
      ProductActions.removeProductVariantFailure,
      (state, { error }): ProductState => ({
        ...state,
        error,
      }),
    ),

    on(ProductActions.disableProduct, (state, { id }) => ({
      ...state,
      isLoading: false,
      error: null,
    })),

    on(ProductActions.disableProductSuccess, (state, { id }) => ({
      ...state,
      selectedProduct: { ...state.selectedProduct!, isActive: false },
      isLoading: false,
    })),

    on(ProductActions.disableProductFailure, (state, { id, error }) => ({
      ...state,
      isLoading: false,
      error: error,
    })),

    on(ProductActions.enableProduct, (state, { id }) => ({
      ...state,
      isLoading: false,
      error: null,
    })),

    on(ProductActions.enableProductSuccess, (state, { id }) => ({
      ...state,
      selectedProduct: { ...state.selectedProduct!, isActive: true },
      isLoading: false,
    })),

    on(ProductActions.enableProductFailure, (state, { id, error }) => ({
      ...state,
      isLoading: false,
      error: error,
    })),

    on(ProductActions.clearSelectedProduct, (state): ProductState => ({
      ...state,
      selectedProduct: null,
    })),
  ),
});
