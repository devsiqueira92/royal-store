import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
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

// Missing keys fall through untranslated, so "2x" .. "12x" render as-is.
const INSTALLMENT_OPTIONS: PaymentOption<number>[] = Array.from(
  { length: 12 },
  (_, index) => ({
    label: index === 0 ? 'sale.current.payment.singleCharge' : `${index + 1}x`,
    value: index + 1,
  }),
);

@Component({
  selector: 'app-card-payment-form',
  imports: [ReactiveFormsModule, TranslatePipe, SelectModule],
  templateUrl: './card-payment-form.component.html',
  styleUrl: './card-payment-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardPaymentFormComponent {
  form = input.required<CardPaymentForm>();
  /** Debit cards are charged in full, so terms only apply to credit. */
  showInstallments = input<boolean>(false);

  protected readonly flagOptions = FLAG_OPTIONS;
  protected readonly installmentOptions = INSTALLMENT_OPTIONS;

  private readonly flagStatus$ = toObservable(this.form).pipe(
    switchMap(form =>
      form.controls.flag.events.pipe(
        startWith(null),
        map(() => form.controls.flag),
      ),
    ),
  );

  protected readonly flagInvalid = toSignal(
    this.flagStatus$.pipe(map(control => control.touched && control.invalid)),
    { initialValue: false },
  );
}
