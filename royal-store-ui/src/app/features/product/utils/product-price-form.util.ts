import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ProductPriceFormControls, ProductPriceInput } from '../interfaces/price-list.interface';

export const buildProductPriceGroup = (
  priceListId: string,
  salePrice: number | null = null,
): FormGroup<ProductPriceFormControls> =>
  new FormGroup<ProductPriceFormControls>({
    priceListId: new FormControl(priceListId, { nonNullable: true }),
    salePrice: new FormControl(salePrice, [Validators.min(0.01)]),
  });

/** The rows worth sending: a list left without a sale price falls back to the default. */
export const toProductPriceInputs = (
  groups: readonly FormGroup<ProductPriceFormControls>[],
): ProductPriceInput[] =>
  groups
    .map((group) => group.getRawValue())
    .filter((row): row is ProductPriceInput => row.salePrice != null);
