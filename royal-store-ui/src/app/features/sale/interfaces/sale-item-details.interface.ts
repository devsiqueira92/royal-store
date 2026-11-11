export interface SaleItemDetails {
  productName: string;
  sku: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  origin: string;
  ncm: string | null;
  cest: string | null;
  cfop: string | null;
  icmsCst: string | null;
  pisCofinsCst: string | null;
  ivaCst: string | null;
  nationalRate: number | null;
  federalRate: number | null;
  importedFederalRate: number | null;
  stateRate: number | null;
  municipalRate: number | null;
}
