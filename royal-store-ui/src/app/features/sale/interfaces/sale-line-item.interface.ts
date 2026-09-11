export interface SaleLineItem {
  variantId: string;
  productId: string;
  name: string;
  variantLabel: string;
  variantSize?: string;
  variantColor?: string;
  unitPrice: number;
  quantity: number;
  coverPhotoUrl?: string;

  // Fiscal / IBPT tax data carried from the catalog
  /** NFe origin code as the catalog stores it — a string ("0" = national), never a number. */
  origin: string;
  nationalRate?: number | null;
  federalRate?: number | null;
  importedFederalRate?: number | null;
  stateRate?: number | null;
  municipalRate?: number | null;
}
