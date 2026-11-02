import { Product } from './product.interface';

export interface LabelProduct {
  id: string;
  name: string;
  price: number;
  barcode: string;
  sku: string;
}

/**
 * One label per variant, since price/barcode/sku are set at the variant
 * level. Products without variants still get a single label using the
 * base price and the product's own SKU, with no barcode.
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

    return product.variants.map(variant => ({
      id: variant.id,
      name: variant.optionName
        ? `${product.name} - ${variant.optionName}`
        : product.name,
      price: variant.price ?? product.basePrice,
      barcode: variant.barcode ?? product.sku,
      sku: variant.sku ?? product.sku,
    }));
  });
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
