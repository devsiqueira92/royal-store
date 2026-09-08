export type DiscountMode = 'amount' | 'percent';

export interface SaleTotals {
  subtotal: number;
  discountAmount: number;
  taxRate: number;
  tax: number;
  total: number;
}
