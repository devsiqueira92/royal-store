import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
} from '@angular/core';
import {
  toObservable,
  toSignal,
  takeUntilDestroyed,
} from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SelectButtonModule } from 'primeng/selectbutton';
import { combineLatest, map, startWith } from 'rxjs';
import {
  CardFlag,
  CardPaymentType,
  PaymentDetails,
  PaymentForm,
  PaymentOption,
  PaymentState,
  SplitPaymentForm,
  SplitPaymentMethod,
} from '../../interfaces/payment.interface';
import { PaymentMethod } from '../../interfaces/sale-totals.interface';
import { CardPaymentFormComponent } from '../card-payment-form/card-payment-form.component';
import { SplitPaymentFormComponent } from '../split-payment-form/split-payment-form.component';

const METHOD_OPTIONS: PaymentOption<PaymentMethod>[] = [
  { label: 'sale.current.payment.cash', value: 'cash' },
  { label: 'sale.current.payment.card', value: 'card' },
  { label: 'sale.current.payment.split', value: 'split' },
];

const round = (value: number): number => Math.round(value * 100) / 100;

const sumAmounts = (rows: { amount?: number | null }[]): number =>
  round(rows.reduce((total, row) => total + (row.amount ?? 0), 0));

@Component({
  selector: 'app-payment-panel',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    SelectButtonModule,
    CardPaymentFormComponent,
    SplitPaymentFormComponent,
  ],
  templateUrl: './payment-panel.component.html',
  styleUrl: './payment-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentPanelComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  total = input<number>(0);

  paymentChange = output<PaymentState>();

  protected readonly methodOptions = METHOD_OPTIONS;

  private readonly splitsMatchTotal: ValidatorFn = (
    control: AbstractControl,
  ): ValidationErrors | null => {
    const remaining = round(this.total() - sumAmounts(control.value ?? []));

    if (remaining > 0) {
      return { splitBelowTotal: { remaining } };
    }

    return remaining < 0 ? { splitAboveTotal: { remaining } } : null;
  };

  protected readonly form: PaymentForm = this.fb.group({
    method: this.fb.control<PaymentMethod>('cash'),
    card: this.fb.group({
      flag: this.fb.control<CardFlag | null>(null, Validators.required),
      type: this.fb.control<CardPaymentType>('credit'),
      installments: this.fb.control<number>(1, Validators.required),
    }),
    splits: this.fb.array<SplitPaymentForm>([], this.splitsMatchTotal),
  });

  private readonly total$ = toObservable(this.total);

  private readonly splits$ = this.form.controls.splits.valueChanges.pipe(
    startWith(this.form.controls.splits.value),
  );

  protected readonly remaining = toSignal(
    combineLatest([this.splits$, this.total$]).pipe(
      map(([rows, total]) => round(total - sumAmounts(rows))),
    ),
    { initialValue: 0 },
  );

  protected readonly method = toSignal(
    this.form.controls.method.valueChanges.pipe(
      startWith(this.form.controls.method.value),
    ),
    { initialValue: this.form.controls.method.value },
  );

  ngOnInit(): void {
    const { method, card } = this.form.controls;

    combineLatest([
      method.valueChanges.pipe(startWith(method.value)),
      card.controls.type.valueChanges.pipe(startWith(card.controls.type.value)),
    ])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([selected, cardType]) =>
        this.syncControls(selected, cardType),
      );

    // Split rows are only valid against the current sale total.
    this.total$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.form.controls.splits.updateValueAndValidity());

    this.form.events
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.paymentChange.emit(this.snapshot()));
  }

  protected addSplit(): void {
    this.form.controls.splits.push(this.createSplitRow());
  }

  protected removeSplit(index: number): void {
    this.form.controls.splits.removeAt(index);
  }

  private syncControls(method: PaymentMethod, cardType: CardPaymentType): void {
    const { card, splits } = this.form.controls;
    const options = { emitEvent: false };

    if (method === 'card') {
      card.enable(options);

      if (cardType === 'credit') {
        card.controls.installments.enable(options);
      } else {
        card.controls.installments.setValue(1, options);
        card.controls.installments.disable(options);
      }
    } else {
      card.disable(options);
    }

    if (method === 'split') {
      splits.enable(options);

      if (splits.length === 0) {
        this.addSplit();
      }
    } else {
      splits.clear(options);
      splits.disable(options);
    }
  }

  private createSplitRow(): SplitPaymentForm {
    return this.fb.group({
      method: this.fb.control<SplitPaymentMethod>('cash'),
      amount: this.fb.control<number | null>(null, [
        Validators.required,
        Validators.min(0.01),
      ]),
    });
  }

  private snapshot(): PaymentState {
    const { method, card, splits } = this.form.getRawValue();

    const details: PaymentDetails = {
      method,
      card:
        method === 'card'
          ? {
              flag: card.flag,
              type: card.type,
              installments: card.type === 'credit' ? card.installments : 1,
            }
          : null,
      splits:
        method === 'split'
          ? splits.map(row => ({ method: row.method, amount: row.amount ?? 0 }))
          : [],
    };

    return { valid: this.form.valid, details };
  }
}
