import { FormControl } from '@angular/forms';

/** A named price table shared by the whole catalog (`PriceLists`). */
export interface PriceList {
  id: string;
  name: string;
  /** Exactly one list is the default: it feeds the POS when no other is chosen. */
  isDefault: boolean;
  /** Inactive lists are hidden from products and checkout. */
  isActive: boolean;
}

/** One sale price of a product on one list. Mirrors the API's `ProductPriceInput`. */
export interface ProductPriceInput {
  priceListId: string;
  salePrice: number;
}

/** One sale price of a product on one list, as the API returns it. */
export interface ProductPrice {
  priceListId: string;
  priceListName: string;
  isDefault: boolean;
  isActive: boolean;
  salePrice: number | null;
  minPrice: number | null;
  discountPercentage: number | null;
  basePriceListId: string | null;
  suggestedPrice: number | null;
  ruleStatus: string;
}

export interface ProductPriceFormControls {
  priceListId: FormControl<string>;
  /** `null` on a non-default list means "falls back to the default list". */
  salePrice: FormControl<number | null>;
}
