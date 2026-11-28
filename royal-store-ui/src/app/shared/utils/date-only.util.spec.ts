import { formatDateOnly, parseDateOnly } from './date-only.util';

describe('parseDateOnly', () => {
  it('builds local midnight from yyyy-MM-dd', () => {
    const date = parseDateOnly('2026-11-05')!;

    expect(date.getFullYear()).toBe(2026);
    expect(date.getMonth()).toBe(10);
    expect(date.getDate()).toBe(5);
    expect(date.getHours()).toBe(0);
  });

  it('ignores time suffix', () => {
    expect(parseDateOnly('2026-01-01T00:00:00')?.getDate()).toBe(1);
  });

  it('returns null for empty or invalid values', () => {
    expect(parseDateOnly(null)).toBeNull();
    expect(parseDateOnly(undefined)).toBeNull();
    expect(parseDateOnly('')).toBeNull();
    expect(parseDateOnly('05/11/2026')).toBeNull();
    expect(parseDateOnly('2026-02-30')).toBeNull();
    expect(parseDateOnly(new Date('invalid'))).toBeNull();
  });

  it('passes Date through', () => {
    const date = new Date(2026, 0, 1);
    expect(parseDateOnly(date)).toBe(date);
  });
});

describe('formatDateOnly', () => {
  it('formats local calendar parts', () => {
    expect(formatDateOnly(new Date(2026, 0, 1, 23, 59))).toBe('2026-01-01');
  });

  it('round-trips with parseDateOnly', () => {
    expect(formatDateOnly(parseDateOnly('2024-02-29'))).toBe('2024-02-29');
  });

  it('passes valid string through, rejects invalid', () => {
    expect(formatDateOnly('2026-11-05')).toBe('2026-11-05');
    expect(formatDateOnly('2026-13-01')).toBeNull();
  });

  it('returns null for empty values', () => {
    expect(formatDateOnly(null)).toBeNull();
    expect(formatDateOnly(undefined)).toBeNull();
  });
});
