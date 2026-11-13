/**
 * Date and colour helpers for the expenses module.
 *
 * Every date here is handled as a `yyyy-MM-dd` / `yyyy-MM` string and never
 * through `new Date(string)`: the API's values carry no time or zone, and
 * parsing them would shift a bill due on the 1st into the previous month for
 * anyone west of UTC.
 */

/** The competence month (`yyyy-MM`) a day belongs to. */
export const toCompetenceMonth = (date: Date): string =>
  `${date.getFullYear()}-${`${date.getMonth() + 1}`.padStart(2, '0')}`;

/** First day of a competence month, in the `yyyy-MM-dd` the API expects. */
export const firstDayOfMonth = (competenceMonth: string): string => `${competenceMonth}-01`;

/**
 * Last day of a competence month, as `yyyy-MM-dd`. Day 0 of the next month is
 * the last day of this one, which also settles February without a leap rule.
 */
export const lastDayOfMonth = (competenceMonth: string): string => {
  const [year, month] = competenceMonth.split('-').map(Number);
  const last = new Date(year, month, 0);

  return `${competenceMonth}-${`${last.getDate()}`.padStart(2, '0')}`;
};

/** Moves a `yyyy-MM` by whole months, rolling the year over. */
export const shiftCompetenceMonth = (competenceMonth: string, delta: number): string => {
  const [year, month] = competenceMonth.split('-').map(Number);
  const zeroBased = year * 12 + (month - 1) + delta;

  return `${Math.floor(zeroBased / 12)}-${`${(zeroBased % 12) + 1}`.padStart(2, '0')}`;
};

/** `1`–`12`, for looking up a translated month name. */
export const monthNumberOf = (isoDate: string): number => Number(isoDate.split('-')[1]);

export const yearOf = (isoDate: string): string => isoDate.split('-')[0];

/** `5` out of `2026-11-05` — the day, without the leading zero. */
export const dayOf = (isoDate: string): number => Number(isoDate.split('-')[2]);

/**
 * Today as `yyyy-MM-dd`, from the local calendar rather than `toISOString()`,
 * which is UTC and so names the wrong day for part of every evening.
 */
export const todayIso = (): string => {
  const now = new Date();
  return `${toCompetenceMonth(now)}-${`${now.getDate()}`.padStart(2, '0')}`;
};

/**
 * Fallback series colours, used only where a category has no colour of its own.
 *
 * These are the 700 steps of six hues, chosen so that every adjacent pair stays
 * separable under the three common colour-vision deficiencies and each one
 * holds 4.5:1 against a white surface — a category is also always labelled, so
 * colour is never the only thing carrying its identity.
 */
export const EXPENSE_SERIES_COLORS = [
  '#6D28D9',
  '#0F766E',
  '#B45309',
  '#1D4ED8',
  '#BE185D',
  '#047857',
] as const;

export const seriesColor = (color: string | null | undefined, index: number): string =>
  color?.trim() || EXPENSE_SERIES_COLORS[index % EXPENSE_SERIES_COLORS.length];

/**
 * Compact money for chart labels: `16.4k`, `892`. The full value always appears
 * in the table beside the chart, so rounding here loses nothing.
 */
export const compactAmount = (value: number): string =>
  value >= 1000 ? `${(value / 1000).toFixed(1)}k` : `${Math.round(value)}`;
