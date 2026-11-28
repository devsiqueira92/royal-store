/**
 * Display helpers for the account page. Inputs are digits-only strings, the
 * way the API stores documents and phones.
 */

const digitsOf = (value: string | null | undefined): string => (value ?? '').replace(/\D/g, '');

/** `52312345678` → `523.123.456-78`; `12345678000190` → `12.345.678/0001-90`. */
export const formatDocument = (value: string | null | undefined): string => {
  const digits = digitsOf(value);

  if (digits.length === 11) {
    return digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }
  if (digits.length === 14) {
    return digits.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
  }
  return digits;
};

/**
 * Hides the leading and check digits, the LGPD-friendly display used on
 * receipts: `***.123.456-**` (CPF), `**.345.678/0001-**` (CNPJ).
 */
export const maskDocument = (value: string | null | undefined): string => {
  const formatted = formatDocument(value);
  const digits = digitsOf(value);

  if (digits.length === 11) {
    return `***${formatted.slice(3, 12)}**`;
  }
  if (digits.length === 14) {
    return `**${formatted.slice(2, 16)}**`;
  }
  return formatted ? '•'.repeat(formatted.length) : '';
};

/** `11987654321` → `(11) 98765-4321`; 10 digits → `(11) 3456-7890`. */
export const formatPhone = (value: string | null | undefined): string => {
  const digits = digitsOf(value);

  if (digits.length === 11) {
    return digits.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  }
  if (digits.length === 10) {
    return digits.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
  }
  return digits;
};

export const onlyDigits = digitsOf;

/** First and last name initials (`Marina Albuquerque Costa` → `MC`). */
export const initialsOf = (name: string | null | undefined): string => {
  const parts = (name ?? '').trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) {
    return '';
  }

  const first = parts[0][0];
  const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
  return `${first}${last}`.toUpperCase();
};
