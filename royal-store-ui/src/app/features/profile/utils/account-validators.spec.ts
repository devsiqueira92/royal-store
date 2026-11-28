import { FormControl, ValidatorFn } from '@angular/forms';
import {
  digitsValidator,
  passwordRulesOf,
  passwordRulesValidator,
  stateRegistrationValidator,
} from './account-validators';

const run = (validator: ValidatorFn, value: string | null) =>
  new FormControl<string | null>(value, validator).errors;

describe('account-validators', () => {
  it('digitsValidator accepts listed lengths and names its error', () => {
    const phone = digitsValidator([10, 11], 'invalidPhone');
    expect(run(phone, '11987654321')).toBeNull();
    expect(run(phone, '1134567890')).toBeNull();
    expect(run(phone, '119876')).toEqual({ invalidPhone: true });
    expect(run(phone, null)).toBeNull();
  });

  it('stateRegistrationValidator allows digits or ISENTO', () => {
    expect(run(stateRegistrationValidator, '110042490114')).toBeNull();
    expect(run(stateRegistrationValidator, 'isento')).toBeNull();
    expect(run(stateRegistrationValidator, '12-34')).toEqual({ invalidStateRegistration: true });
  });

  it('passwordRules reports each rule and fails until all pass', () => {
    expect(passwordRulesOf('abc')).toEqual({ length: false, letter: true, number: false });
    expect(run(passwordRulesValidator, 'abcdefgh')).toEqual({ passwordRules: true });
    expect(run(passwordRulesValidator, 'abcdefg1')).toBeNull();
  });
});
