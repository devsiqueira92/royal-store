/** EAN-13 helpers: generate a random valid code and validate the check digit. */

function ean13CheckDigit(first12: string): number {
  let sum = 0;
  for (let i = 0; i < 12; i++) {
    const digit = Number(first12[i]);
    sum += i % 2 === 0 ? digit : digit * 3;
  }
  return (10 - (sum % 10)) % 10;
}

export function generateEan13(prefix = '789'): string {
  let body = prefix.replace(/\D/g, '').slice(0, 12);
  while (body.length < 12) {
    body += Math.floor(Math.random() * 10).toString();
  }
  return body + ean13CheckDigit(body).toString();
}

export function isValidEan13(value: string | null | undefined): boolean {
  if (!value || !/^\d{13}$/.test(value)) {
    return false;
  }
  return Number(value[12]) === ean13CheckDigit(value.slice(0, 12));
}
