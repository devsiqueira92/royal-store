import { PosProduct } from './pos-product.interface';

const normalize = (term: string): string => term.trim().toLowerCase();

/** The POS search rule: part of the name, SKU or barcode, ignoring case. A blank term matches all. */
export function filterPosProducts(
  products: PosProduct[],
  term: string,
): PosProduct[] {
  const text = normalize(term);
  if (!text) {
    return products;
  }

  return products.filter(
    product =>
      product.productName.toLowerCase().includes(text) ||
      product.sku.toLowerCase().includes(text) ||
      !!product.barcode?.toLowerCase().includes(text),
  );
}

/**
 * The one product a search points at, for adding it straight to the cart: an exact barcode match
 * wins (a scanner types the whole code), else the only product the search finds. Null when the
 * search is blank or ambiguous.
 */
export function singlePosMatch(
  products: PosProduct[],
  term: string,
): PosProduct | null {
  const text = normalize(term);
  if (!text) {
    return null;
  }

  const byBarcode = products.filter(
    product => product.barcode?.toLowerCase() === text,
  );
  if (byBarcode.length === 1) {
    return byBarcode[0];
  }

  const matches = filterPosProducts(products, text);
  return matches.length === 1 ? matches[0] : null;
}
