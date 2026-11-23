import { PriceList } from '@app/features/price-list/interfaces/price-list.interface';
import {
  LabelCell,
  LabelElement,
  LabelElementAlignment,
} from '../interfaces/label-format.interface';
import { Product } from '../interfaces/product.interface';
import {
  LabelSampleProduct,
  resolveLabelFieldText,
  toLabelSampleProduct,
} from './label-field.util';
import { DEFAULT_BARCODE_VALUE_FONT_PT } from './label-barcode.util';

/** One physical label to print. */
export interface LabelPrintItem {
  /** Unique per copy (variant id + copy number). */
  key: string;
  product: LabelSampleProduct;
}

/** One element of a label, resolved for a product and placed in mm. */
export interface LabelPrintElement {
  type: LabelElement['type'];
  text: string;
  showValue: boolean;
  /** Element width in mm; sizes the barcode's bars. */
  widthMm: number;
  style: Record<string, string>;
}

/** One label slot on a sheet; `elements` is empty for an unused slot. */
export interface LabelPrintCell {
  key: string;
  style: Record<string, string>;
  elements: LabelPrintElement[];
}

const DEFAULT_FONT_SIZE_PT = 9;

const JUSTIFY: Record<LabelElementAlignment, string> = {
  left: 'flex-start',
  center: 'center',
  right: 'flex-end',
};

/** Untracked stock (`null`) prints one label; otherwise one per unit, none when out of stock. */
const copiesFor = (stockQuantity: number | null | undefined): number =>
  stockQuantity == null ? 1 : Math.max(Math.trunc(stockQuantity), 0);

/**
 * One label per unit in stock, per variant (same rule as the previous print screen);
 * a product without variants prints a single label.
 */
export const buildLabelPrintItems = (
  products: readonly Product[],
  priceLists: readonly PriceList[],
): LabelPrintItem[] =>
  products.flatMap((product) => {
    if (product.variants.length === 0) {
      return [{ key: product.id, product: toLabelSampleProduct(product, priceLists) }];
    }
    return product.variants.flatMap((variant) => {
      const data = toLabelSampleProduct(product, priceLists, variant);
      return Array.from({ length: copiesFor(variant.stockQuantity) }, (_, copy) => ({
        key: `${variant.id}-${copy}`,
        product: data,
      }));
    });
  });

/** Splits `items` into sheets of `perPage` labels. */
export const paginateLabelItems = (
  items: readonly LabelPrintItem[],
  perPage: number,
): LabelPrintItem[][] => {
  if (perPage <= 0) {
    return [];
  }
  const pages: LabelPrintItem[][] = [];
  for (let i = 0; i < items.length; i += perPage) {
    pages.push(items.slice(i, i + perPage));
  }
  return pages;
};

/** Template elements filled with `product`'s data, positioned in real millimetres. */
export const toLabelPrintElements = (
  elements: readonly LabelElement[],
  product: LabelSampleProduct,
): LabelPrintElement[] =>
  elements.map((element) => {
    const isBarcode = element.type === 'barcode';
    const alignment = element.alignment ?? (isBarcode ? 'center' : 'left');
    const fontSize =
      element.fontSize ?? (isBarcode ? DEFAULT_BARCODE_VALUE_FONT_PT : DEFAULT_FONT_SIZE_PT);
    return {
      type: element.type,
      text: resolveLabelFieldText(element.field, product, element.showLabel ?? true),
      showValue: !!element.showValue,
      widthMm: element.width,
      style: {
        left: `${element.x}mm`,
        top: `${element.y}mm`,
        width: `${element.width}mm`,
        height: `${element.height}mm`,
        'font-size': `${fontSize}pt`,
        'font-weight': element.fontWeight === 'bold' ? '700' : '400',
        'text-align': alignment,
        // Barcodes stack bars and digits in a column: centre them vertically.
        'justify-content': isBarcode ? 'center' : JUSTIFY[alignment],
      },
    };
  });

/** Position of a label slot on its sheet, in mm. */
export const labelCellStyle = (cell: LabelCell): Record<string, string> => ({
  left: `${cell.x}mm`,
  top: `${cell.y}mm`,
  width: `${cell.width}mm`,
  height: `${cell.height}mm`,
});
