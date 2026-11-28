/**
 * Conversion between .NET `DateOnly` (`yyyy-MM-dd`) and JS `Date`.
 *
 * Never `new Date('yyyy-MM-dd')`: parsed as UTC midnight, shows previous day
 * west of UTC. Never `toISOString()`: UTC, shifts day back in evening.
 * Both directions use local calendar parts only.
 */

const DATE_ONLY_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * `'2026-11-05'` → local `Date` at 00:00, ready for `p-datepicker`.
 * Returns `null` for empty or invalid input (e.g. `'2026-02-30'`).
 * A `Date` passes through untouched.
 */
export const parseDateOnly = (value: string | Date | null | undefined): Date | null => {
  if (value instanceof Date) {
    return Number.isNaN(value.getTime()) ? null : value;
  }

  const match = value ? DATE_ONLY_PATTERN.exec(value.slice(0, 10)) : null;
  if (!match) {
    return null;
  }

  const [, year, month, day] = match.map(Number);
  const date = new Date(year, month - 1, day);

  // Rejects overflow: Date rolls '2026-02-30' into March.
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day
    ? date
    : null;
};

/**
 * Local `Date` → `'2026-11-05'`, the shape the API's `DateOnly` binds to.
 * A `yyyy-MM-dd` string passes through untouched.
 */
export const formatDateOnly = (value: Date | string | null | undefined): string | null => {
  if (typeof value === 'string') {
    return parseDateOnly(value) ? value.slice(0, 10) : null;
  }

  if (!value || Number.isNaN(value.getTime())) {
    return null;
  }

  const month = `${value.getMonth() + 1}`.padStart(2, '0');
  const day = `${value.getDate()}`.padStart(2, '0');

  return `${value.getFullYear()}-${month}-${day}`;
};
