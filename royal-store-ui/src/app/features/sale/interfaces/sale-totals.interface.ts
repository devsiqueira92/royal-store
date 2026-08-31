export type DiscountMode = 'amount' | 'percent';

export type PaymentMethod = 'cash' | 'card' | 'split';

export interface SaleTotals {
  subtotal: number;
  discountAmount: number;
  taxRate: number;
  tax: number;
  total: number;
}
