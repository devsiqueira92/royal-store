import {
  AbstractControl,
  FormControl,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import {
  ProductPriceFormControls,
  ProductPriceInput,
} from '../interfaces/price-list.interface';

/** A discount floor above the sale price would block every sale on that list. */
const minPriceWithinSalePrice = (group: AbstractControl): ValidationErrors | null => {
  const { salePrice, minPrice } = (group as FormGroup<ProductPriceFormControls>).getRawValue();

  return salePrice != null && minPrice != null && minPrice > salePrice
    ? { minAboveSale: true }
    : null;
};

export const buildProductPriceGroup = (
  priceListId: string,
  salePrice: number | null = null,
  minPrice: number | null = null,
): FormGroup<ProductPriceFormControls> =>
  new FormGroup<ProductPriceFormControls>(
    {
      priceListId: new FormControl(priceListId, { nonNullable: true }),
      salePrice: new FormControl(salePrice, [Validators.min(0.01)]),
      minPrice: new FormControl(minPrice, [Validators.min(0)]),
    },
    { validators: minPriceWithinSalePrice },
  );

/** The rows worth sending: a list left without a sale price falls back to the default. */
export const toProductPriceInputs = (
  groups: readonly FormGroup<ProductPriceFormControls>[],
): ProductPriceInput[] =>
  groups
    .map((group) => group.getRawValue())
    .filter((row): row is ProductPriceInput => row.salePrice != null);
