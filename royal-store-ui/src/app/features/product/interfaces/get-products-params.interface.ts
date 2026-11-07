import { StockStatus } from './product.interface';

export interface GetProductsParams {
  isActive?: boolean;
  categoryId?: string;
  stockStatus?: StockStatus;
  supplierId?: string;
  search?: string;
}
