import {
  AbstractControl,
  FormControl,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import {
  ProductCostItem,
  ProductCostItemFormControls,
} from '../interfaces/product.interface';
import { ProductCostItemInput } from '../interfaces/create-product-request.interface';

export const COST_ITEM_DESCRIPTION_MAX_LENGTH = 80;

const isBlank = (value: string | null | undefined): boolean =>
  !value?.trim();

/** A row left completely empty is ignored on save; one with a value needs a description. */
const isEmptyRow = (row: {
  description: string | null;
  value: number | null;
}): boolean => isBlank(row.description) && row.value == null;

const descriptionRequiredWhenValued = (
  control: AbstractControl,
): ValidationErrors | null => {
  const group = control as FormGroup<ProductCostItemFormControls>;
  const { description, value } = group.getRawValue();

  return value != null && isBlank(description)
    ? { descriptionRequired: true }
    : null;
};

export const buildProductCostItemGroup = (
  item?: ProductCostItem,
): FormGroup<ProductCostItemFormControls> =>
  new FormGroup<ProductCostItemFormControls>(
    {
      id: new FormControl(item?.id ?? null),
      description: new FormControl(item?.description ?? null, [
        Validators.maxLength(COST_ITEM_DESCRIPTION_MAX_LENGTH),
      ]),
      value: new FormControl(item?.value ?? null, [Validators.min(0)]),
    },
    { validators: descriptionRequiredWhenValued },
  );

/** Sum of the lines' values, rounded to cents. */
export const sumProductCostItems = (
  groups: readonly FormGroup<ProductCostItemFormControls>[],
): number =>
  Math.round(
    groups.reduce((sum, group) => sum + (group.controls.value.value ?? 0), 0) *
      100,
  ) / 100;

/** The rows worth sending, in screen order; values rounded to cents. */
export const toProductCostItemInputs = (
  groups: readonly FormGroup<ProductCostItemFormControls>[],
): ProductCostItemInput[] =>
  groups
    .map(group => group.getRawValue())
    .filter(row => !isEmptyRow(row))
    .map(row => ({
      id: row.id,
      description: (row.description ?? '').trim(),
      value: Math.round((row.value ?? 0) * 100) / 100,
    }));
