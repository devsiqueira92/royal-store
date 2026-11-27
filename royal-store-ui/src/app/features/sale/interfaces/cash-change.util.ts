const round = (value: number): number => Math.round(value * 100) / 100;

/** Below a cent, a difference is rounding noise, not a shortfall. */
const CENT_TOLERANCE = 0.005;

/** Round-ups offered as one-tap amounts, the way customers usually hand cash over. */
const ROUND_UP_STEPS = [10, 50, 100];

/** Change owed to the customer; null when nothing was typed or it doesn't cover the total. */
export function cashChange(received: number | null, total: number): number | null {
  if (received === null || isCashShort(received, total)) {
    return null;
  }

  return Math.max(round(received - total), 0);
}

export function isCashShort(received: number | null, total: number): boolean {
  return received !== null && received < total - CENT_TOLERANCE;
}

/** The exact total plus the next round amounts above it, without repeats. */
export function quickCashAmounts(total: number): number[] {
  if (total <= 0) {
    return [];
  }

  const exact = round(total);
  const roundUps = ROUND_UP_STEPS.map(step => Math.ceil(exact / step) * step).filter(
    amount => amount > exact,
  );

  return [exact, ...new Set(roundUps)];
}
