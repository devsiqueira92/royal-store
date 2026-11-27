import { cashChange, isCashShort, quickCashAmounts } from './cash-change.util';

describe('cashChange', () => {
  it('is the amount above the total, rounded to cents', () => {
    expect(cashChange(50, 37.4)).toBe(12.6);
    expect(cashChange(37.4, 37.4)).toBe(0);
  });

  it('is null when nothing was typed or it falls short', () => {
    expect(cashChange(null, 10)).toBeNull();
    expect(cashChange(9.99, 10)).toBeNull();
  });
});

describe('isCashShort', () => {
  it('ignores sub-cent rounding noise', () => {
    expect(isCashShort(33.33, 33.333)).toBe(false);
    expect(isCashShort(33.32, 33.333)).toBe(true);
    expect(isCashShort(null, 10)).toBe(false);
  });
});

describe('quickCashAmounts', () => {
  it('offers the exact total and the next round amounts', () => {
    expect(quickCashAmounts(37.4)).toEqual([37.4, 40, 50, 100]);
  });

  it('skips round-ups equal to the total and repeats', () => {
    expect(quickCashAmounts(100)).toEqual([100]);
    expect(quickCashAmounts(120)).toEqual([120, 150, 200]);
  });

  it('offers nothing when there is nothing to pay', () => {
    expect(quickCashAmounts(0)).toEqual([]);
  });
});
