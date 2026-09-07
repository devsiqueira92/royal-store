import { createSelector } from '@ngrx/store';
import { productAdapter, productFeature } from './product.reducer';

export const {
  selectProductState,
  selectSelectedProduct,
  selectIsLoading,
  selectError,
} = productFeature;

const { selectAll, selectEntities, selectTotal } =
  productAdapter.getSelectors();

export const selectAllProducts = createSelector(
  selectProductState,
  selectAll, // O adapter se encarrega de transformar o dicionário (entities) de volta em um Array[]
);

export const selectProductEntities = createSelector(
  selectProductState,
  selectEntities, // Retorna um dicionário { id1: {...}, id2: {...} } ótimo para buscas rápidas
);

export const productVariants = createSelector(
  selectSelectedProduct,
  state => state?.variants,
);
