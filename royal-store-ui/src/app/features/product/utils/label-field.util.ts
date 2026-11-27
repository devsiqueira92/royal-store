import { PriceList } from '@app/features/price-list/interfaces/price-list.interface';
import { LabelElement, LabelElementType } from '../interfaces/label-format.interface';
import { Product, ProductVariant } from '../interfaces/product.interface';

/** 1 pt = 0.3528 mm. */
export const MM_PER_PT = 0.3528;
export const MIN_FONT_SIZE_PT = 4;
export const MAX_FONT_SIZE_PT = 72;
export const MIN_ELEMENT_WIDTH_MM = 1;
export const MIN_ELEMENT_HEIGHT_MM = 1;

/** Editable geometry/typography of one element. */
export type LabelElementPatch = Partial<
  Pick<
    LabelElement,
    | 'type'
    | 'x'
    | 'y'
    | 'width'
    | 'height'
    | 'fontSize'
    | 'fontWeight'
    | 'alignment'
    | 'format'
    | 'showValue'
    | 'showLabel'
  >
>;

/** Fields whose value can be drawn either as text or as a barcode. */
const CODE_FIELDS: ReadonlySet<string> = new Set(['product.sku', 'product.barcode']);

export const canSwitchToBarcode = (field: string): boolean => CODE_FIELDS.has(field);

const BARCODE_MIN_HEIGHT_MM = 8;
const TEXT_DEFAULT_FONT_SIZE_PT = 9;

/**
 * Patch that turns an element into a CODE128 barcode or back into text.
 * Keys that do not apply to the new type are cleared (`undefined`, dropped on save);
 * a barcode gets enough height to be scannable, within the label.
 */
export const switchElementType = (
  element: LabelElement,
  type: LabelElement['type'],
  labelHeight: number,
): LabelElementPatch => {
  const maxHeight = Math.max(labelHeight - element.y, element.height);

  if (type === 'barcode') {
    return {
      type,
      format: 'CODE128',
      // Bars only until the user asks for the digits.
      showValue: false,
      fontSize: undefined,
      fontWeight: undefined,
      alignment: undefined,
      height: Math.min(Math.max(element.height, BARCODE_MIN_HEIGHT_MM), maxHeight),
    };
  }

  return {
    type,
    format: undefined,
    showValue: undefined,
    fontSize: TEXT_DEFAULT_FONT_SIZE_PT,
    fontWeight: 'normal',
    alignment: 'center',
    height: Math.min(
      Math.max(element.height, minHeightForFont(TEXT_DEFAULT_FONT_SIZE_PT)),
      maxHeight,
    ),
  };
};

export interface LabelElementChange {
  index: number;
  changes: LabelElementPatch;
}

/** Smallest height (mm, rounded up to 0.5) that fits one line of `fontSize` pt text. */
export const minHeightForFont = (fontSize: number): number =>
  Math.ceil(fontSize * MM_PER_PT * 1.2 * 2) / 2;

/** Prefix of a price-list field: `product.prices.{priceListId}`. */
export const PRICE_FIELD_PREFIX = 'product.prices.';

/** Legacy field from the stored templates; resolves to the default price list. */
const DEFAULT_PRICE_FIELD = 'product.price';

/** A product field the user can drop on the label. */
export interface LabelFieldOption {
  field: string;
  type: LabelElementType;
  labelKey: string;
  labelParams?: Record<string, string>;
  group: 'product' | 'price';
}

export interface LabelSamplePrice {
  priceListId: string;
  priceListName: string;
  isDefault: boolean;
  value: number | null;
}

/** The product data the editor previews the label with. */
export interface LabelSampleProduct {
  name: string;
  description: string;
  sku: string;
  barcode: string;
  prices: LabelSamplePrice[];
}

const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

export const buildLabelFieldOptions = (priceLists: readonly PriceList[]): LabelFieldOption[] => [
  {
    field: 'product.name',
    type: 'text',
    labelKey: 'product.labelDesigner.editor.fields.name',
    group: 'product',
  },
  {
    field: 'product.description',
    type: 'text',
    labelKey: 'product.labelDesigner.editor.fields.description',
    group: 'product',
  },
  {
    field: 'product.sku',
    type: 'text',
    labelKey: 'product.labelDesigner.editor.fields.sku',
    group: 'product',
  },
  {
    field: 'product.barcode',
    type: 'barcode',
    labelKey: 'product.labelDesigner.editor.fields.barcode',
    group: 'product',
  },
  ...priceLists
    .filter((list) => list.isActive)
    .map<LabelFieldOption>((list) => ({
      field: `${PRICE_FIELD_PREFIX}${list.id}`,
      type: 'text',
      labelKey: 'product.labelDesigner.editor.fields.price',
      labelParams: { name: list.name },
      group: 'price',
    })),
];

/**
 * Turns a product (one of its variants; the first by default) into label data.
 * Variant-level sku/barcode win over the product's, same as `toLabelProducts`.
 */
export const toLabelSampleProduct = (
  product: Product,
  priceLists: readonly PriceList[],
  variant: ProductVariant | undefined = product.variants[0],
): LabelSampleProduct => {
  const byList = new Map((product.prices ?? []).map((p) => [p.priceListId, p] as const));

  return {
    name: variant?.optionName ? `${product.name} - ${variant.optionName}` : product.name,
    description: product.description ?? '',
    sku: variant?.sku ?? product.sku,
    barcode: variant?.barcode ?? product.sku,
    prices: priceLists.map((list) => {
      const price = byList.get(list.id);
      return {
        priceListId: list.id,
        priceListName: list.name,
        isDefault: list.isDefault,
        value: price?.salePrice ?? price?.suggestedPrice ?? null,
      };
    }),
  };
};

/** Used while no product was picked on the products page. */
export const buildMockLabelSampleProduct = (
  priceLists: readonly PriceList[],
): LabelSampleProduct => ({
  name: 'Camiseta Básica Branca P',
  description: '100% algodão, gola careca',
  sku: 'SKU-00001',
  barcode: '7891000000017',
  prices: priceLists.map((list, index) => ({
    priceListId: list.id,
    priceListName: list.name,
    isDefault: list.isDefault,
    value: Math.round(49.9 * (1 - index * 0.05) * 100) / 100,
  })),
});

const findPrice = (field: string, product: LabelSampleProduct): LabelSamplePrice | undefined =>
  field === DEFAULT_PRICE_FIELD
    ? product.prices.find((p) => p.isDefault)
    : product.prices.find((p) => p.priceListId === field.slice(PRICE_FIELD_PREFIX.length));

/** The text an element shows for the sample product. Prices read "List name: R$ 0,00". */
/**
 * Rewrites the legacy `product.price` field to the default price list's own field
 * (`product.prices.{id}`), so a stored template matches the editor's price toggles.
 * Left untouched until the price lists are known.
 */
export const normalizeLabelField = (field: string, priceLists: readonly PriceList[]): string => {
  if (field !== DEFAULT_PRICE_FIELD) {
    return field;
  }
  const defaultList = priceLists.find((list) => list.isDefault);
  return defaultList ? `${PRICE_FIELD_PREFIX}${defaultList.id}` : field;
};

/** Same array back when nothing changed, so signals holding it are not re-notified. */
export const normalizeLabelElements = (
  elements: readonly LabelElement[],
  priceLists: readonly PriceList[],
): LabelElement[] => {
  let changed = false;
  const normalized = elements.map((element) => {
    const field = normalizeLabelField(element.field, priceLists);
    if (field === element.field) {
      return element;
    }
    changed = true;
    return { ...element, field };
  });
  return changed ? normalized : (elements as LabelElement[]);
};

/** Price fields: `field` of a price list (or the legacy default-price field). */
export const isPriceField = (field: string): boolean =>
  field === DEFAULT_PRICE_FIELD || field.startsWith(PRICE_FIELD_PREFIX);

/**
 * The text an element shows for `product`. Prices read "List name: R$ 0,00",
 * or just "R$ 0,00" with `showLabel` false.
 */
export const resolveLabelFieldText = (
  field: string,
  product: LabelSampleProduct,
  showLabel = true,
): string => {
  switch (field) {
    case 'product.name':
      return product.name;
    case 'product.description':
      return product.description;
    case 'product.sku':
      return product.sku;
    case 'product.barcode':
      return product.barcode;
  }

  if (isPriceField(field)) {
    const price = findPrice(field, product);
    if (!price) {
      return '';
    }
    const value = price.value == null ? '—' : currency.format(price.value);
    return showLabel ? `${price.priceListName}: ${value}` : value;
  }

  return field;
};

const ELEMENT_DEFAULTS: Record<
  LabelFieldOption['group'] | 'barcode',
  Pick<LabelElement, 'height' | 'fontSize' | 'fontWeight'>
> = {
  product: { height: 5, fontSize: 9, fontWeight: 'normal' },
  price: { height: 6, fontSize: 12, fontWeight: 'bold' },
  barcode: { height: 8 },
};

/** A new element for `option`, stacked below the existing ones and kept inside the label. */
export const createLabelElement = (
  option: LabelFieldOption,
  existing: readonly LabelElement[],
  labelWidth: number,
  labelHeight: number,
): LabelElement => {
  const margin = Math.min(2, labelWidth / 10);
  const defaults = ELEMENT_DEFAULTS[option.type === 'barcode' ? 'barcode' : option.group];
  const height = Math.min(defaults.height, labelHeight);
  const nextY = existing.reduce((max, e) => Math.max(max, e.y + e.height + 1), margin);
  const y = Math.max(Math.min(nextY, labelHeight - height), 0);
  const base = {
    field: option.field,
    x: margin,
    y,
    width: Math.max(labelWidth - margin * 2, 1),
    height,
  };

  return option.type === 'barcode'
    ? { ...base, type: 'barcode', format: 'CODE128', showValue: false }
    : {
        ...base,
        type: 'text',
        fontSize: defaults.fontSize,
        fontWeight: defaults.fontWeight,
        alignment: 'center',
        ...(option.group === 'price' ? { format: 'currency', showLabel: true } : {}),
      };
};
