import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectModule } from 'primeng/select';
import { map, startWith, switchMap } from 'rxjs';
import {
  CardFlag,
  CardPaymentForm,
  PaymentOption,
} from '../../interfaces/payment.interface';

const FLAG_OPTIONS: PaymentOption<CardFlag>[] = [
  { label: 'Visa', value: 'visa' },
  { label: 'Mastercard', value: 'mastercard' },
  { label: 'Elo', value: 'elo' },
  { label: 'Amex', value: 'amex' },
  { label: 'Hipercard', value: 'hipercard' },
];

const MAX_INSTALLMENTS = 12;

interface InstallmentOption {
  value: number;
  /** What each installment charges, for the cashier to read back to the customer. */
  amount: number;
}

/** Brand (every card method) and installments (credit only) of a card payment. */
@Component({
  selector: 'app-card-payment-form',
  imports: [CurrencyPipe, ReactiveFormsModule, TranslatePipe, SelectModule],
  templateUrl: './card-payment-form.component.html',
  styleUrl: './card-payment-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardPaymentFormComponent {
  form = input.required<CardPaymentForm>();
  /** Debit and store cards are charged in full, so terms only apply to credit. */
  showInstallments = input<boolean>(false);
  /** Amount charged on the card, split across the installments. */
  total = input<number>(0);

  protected readonly flagOptions = FLAG_OPTIONS;

  protected readonly installmentOptions = computed<InstallmentOption[]>(() =>
    Array.from({ length: MAX_INSTALLMENTS }, (_, index) => ({
      value: index + 1,
      amount: Math.round((this.total() / (index + 1)) * 100) / 100,
    })),
  );

  private readonly flagControl$ = toObservable(this.form).pipe(
    switchMap(form =>
      form.controls.flag.events.pipe(
        startWith(null),
        map(() => form.controls.flag),
      ),
    ),
  );

  protected readonly selectedFlag = toSignal(
    this.flagControl$.pipe(map(control => control.value)),
    { initialValue: null as CardFlag | null },
  );

  protected readonly flagInvalid = toSignal(
    this.flagControl$.pipe(map(control => control.touched && control.invalid)),
    { initialValue: false },
  );

  protected selectFlag(flag: CardFlag): void {
    const { flag: control } = this.form().controls;
    control.setValue(flag);
    control.markAsTouched();
  }
}
