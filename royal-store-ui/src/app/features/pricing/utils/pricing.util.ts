import { IbptTax } from '@app/core/ibpt-tax/interfaces/ibpt-tax.interface';
import {
  ExpenseRecurrence,
  RecurrenceFrequency,
} from '@app/features/expense/interfaces/expense.interface';
import {
  CardFee,
  FixedExpense,
  MethodProfit,
  PricingInput,
  PricingResult,
} from '../interfaces/pricing.interface';

const roundMoney = (value: number): number => Math.round(value * 100) / 100;

const EMPTY_RESULT: Omit<PricingResult, 'error' | 'totalRate'> = {
  price: 0,
  multiplier: 0,
  taxAmount: 0,
  cardAmount: 0,
  fixedExpenseAmount: 0,
  commissionAmount: 0,
  profitAmount: 0,
};

/**
 * Divisor markup: `PV = cost / (1 - Z)`.
 *
 * Tax, card fee, commission, the fixed-expense share and the margin are all
 * percentages of the SALE price, so they cannot be added on top of the cost —
 * that is the multiplier markup, and it always undershoots the margin.
 */
export const calculatePrice = (input: PricingInput): PricingResult => {
  const totalRate =
    input.taxRate +
    input.cardRate +
    input.fixedExpenseRate +
    input.commissionRate +
    input.marginRate;

  // Z >= 1 has no price that satisfies it: the divisor is zero or negative.
  if (totalRate >= 100) {
    return { ...EMPTY_RESULT, error: 'sumTooHigh', totalRate };
  }

  if (!(input.cost > 0)) {
    return { ...EMPTY_RESULT, error: 'costRequired', totalRate };
  }

  const price = roundMoney(input.cost / (1 - totalRate / 100));
  const share = (rate: number): number => roundMoney((price * rate) / 100);

  return {
    error: null,
    totalRate,
    price,
    multiplier: price / input.cost,
    taxAmount: share(input.taxRate),
    cardAmount: share(input.cardRate),
    fixedExpenseAmount: share(input.fixedExpenseRate),
    commissionAmount: share(input.commissionRate),
    profitAmount: share(input.marginRate),
  };
};

/**
 * Average acquirer fee weighted by how much of the sales each method carries.
 * With no shares informed it falls back to the plain mean of the three rates.
 */
export const weightedCardRate = (fees: readonly CardFee[]): number => {
  if (!fees.length) {
    return 0;
  }

  const totalShare = fees.reduce((sum, fee) => sum + fee.share, 0);

  return totalShare > 0
    ? fees.reduce((sum, fee) => sum + fee.rate * fee.share, 0) / totalShare
    : fees.reduce((sum, fee) => sum + fee.rate, 0) / fees.length;
};

/** Net profit at `price` for each method, charging its own fee instead of the average. */
export const profitByMethod = (
  price: number,
  input: PricingInput,
  fees: readonly CardFee[],
): MethodProfit[] =>
  fees.map((fee) => {
    const deductions = input.taxRate + input.fixedExpenseRate + input.commissionRate + fee.rate;
    const profit = roundMoney(price * (1 - deductions / 100) - input.cost);

    return {
      method: fee.method,
      rate: fee.rate,
      profit,
      margin: price > 0 ? (profit / price) * 100 : 0,
    };
  });

/** What the fixed expenses weigh on revenue, in percent. `0` while revenue is unknown. */
export const fixedExpenseRate = (monthlyExpenses: number, monthlyRevenue: number | null): number =>
  monthlyRevenue && monthlyRevenue > 0 ? (monthlyExpenses / monthlyRevenue) * 100 : 0;

const MONTHLY_FACTOR: Record<RecurrenceFrequency, number> = {
  weekly: 52 / 12,
  monthly: 1,
  quarterly: 1 / 3,
  semiannually: 1 / 6,
  yearly: 1 / 12,
};

/** A recurring template as one monthly fixed expense, whatever its frequency. */
export const recurrenceToFixedExpense = (recurrence: ExpenseRecurrence): FixedExpense => ({
  id: recurrence.id,
  description: recurrence.description,
  amount: roundMoney(recurrence.amount * MONTHLY_FACTOR[recurrence.frequency]),
  recurrenceId: recurrence.id,
});

/** Same rule as the API's `SaleItemEnricher`: these origins use the national federal rate. */
const NATIONAL_ORIGINS = new Set(['0', '3', '4', '5']);

/** Approximate tax burden (IBPT) of an item: federal + state + municipal, in percent. */
export const approximateTaxRate = (tax: IbptTax, origin: string | null | undefined): number => {
  const federal = NATIONAL_ORIGINS.has(origin ?? '0') ? tax.nationalRate : tax.importedFederalRate;

  return Math.round((federal + tax.stateRate + tax.municipalRate) * 100) / 100;
};
