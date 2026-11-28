import { PosProduct } from './pos-product.interface';
import { filterPosProducts, singlePosMatch } from './pos-product-search.util';

const product = (
  productName: string,
  sku: string,
  barcode: string | null,
): PosProduct =>
  ({ variantId: sku, productName, sku, barcode }) as PosProduct;

const ring = product('Anel Prata', 'ANL-01', '7890000000017');
const ringGold = product('Anel Ouro', 'ANL-02', '7890000000024');
const necklace = product('Colar', 'COL-01', '78900000000');

describe('filterPosProducts', () => {
  it('matches part of the name, SKU or barcode, ignoring case', () => {
    expect(filterPosProducts([ring, ringGold, necklace], 'anel')).toEqual([
      ring,
      ringGold,
    ]);
    expect(filterPosProducts([ring, ringGold, necklace], 'col-')).toEqual([
      necklace,
    ]);
  });

  it('keeps everything for a blank term', () => {
    expect(filterPosProducts([ring, necklace], '  ')).toEqual([ring, necklace]);
  });
});

describe('singlePosMatch', () => {
  it('returns the only product the search finds', () => {
    expect(singlePosMatch([ring, ringGold, necklace], 'prata')).toBe(ring);
  });

  it('is null when the search is ambiguous or blank', () => {
    expect(singlePosMatch([ring, ringGold], 'anel')).toBeNull();
    expect(singlePosMatch([ring], ' ')).toBeNull();
  });

  it('prefers an exact barcode over partial matches', () => {
    // "78900000000" is also part of the other two barcodes.
    expect(singlePosMatch([ring, ringGold, necklace], '78900000000')).toBe(
      necklace,
    );
  });
});
