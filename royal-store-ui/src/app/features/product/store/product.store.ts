import {
  loadProducts,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
} from './product.effects';
import { createService, saveServiceEdit } from './service.effects';

export const productStore = {
  loadProducts,
  loadProduct,
  createProduct,
  saveProductEdit,
  disableProduct,
  createService,
  saveServiceEdit,
};
