import { createFeature, createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Product } from '../interfaces/product.interface';
import { ProductActions } from './product.actions';
import { ServiceActions } from './service.actions';
export interface ProductState extends EntityState<Product> {
  selectedProduct: Product | null;
  selectedLabelProducts: Product[];
  isLoading: boolean;
  error: string | null;
}

export const productAdapter: EntityAdapter<Product> =
  createEntityAdapter<Product>({});

export const initialProductState: ProductState = productAdapter.getInitialState(
  {
    selectedProduct: null,
    selectedLabelProducts: [],
    isLoading: false,
    error: null,
  },
);

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
      (state, { products }): ProductState => {
        return productAdapter.setAll(products, {
          ...state,
          isLoading: false,
        });
      },
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

    on(
      ProductActions.createProduct,
      ServiceActions.createService,
      (state): ProductState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),

    on(
      ProductActions.createProductSuccess,
      ServiceActions.createServiceSuccess,
      (state, { product }): ProductState => {
        return productAdapter.addOne(product, {
          ...state,
          isLoading: false,
        });
      },
    ),

    on(
      ProductActions.createProductFailure,
      ServiceActions.createServiceFailure,
      (state, { error }): ProductState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(
      ProductActions.saveProductEdit,
      ServiceActions.saveServiceEdit,
      (state): ProductState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),

    on(
      ProductActions.updateProductSuccess,
      ServiceActions.updateServiceSuccess,
      (state, { product }): ProductState => {
        const updatedState = productAdapter.updateOne(
          { id: product.id, changes: product },
          state,
        );

        return {
          ...updatedState,
          selectedProduct:
            state.selectedProduct?.id === product.id
              ? product
              : state.selectedProduct,
          isLoading: false,
        };
      },
    ),

    on(
      ProductActions.updateProductFailure,
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
        const updatedState = productAdapter.map(
          p => ({
            ...p,
            variants: p.variants
              ? p.variants.filter(v => v.id !== variantId)
              : [],
          }),
          state,
        );

        let newSelectedProduct: Product | null = null;
        if (state.selectedProduct) {
          newSelectedProduct = {
            ...state.selectedProduct,
            variants: state.selectedProduct.variants
              ? state.selectedProduct.variants.filter(v => v.id !== variantId)
              : [],
          };
        }

        return {
          ...updatedState,
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

    on(ProductActions.disableProductSuccess, (state, { id }) => {
      const updatedState = productAdapter.updateOne(
        { id, changes: { isActive: false } },
        state,
      );
      return {
        ...updatedState,
        selectedProduct: state.selectedProduct
          ? { ...state.selectedProduct, isActive: false }
          : null,
        isLoading: false,
      };
    }),

    on(ProductActions.enableProductSuccess, (state, { id }) => {
      const updatedState = productAdapter.updateOne(
        { id, changes: { isActive: true } },
        state,
      );
      return {
        ...updatedState,
        selectedProduct: state.selectedProduct
          ? { ...state.selectedProduct, isActive: true }
          : null,
        isLoading: false,
      };
    }),

    on(
      ProductActions.disableProductFailure,
      ProductActions.enableProductFailure,
      (state, { error }) => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),

    on(ProductActions.clearSelectedProduct, (state): ProductState => ({
      ...state,
      selectedProduct: null,
    })),

    on(
      ProductActions.setSelectedLabelProducts,
      (state, { products }): ProductState => ({
        ...state,
        selectedLabelProducts: products,
      }),
    ),

    on(
      ProductActions.clearSelectedLabelProducts,
      (state): ProductState => ({
        ...state,
        selectedLabelProducts: [],
      }),
    ),
  ),
});
