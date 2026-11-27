import { FormControl, FormGroup, Validators } from '@angular/forms';
import {
  ProductPrice,
  ProductPriceFormControls,
  ProductPriceInput,
} from '../interfaces/price-list.interface';
import { Product } from '../interfaces/product.interface';

export const buildProductPriceGroup = (
  priceListId: string,
  salePrice: number | null = null,
): FormGroup<ProductPriceFormControls> =>
  new FormGroup<ProductPriceFormControls>({
    priceListId: new FormControl(priceListId, { nonNullable: true }),
    salePrice: new FormControl(salePrice, [Validators.min(0.01)]),
  });

/** The product's price on the store's default list — the only sale price a product has. */
export const defaultSalePrice = (
  product: Pick<Product, 'prices'> | null | undefined,
): number | null =>
  product?.prices?.find(price => price.isDefault)?.salePrice ?? null;

/** A saved discount floor is kept while it still fits under the new sale price. */
const keptMinPrice = (
  saved: ProductPrice | undefined,
  salePrice: number | null,
): number | null => {
  const min = saved?.minPrice ?? null;
  return min !== null && salePrice !== null && min <= salePrice ? min : null;
};

export interface ProductPricesSubmitInput {
  /** Rows of the price table; empty while the table is closed. */
  rows: readonly FormGroup<ProductPriceFormControls>[];
  /** The simple "price" field: the default-list price while the table is closed. */
  basePrice: number | null;
  defaultPriceListId: string | null;
  /** Prices already saved on the product (edit); empty on create. */
  saved: readonly ProductPrice[];
}

/**
 * The price set sent on save — the API stores sale prices only per price list.
 *
 * Table open: its rows are the set. A non-default list left empty falls back to the default
 * list and is not sent; the default row is always sent, so an empty one is refused by the API
 * with a message naming the list.
 *
 * Table closed: the simple price field is the default-list price, and every other saved price
 * is resubmitted unchanged, so an edit never wipes them.
 */
export const toProductPriceInputs = ({
  rows,
  basePrice,
  defaultPriceListId,
  saved,
}: ProductPricesSubmitInput): ProductPriceInput[] => {
  const savedById = new Map(saved.map(price => [price.priceListId, price]));
  const toInput = (
    priceListId: string,
    salePrice: number | null,
  ): ProductPriceInput => ({
    priceListId,
    salePrice,
    minPrice: keptMinPrice(savedById.get(priceListId), salePrice),
  });

  if (rows.length) {
    return rows
      .map(group => group.getRawValue())
      .filter(
        row =>
          row.salePrice != null || row.priceListId === defaultPriceListId,
      )
      .map(row => toInput(row.priceListId, row.salePrice));
  }

  const others = saved
    .filter(price => !price.isDefault && price.salePrice != null)
    .map(price => toInput(price.priceListId, price.salePrice));

  return defaultPriceListId
    ? [...others, toInput(defaultPriceListId, basePrice)]
    : others;
};
