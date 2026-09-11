export interface PosProduct {
  productId: string;
  variantId: string;
  productName: string;
  category: string;
  categoryColor?: string;
  variantColor?: string;
  barcode?: string;
  price: number;
  coverImage?: string;
  variantSize?: string;
  isStockTracked: boolean;
  stockQuantity: number;
  soldOut: boolean;

  // Fiscal / IBPT tax data
  origin: number;
  nationalRate?: number | null;
  federalRate?: number | null;
  importedFederalRate?: number | null;
  stateRate?: number | null;
  municipalRate?: number | null;
  effectiveStart?: string | null;
  effectiveEnd?: string | null;
  source?: string | null;
  key?: string | null;
}

export interface SaleDetails {
  total: number;
  discount: number;
  discountMode: string;
  paymentMethod: string;
  cardFlag?: string | null;
  cardPaymentType?: string;
  installments?: number;
}
