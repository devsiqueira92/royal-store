import { FormControl, FormGroup } from '@angular/forms';

export type CardFlag = 'visa' | 'mastercard' | 'elo' | 'amex' | 'hipercard';

/** Card methods that ask for a brand; only `credit` is ever split into installments. */
export type CardPaymentType = 'credit' | 'debit' | 'store';

export interface PaymentOption<T> {
  label: string;
  value: T;
}

export interface CardPaymentDetails {
  flag: CardFlag | null;
  type: CardPaymentType;
  installments: number;
}

export interface PaymentDetails {
  /** A real payment-method catalog id — never a fixed enum. */
  methodId: string | null;
  /** The selected method's catalog fiscal payment code. */
  fiscalType: number;
  /** The selected method's catalog discount as a 0..1 rate. */
  discountRate: number;
  card: CardPaymentDetails | null;
  /** Cash handed over by the customer, to work out the change; null when not typed or not cash. */
  cashReceived: number | null;
}

export interface PaymentState {
  valid: boolean;
  details: PaymentDetails;
}

/** One line of the final charge payload — the chosen method, plus a separate line per cashback
 * redemption applied on top of it. */
export interface PaymentEntry {
  methodId: string;
  fiscalType: number;
  amount: number;
  discountRate: number;
  card: CardPaymentDetails | null;
}

export type CardPaymentForm = FormGroup<{
  flag: FormControl<CardFlag | null>;
  installments: FormControl<number>;
}>;

export type PaymentForm = FormGroup<{
  method: FormControl<string | null>;
  card: CardPaymentForm;
  cashReceived: FormControl<number | null>;
}>;
