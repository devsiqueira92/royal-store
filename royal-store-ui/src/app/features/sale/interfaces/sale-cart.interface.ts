import { DiscountMode } from './sale-totals.interface';
import { SaleLineItem } from './sale-line-item.interface';

/** The sale being built at the POS. Kept in the store so it survives the trip to checkout. */
export interface SaleCart {
  items: SaleLineItem[];
  /** Manual discount as typed by the cashier, in `discountMode` units. */
  discountValue: number;
  discountMode: DiscountMode;
}

export const EMPTY_SALE_CART: SaleCart = {
  items: [],
  discountValue: 0,
  discountMode: 'amount',
};
