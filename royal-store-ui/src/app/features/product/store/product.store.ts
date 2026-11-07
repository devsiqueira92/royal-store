import {
  loadProducts,
  loadProductsSummary,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
  enableProduct,
  removeProductVariant,
} from './product.effects';
import { createService, saveServiceEdit } from './service.effects';

export const productStore = {
  loadProducts,
  loadProductsSummary,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
  enableProduct,
  removeProductVariant,
  createService,
  saveServiceEdit,
};
