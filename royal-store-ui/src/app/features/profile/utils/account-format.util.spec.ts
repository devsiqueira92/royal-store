import { formatDocument, formatPhone, initialsOf, maskDocument } from './account-format.util';

describe('account-format.util', () => {
  it('formats CPF and CNPJ', () => {
    expect(formatDocument('52312345678')).toBe('523.123.456-78');
    expect(formatDocument('12345678000190')).toBe('12.345.678/0001-90');
  });

  it('masks leading and check digits', () => {
    expect(maskDocument('52312345678')).toBe('***.123.456-**');
    expect(maskDocument('12345678000190')).toBe('**.345.678/0001-**');
    expect(maskDocument('')).toBe('');
  });

  it('formats mobile and landline phones', () => {
    expect(formatPhone('11987654321')).toBe('(11) 98765-4321');
    expect(formatPhone('1134567890')).toBe('(11) 3456-7890');
  });

  it('builds initials from first and last name', () => {
    expect(initialsOf('Marina Albuquerque Costa')).toBe('MC');
    expect(initialsOf('marina')).toBe('M');
    expect(initialsOf(null)).toBe('');
  });
});
