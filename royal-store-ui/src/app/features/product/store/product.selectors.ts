import { createSelector } from '@ngrx/store';
import { productFeature } from './product.reducer';

export const {
  selectProducts,
  selectSelectedProduct,
  selectIsLoading,
  selectError,
} = productFeature;

export const productVariants = createSelector(
  selectSelectedProduct,
  state => state?.variants,
);
