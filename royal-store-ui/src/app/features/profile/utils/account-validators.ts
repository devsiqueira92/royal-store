import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Named validators for the account forms. Each returns its own error key so
 * `app-form-error` can say what is wrong and how to fix it — the generic
 * `pattern` key only renders "Formato inválido.".
 */

/** Exact digit count (masked inputs emit digits only). Empty passes: pair with `required`. */
export const digitsValidator =
  (lengths: number[], errorKey: string): ValidatorFn =>
  (control: AbstractControl<string | null>): ValidationErrors | null => {
    const value = control.value ?? '';
    if (!value) {
      return null;
    }
    return /^\d+$/.test(value) && lengths.includes(value.length) ? null : { [errorKey]: true };
  };

/** 8–14 digits, or the literal `ISENTO`. */
export const stateRegistrationValidator: ValidatorFn = (
  control: AbstractControl<string | null>,
): ValidationErrors | null => {
  const value = (control.value ?? '').trim();
  if (!value) {
    return null;
  }
  return /^(\d{8,14}|ISENTO)$/i.test(value) ? null : { invalidStateRegistration: true };
};

export interface PasswordRules {
  length: boolean;
  letter: boolean;
  number: boolean;
}

export const passwordRulesOf = (value: string | null | undefined): PasswordRules => ({
  length: (value ?? '').length >= 8,
  letter: /[A-Za-z]/.test(value ?? ''),
  number: /\d/.test(value ?? ''),
});

/** One error for all rules; the live checklist shows which one is missing. */
export const passwordRulesValidator: ValidatorFn = (
  control: AbstractControl<string | null>,
): ValidationErrors | null => {
  if (!control.value) {
    return null;
  }
  const rules = passwordRulesOf(control.value);
  return rules.length && rules.letter && rules.number ? null : { passwordRules: true };
};

/**
 * After a failed submit, move focus to the first invalid field so keyboard and
 * screen-reader users land on the problem instead of the button.
 */
export const focusFirstInvalid = (host: HTMLElement): void => {
  queueMicrotask(() => {
    // Only the control element itself: wrapping <form>s are ng-invalid too.
    const control = host.querySelector<HTMLElement>('[formcontrolname].ng-invalid');
    if (!control) {
      return;
    }
    const target = control.matches('input, textarea')
      ? control
      : control.querySelector<HTMLElement>('input, textarea, [role="combobox"], [tabindex="0"]');
    target?.focus();
  });
};
