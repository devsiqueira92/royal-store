/** Wire shape of `SalePricingItem`: one cart line to price. */
export interface SalePricingItem {
  variantId: string;
  quantity: number;
}

/** Wire shape of `SalePriceListLineResponse`. */
export interface SalePriceListLine {
  variantId: string;
  unitPrice: number;
  lineTotal: number;
}

/** Wire shape of `SalePriceListOptionResponse`: a list the sale can be charged on, with the cart priced on it. */
export interface SalePriceListOption {
  priceListId: string;
  name: string;
  isDefault: boolean;
  subtotal: number;
  lines: SalePriceListLine[];
}
