import { PaymentMethod } from '@app/features/payment-method/interfaces/payment-method.interface';
import {
  buildCheckoutTotals,
  estimateCashbackEarned,
} from './checkout-totals.util';
import { Customer } from './customer.interface';
import { toSelectableOptions } from './payment-method-option.const';

const customer = (cashbackPercentage: number): Customer => ({
  id: 'c1',
  name: 'Ana',
  loyaltyTierId: 't1',
  loyaltyTierName: 'Gold',
  loyaltyTierCashbackPercentage: cashbackPercentage,
});

const method = (paymentMethod: string, paymentType: string): PaymentMethod => ({
  id: paymentType,
  paymentMethod,
  paymentType,
  discount: 0,
  active: true,
});

describe('buildCheckoutTotals', () => {
  it('never discounts the tier cashback from the total', () => {
    const totals = buildCheckoutTotals({
      subtotal: 100,
      saleDiscount: 0,
      tax: 0,
      customer: customer(10),
      cashbackBalance: 0,
      useCashback: false,
      paymentRate: 0,
    });

    expect(totals.loyaltyRate).toBe(0.1);
    expect(totals.loyaltyDiscount).toBe(0);
    expect(totals.total).toBe(100);
  });

  it('caps redeemed cashback at what is still owed', () => {
    const totals = buildCheckoutTotals({
      subtotal: 100,
      saleDiscount: 0,
      tax: 0,
      customer: customer(0),
      cashbackBalance: 500,
      useCashback: true,
      paymentRate: 0.1,
    });

    expect(totals.cashbackApplied).toBe(90);
    expect(totals.total).toBe(0);
  });
});

describe('estimateCashbackEarned', () => {
  it('applies the tier rate over the base net of redeemed cashback', () => {
    const totals = buildCheckoutTotals({
      subtotal: 120,
      saleDiscount: 20,
      tax: 0,
      customer: customer(5),
      cashbackBalance: 10,
      useCashback: true,
      paymentRate: 0,
    });

    expect(estimateCashbackEarned(totals)).toBe(4.5);
  });

  it('is zero without a customer', () => {
    const totals = buildCheckoutTotals({
      subtotal: 100,
      saleDiscount: 0,
      tax: 0,
      customer: null,
      cashbackBalance: 0,
      useCashback: false,
      paymentRate: 0,
    });

    expect(estimateCashbackEarned(totals)).toBe(0);
  });
});

describe('toSelectableOptions', () => {
  it('derives the card type from the fiscal code and hides the cashback method', () => {
    const options = toSelectableOptions([
      method('Cartão de Crédito', '03'),
      method('Cartão de Débito', '04'),
      method('Cartão da Loja', '05'),
      method('Gift Card', '12'),
      method('Cashback / Loyalty Credit', '19'),
    ]);

    expect(options.map(option => option.cardType)).toEqual([
      'credit',
      'debit',
      'store',
      null,
    ]);
  });
});
