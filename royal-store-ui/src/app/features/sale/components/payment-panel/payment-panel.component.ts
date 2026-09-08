import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
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
import { combineLatest, map, startWith } from 'rxjs';
import {
  CardFlag,
  PaymentDetails,
  PaymentForm,
  PaymentState,
  SplitPaymentForm,
} from '../../interfaces/payment.interface';
import {
  SelectablePaymentMethod,
  toSelectableOptions,
} from '../../interfaces/payment-method-option.const';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { PaymentMethodListComponent } from '../payment-method-list/payment-method-list.component';
import { CardPaymentFormComponent } from '../card-payment-form/card-payment-form.component';
import { SplitPaymentFormComponent } from '../split-payment-form/split-payment-form.component';

const round = (value: number): number => Math.round(value * 100) / 100;

const sumAmounts = (rows: { amount?: number | null }[]): number =>
  round(rows.reduce((total, row) => total + (row.amount ?? 0), 0));

@Component({
  selector: 'app-payment-panel',
  imports: [
    ReactiveFormsModule,
    PaymentMethodListComponent,
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
  private readonly paymentMethodFacade = inject(PaymentMethodFacade);

  total = input<number>(0);
  base = input<number>(0);

  paymentChange = output<PaymentState>();

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
    method: this.fb.control<string | null>(null, Validators.required),
    card: this.fb.group({
      flag: this.fb.control<CardFlag | null>(null, Validators.required),
      installments: this.fb.control<number>(1, Validators.required),
    }),
    splits: this.fb.array<SplitPaymentForm>([], this.splitsMatchTotal),
  });

  /** Every active catalog method plus the synthetic split row, in picker order. */
  protected readonly methods = toSignal(
    this.paymentMethodFacade.activePaymentMethods$.pipe(
      map(toSelectableOptions),
    ),
    { initialValue: [] as SelectablePaymentMethod[] },
  );

  /** Real catalog rows only — offered as the per-line choice inside a split. */
  protected readonly splitMethods = computed(() =>
    this.methods().filter(option => !option.isSplit),
  );

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
    this.form.controls.method.valueChanges.pipe(startWith(null)),
    { initialValue: null as string | null },
  );

  protected readonly selectedOption = computed(
    () => this.methods().find(option => option.id === this.method()) ?? null,
  );

  ngOnInit(): void {
    this.paymentMethodFacade.loadPaymentMethods();

    this.form.controls.method.valueChanges
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncControls());

    // Split rows are only valid against the current sale total.
    this.total$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.form.controls.splits.updateValueAndValidity());

    this.form.events
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.paymentChange.emit(this.snapshot()));
  }

  protected selectMethod(id: string): void {
    this.form.controls.method.setValue(id);
  }

  protected addSplit(): void {
    this.form.controls.splits.push(this.createSplitRow());
  }

  protected removeSplit(index: number): void {
    this.form.controls.splits.removeAt(index);
  }

  private syncControls(): void {
    const option = this.selectedOption();
    const { card, splits } = this.form.controls;
    const options = { emitEvent: false };

    if (option?.cardType) {
      card.enable(options);

      if (option.cardType === 'credit') {
        card.controls.installments.enable(options);
      } else {
        card.controls.installments.setValue(1, options);
        card.controls.installments.disable(options);
      }
    } else {
      card.disable(options);
    }

    if (option?.isSplit) {
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
    const defaultMethodId = this.splitMethods()[0]?.id ?? '';

    return this.fb.group({
      method: this.fb.control<string>(defaultMethodId),
      amount: this.fb.control<number | null>(null, [
        Validators.required,
        Validators.min(0.01),
      ]),
    });
  }

  private snapshot(): PaymentState {
    const { method, card, splits } = this.form.getRawValue();
    const option = this.selectedOption();

    const details: PaymentDetails = {
      methodId: method,
      discountRate: option ? option.discount / 100 : 0,
      card: option?.cardType
        ? {
            flag: card.flag,
            type: option.cardType,
            installments: option.cardType === 'credit' ? card.installments : 1,
          }
        : null,
      splits: option?.isSplit
        ? splits.map(row => ({ methodId: row.method, amount: row.amount ?? 0 }))
        : [],
    };

    return { valid: this.form.valid, details };
  }
}
