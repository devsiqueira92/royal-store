export interface LabelProduct {
  id: string;
  name: string;
  price: number;
  barcode: string;
}

export type LabelFieldKey = 'name' | 'price' | 'barcode';

export interface LabelFieldDefinition {
  key: LabelFieldKey;
  labelKey: string;
}

export const LABEL_FIELD_DEFINITIONS: LabelFieldDefinition[] = [
  { key: 'name', labelKey: 'product.labelPrint.field.name' },
  { key: 'price', labelKey: 'product.labelPrint.field.price' },
  { key: 'barcode', labelKey: 'product.labelPrint.field.barcode' },
];
