import { Product } from './product.interface';

export interface LabelProduct {
  id: string;
  name: string;
  price: number;
  barcode: string;
  sku: string;
}

/**
 * One label per unit in stock, since each physical item leaving the shelf needs its own
 * sticker. Price/barcode/sku are set at the variant level, so the count comes from
 * `stockQuantity` there too; a variant with none in stock contributes no labels at all.
 * Products without variants (and variants with untracked stock, i.e. `stockQuantity`
 * is null) have no quantity to multiply by, so they fall back to a single label.
 */
export function toLabelProducts(products: Product[]): LabelProduct[] {
  return products.flatMap(product => {
    if (product.variants.length === 0) {
      return [
        {
          id: product.id,
          name: product.name,
          price: product.basePrice,
          barcode: product.sku,
          sku: product.sku,
        },
      ];
    }

    return product.variants.flatMap(variant =>
      repeatForStock(
        {
          id: variant.id,
          name: variant.optionName
            ? `${product.name} - ${variant.optionName}`
            : product.name,
          price: variant.price ?? product.basePrice,
          barcode: variant.barcode ?? product.sku,
          sku: variant.sku ?? product.sku,
        },
        variant.stockQuantity,
      ),
    );
  });
}

function repeatForStock(
  label: LabelProduct,
  stockQuantity: number | null | undefined,
): LabelProduct[] {
  const count =
    stockQuantity == null ? 1 : Math.max(Math.trunc(stockQuantity), 0);

  return Array.from({ length: count }, (_, index) =>
    count > 1 ? { ...label, id: `${label.id}-${index + 1}` } : label,
  );
}

export type LabelFieldKey = 'name' | 'price' | 'barcode' | 'sku';

export interface LabelFieldDefinition {
  key: LabelFieldKey;
  labelKey: string;
}

export const LABEL_FIELD_DEFINITIONS: LabelFieldDefinition[] = [
  { key: 'name', labelKey: 'product.labelPrint.field.name' },
  { key: 'price', labelKey: 'product.labelPrint.field.price' },
  { key: 'barcode', labelKey: 'product.labelPrint.field.barcode' },
  { key: 'sku', labelKey: 'product.labelPrint.field.sku' },
];
