import {
  cartDiscount,
  cartSubtotal,
  cartTax,
  repriceItems,
} from './cart-totals.util';
import { SaleLineItem } from './sale-line-item.interface';
import { SalePriceListOption } from './sale-price-list-option.interface';

const item = (
  variantId: string,
  unitPrice: number,
  quantity: number,
  stateRate = 0,
): SaleLineItem => ({
  variantId,
  productId: `p-${variantId}`,
  name: variantId,
  variantLabel: '',
  unitPrice,
  quantity,
  origin: '0',
  stateRate,
});

const option = (
  lines: { variantId: string; unitPrice: number }[],
): SalePriceListOption => ({
  priceListId: 'list',
  name: 'A Prazo',
  isDefault: false,
  subtotal: 0,
  lines: lines.map(line => ({ ...line, lineTotal: line.unitPrice })),
});

describe('cart-totals.util', () => {
  it('sums unit price times quantity', () => {
    expect(cartSubtotal([item('a', 10, 2), item('b', 5.5, 1)])).toBe(25.5);
  });

  it('clamps the discount to the subtotal, in both modes', () => {
    expect(cartDiscount(100, 10, 'percent')).toBe(10);
    expect(cartDiscount(100, 150, 'amount')).toBe(100);
    expect(cartDiscount(100, -5, 'amount')).toBe(0);
  });

  it('spreads the discount over lines before applying their tax rate', () => {
    const items = [item('a', 60, 1, 10), item('b', 40, 1, 0)];
    // a: (60 - 10 * 0.6) * 10% = 5.40; b: no tax.
    expect(cartTax(items, 10)).toBe(5.4);
  });

  it('reprices lines from the chosen list and keeps the rest', () => {
    const items = [item('a', 10, 1), item('b', 20, 1)];
    const repriced = repriceItems(items, option([{ variantId: 'a', unitPrice: 12 }]));

    expect(repriced.map(i => i.unitPrice)).toEqual([12, 20]);
    expect(repriceItems(items, null)).toBe(items);
  });
});
