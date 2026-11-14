/** The three ways a sale is settled that carry an acquirer fee worth pricing in. */
export type CardFeeMethod = 'debit' | 'credit' | 'pix';

export interface CardFee {
  method: CardFeeMethod;
  /** Fee charged by the acquirer, in percent of the sale. */
  rate: number;
  /** Share of the store's sales settled this way, in percent. Weights the average. */
  share: number;
}

/** One monthly fixed cost of the store (rent, payroll, energy…). */
export interface FixedExpense {
  id: string;
  description: string;
  /** Monthly amount, already normalised when imported from a non-monthly recurrence. */
  amount: number;
  /** Set when the row came from a recurring-expense template, so a re-import replaces it. */
  recurrenceId: string | null;
}

/** Everything the divisor markup needs. Rates are percentages (`12` = 12%). */
export interface PricingInput {
  cost: number;
  taxRate: number;
  cardRate: number;
  fixedExpenseRate: number;
  commissionRate: number;
  marginRate: number;
}

export type PricingError = 'costRequired' | 'sumTooHigh';

export interface PricingResult {
  error: PricingError | null;
  /** Sum of every percentage that comes out of the sale price (`Z` × 100). */
  totalRate: number;
  /** Suggested sale price, `cost / (1 - Z)`. `0` when there is an error. */
  price: number;
  /** `price / cost` — the equivalent multiplier markup, for reference. */
  multiplier: number;
  taxAmount: number;
  cardAmount: number;
  fixedExpenseAmount: number;
  commissionAmount: number;
  profitAmount: number;
}

/** What is really left at a given price when the sale is settled by one method. */
export interface MethodProfit {
  method: CardFeeMethod;
  rate: number;
  profit: number;
  margin: number;
}

/** Store-wide figures kept between visits; the product, its cost and tax are not. */
export interface PricingSettings {
  expenses: FixedExpense[];
  monthlyRevenue: number | null;
  fees: CardFee[];
  commissionRate: number;
  marginRate: number;
  taxRate: number;
}
