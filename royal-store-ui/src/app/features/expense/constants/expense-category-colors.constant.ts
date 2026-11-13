/**
 * The colours a category can be tagged with.
 *
 * Six series hues plus three neutrals, all as `#rrggbb` — the API caps the
 * column at seven characters and refuses anything that does not start with
 * `#`, so a three-digit shorthand or a CSS name would be rejected.
 *
 * The first six are the chart series colours, which is what makes a category's
 * colour here the same colour it gets in the P&L legend.
 */
export const EXPENSE_CATEGORY_COLORS = [
  '#6D28D9',
  '#0F766E',
  '#B45309',
  '#1D4ED8',
  '#BE185D',
  '#047857',
  '#DC2626',
  '#0E7490',
  '#52525B',
] as const;
