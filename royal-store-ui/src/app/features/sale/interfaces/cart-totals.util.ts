import { DiscountMode } from './sale-totals.interface';
import { SaleLineItem } from './sale-line-item.interface';
import { SalePriceListOption } from './sale-price-list-option.interface';
import { lineItemTaxRate } from './sale-tax.util';

const round = (value: number): number => Math.round(value * 100) / 100;

export function cartSubtotal(items: SaleLineItem[]): number {
  return items.reduce(
    (total, item) => total + item.unitPrice * item.quantity,
    0,
  );
}

/** Manual sale discount as an amount, clamped to [0, subtotal]. */
export function cartDiscount(
  subtotal: number,
  value: number,
  mode: DiscountMode,
): number {
  const raw = mode === 'percent' ? (subtotal * value) / 100 : value;
  return Math.min(Math.max(raw, 0), subtotal);
}

/** IBPT tax over each line net of its share of the sale discount, rounded per line. */
export function cartTax(items: SaleLineItem[], discount: number): number {
  const subtotal = cartSubtotal(items);
  if (subtotal <= 0) {
    return 0;
  }

  const rawTotal = items.reduce((total, item) => {
    const lineSubtotal = item.unitPrice * item.quantity;
    const lineNet = lineSubtotal - discount * (lineSubtotal / subtotal);

    return total + round((lineNet * lineItemTaxRate(item)) / 100);
  }, 0);

  return round(rawTotal);
}

/** The cart with each line's unit price taken from the chosen price list; unchanged when none. */
export function repriceItems(
  items: SaleLineItem[],
  option: SalePriceListOption | null,
): SaleLineItem[] {
  if (!option) {
    return items;
  }

  const prices = new Map(
    option.lines.map(line => [line.variantId, line.unitPrice]),
  );

  return items.map(item => ({
    ...item,
    unitPrice: prices.get(item.variantId) ?? item.unitPrice,
  }));
}
