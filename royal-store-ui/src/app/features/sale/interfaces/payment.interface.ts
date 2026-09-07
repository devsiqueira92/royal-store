import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { PaymentMethod } from './sale-totals.interface';

export type CardFlag = 'visa' | 'mastercard' | 'elo' | 'amex' | 'hipercard';

export type CardPaymentType = 'credit' | 'debit';

export type SplitPaymentMethod = 'cash' | 'debit' | 'credit' | 'pix';

export interface PaymentOption<T> {
  label: string;
  value: T;
}

export interface CardPaymentDetails {
  flag: CardFlag | null;
  type: CardPaymentType;
  installments: number;
}

export interface SplitPaymentEntry {
  method: SplitPaymentMethod;
  amount: number;
}

export interface PaymentDetails {
  method: PaymentMethod;
  card: CardPaymentDetails | null;
  splits: SplitPaymentEntry[];
}

export interface PaymentState {
  valid: boolean;
  details: PaymentDetails;
}

export type CardPaymentForm = FormGroup<{
  flag: FormControl<CardFlag | null>;
  type: FormControl<CardPaymentType>;
  installments: FormControl<number>;
}>;

export type SplitPaymentForm = FormGroup<{
  method: FormControl<SplitPaymentMethod>;
  amount: FormControl<number | null>;
}>;

export type SplitPaymentFormArray = FormArray<SplitPaymentForm>;

export type PaymentForm = FormGroup<{
  method: FormControl<PaymentMethod>;
  card: CardPaymentForm;
  splits: SplitPaymentFormArray;
}>;
