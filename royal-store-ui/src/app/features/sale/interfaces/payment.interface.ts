import { FormArray, FormControl, FormGroup } from '@angular/forms';

export type CardFlag = 'visa' | 'mastercard' | 'elo' | 'amex' | 'hipercard';

export type CardPaymentType = 'credit' | 'debit';

export interface PaymentOption<T> {
  label: string;
  value: T;
}

export interface CardPaymentDetails {
  flag: CardFlag | null;
  type: CardPaymentType;
  installments: number;
}

/** `methodId` is a real catalog id — never the synthetic split row's own id. */
export interface SplitPaymentEntry {
  methodId: string;
  amount: number;
}

export interface PaymentDetails {
  /** A real payment-method catalog id, or the synthetic split id — never a fixed enum. */
  methodId: string | null;
  /** The selected method's catalog discount as a 0..1 rate; 0 for split, since it has none. */
  discountRate: number;
  card: CardPaymentDetails | null;
  splits: SplitPaymentEntry[];
}

export interface PaymentState {
  valid: boolean;
  details: PaymentDetails;
}

export type CardPaymentForm = FormGroup<{
  flag: FormControl<CardFlag | null>;
  installments: FormControl<number>;
}>;

export type SplitPaymentForm = FormGroup<{
  method: FormControl<string>;
  amount: FormControl<number | null>;
}>;

export type SplitPaymentFormArray = FormArray<SplitPaymentForm>;

export type PaymentForm = FormGroup<{
  method: FormControl<string | null>;
  card: CardPaymentForm;
  splits: SplitPaymentFormArray;
}>;
