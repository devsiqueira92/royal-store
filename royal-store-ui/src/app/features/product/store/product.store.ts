import {
  loadProducts,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
  removeProductVariant,
} from './product.effects';
import { createService, saveServiceEdit } from './service.effects';

export const productStore = {
  loadProducts,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
  removeProductVariant,
  createService,
  saveServiceEdit,
};
