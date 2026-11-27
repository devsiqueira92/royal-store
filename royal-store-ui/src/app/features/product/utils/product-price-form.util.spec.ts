import { ProductPrice } from '../interfaces/price-list.interface';
import {
  applyPercentageRule,
  buildProductPriceGroup,
  defaultSalePrice,
  toProductPriceInputs,
} from './product-price-form.util';

const saved = (
  priceListId: string,
  salePrice: number,
  isDefault = false,
  minPrice: number | null = null,
): ProductPrice => ({
  priceListId,
  priceListName: priceListId,
  isDefault,
  isActive: true,
  salePrice,
  minPrice,
  discountPercentage: null,
  basePriceListId: null,
  suggestedPrice: null,
  ruleStatus: 'none',
});

describe('product-price-form.util', () => {
  it('reads the default-list price', () => {
    expect(defaultSalePrice({ prices: [saved('a', 10), saved('d', 20, true)] })).toBe(20);
    expect(defaultSalePrice({ prices: [] })).toBeNull();
    expect(defaultSalePrice(null)).toBeNull();
  });

  it('table closed: sends the simple price on the default list and keeps the other saved prices', () => {
    const inputs = toProductPriceInputs({
      rows: [],
      basePrice: 50,
      defaultPriceListId: 'd',
      saved: [saved('d', 40, true, 30), saved('a', 45)],
    });

    expect(inputs).toEqual([
      { priceListId: 'a', salePrice: 45, minPrice: null },
      { priceListId: 'd', salePrice: 50, minPrice: 30 },
    ]);
  });

  it('table open: skips empty non-default rows but always sends the default row', () => {
    const inputs = toProductPriceInputs({
      rows: [
        buildProductPriceGroup('d', null),
        buildProductPriceGroup('a', null),
        buildProductPriceGroup('b', 12),
      ],
      basePrice: 99,
      defaultPriceListId: 'd',
      saved: [],
    });

    expect(inputs).toEqual([
      { priceListId: 'd', salePrice: null, minPrice: null },
      { priceListId: 'b', salePrice: 12, minPrice: null },
    ]);
  });

  it('drops a saved discount floor that no longer fits under the new price', () => {
    const inputs = toProductPriceInputs({
      rows: [],
      basePrice: 20,
      defaultPriceListId: 'd',
      saved: [saved('d', 40, true, 30)],
    });

    expect(inputs).toEqual([{ priceListId: 'd', salePrice: 20, minPrice: null }]);
  });
});

describe('applyPercentageRule', () => {
  it('takes a negative percentage off the base price', () => {
    expect(applyPercentageRule(100, -5)).toBe(95);
  });

  it('adds a positive percentage on top of the base price', () => {
    expect(applyPercentageRule(220.32, 8)).toBe(237.95);
  });

  it('rounds half away from zero, like the API', () => {
    expect(applyPercentageRule(0.5, 5)).toBe(0.53);
  });

  it('makes the item free at −100%', () => {
    expect(applyPercentageRule(42, -100)).toBe(0);
  });
});
