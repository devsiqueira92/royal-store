export interface ProductsSummary {
  stockValue: number;
  totalQuantity: number;
  result: number;
}

export interface GetProductsSummaryParams {
  categoryId?: string;
  supplierId?: string;
}
