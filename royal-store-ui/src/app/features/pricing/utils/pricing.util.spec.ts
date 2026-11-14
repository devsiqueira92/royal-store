import { CardFee, PricingInput } from '../interfaces/pricing.interface';
import {
  calculatePrice,
  fixedExpenseRate,
  profitByMethod,
  weightedCardRate,
} from './pricing.util';

const input: PricingInput = {
  cost: 50,
  taxRate: 6,
  cardRate: 3,
  fixedExpenseRate: 15,
  commissionRate: 1,
  marginRate: 25,
};

describe('pricing util', () => {
  it('prices by divisor markup', () => {
    const result = calculatePrice(input);

    expect(result.error).toBeNull();
    expect(result.totalRate).toBe(50);
    expect(result.price).toBe(100);
    expect(result.profitAmount).toBe(25);
    expect(result.multiplier).toBe(2);
  });

  it('rejects a sum of rates at or above 100%', () => {
    expect(calculatePrice({ ...input, marginRate: 75 }).error).toBe('sumTooHigh');
    expect(calculatePrice({ ...input, marginRate: 90 }).error).toBe('sumTooHigh');
  });

  it('asks for a cost before pricing', () => {
    expect(calculatePrice({ ...input, cost: 0 }).error).toBe('costRequired');
  });

  it('weights the card fee by sales share and falls back to the mean', () => {
    const fees: CardFee[] = [
      { method: 'debit', rate: 2, share: 50 },
      { method: 'credit', rate: 4, share: 50 },
      { method: 'pix', rate: 0, share: 0 },
    ];

    expect(weightedCardRate(fees)).toBe(3);
    expect(weightedCardRate(fees.map((fee) => ({ ...fee, share: 0 })))).toBe(2);
  });

  it('charges each method its own fee', () => {
    const [pix] = profitByMethod(100, input, [{ method: 'pix', rate: 0, share: 100 }]);

    expect(pix.profit).toBe(28);
    expect(pix.margin).toBe(28);
  });

  it('rates fixed expenses over revenue', () => {
    expect(fixedExpenseRate(3000, 20000)).toBe(15);
    expect(fixedExpenseRate(3000, null)).toBe(0);
  });
});
