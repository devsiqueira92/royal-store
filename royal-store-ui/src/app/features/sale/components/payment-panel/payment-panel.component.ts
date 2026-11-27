import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { map, startWith } from 'rxjs';
import {
  CardFlag,
  PaymentDetails,
  PaymentForm,
  PaymentState,
} from '../../interfaces/payment.interface';
import {
  SelectablePaymentMethod,
  toSelectableOptions,
} from '../../interfaces/payment-method-option.const';
import { isCashShort } from '../../interfaces/cash-change.util';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { PaymentMethodListComponent } from '../payment-method-list/payment-method-list.component';
import { CardPaymentFormComponent } from '../card-payment-form/card-payment-form.component';
import { CashPaymentFormComponent } from '../cash-payment-form/cash-payment-form.component';

@Component({
  selector: 'app-payment-panel',
  imports: [
    ReactiveFormsModule,
    PaymentMethodListComponent,
    CardPaymentFormComponent,
    CashPaymentFormComponent,
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
  /** Accessible name of the method picker, owned by the section heading around it. */
  labelledBy = input<string | null>(null);

  paymentChange = output<PaymentState>();

  protected readonly form: PaymentForm = this.fb.group({
    method: this.fb.control<string | null>(null, Validators.required),
    card: this.fb.group({
      flag: this.fb.control<CardFlag | null>(null, Validators.required),
      installments: this.fb.control<number>(1, Validators.required),
    }),
    // Optional — an exact payment needs no typing — but never less than what is due.
    cashReceived: this.fb.control<number | null>(null, (control: AbstractControl) =>
      this.validateCashReceived(control),
    ),
  });

  /** Every active catalog method, in picker order. */
  protected readonly methods = toSignal(
    this.paymentMethodFacade.activePaymentMethods$.pipe(
      map(toSelectableOptions),
    ),
    { initialValue: [] as SelectablePaymentMethod[] },
  );

  protected readonly method = toSignal(
    this.form.controls.method.valueChanges.pipe(startWith(null)),
    { initialValue: null as string | null },
  );

  protected readonly selectedOption = computed(
    () => this.methods().find(option => option.id === this.method()) ?? null,
  );

  constructor() {
    // The total moves with the chosen method's discount and the cashback toggle, so what was
    // typed is checked again against the new amount due.
    effect(() => {
      this.total();
      this.form.controls.cashReceived.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.paymentMethodFacade.loadPaymentMethods();

    this.form.controls.method.valueChanges
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncControls());

    this.form.events
      .pipe(startWith(null), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.paymentChange.emit(this.snapshot()));
  }

  protected selectMethod(id: string): void {
    this.form.controls.method.setValue(id);
  }

  private syncControls(): void {
    const option = this.selectedOption();
    const { card, cashReceived } = this.form.controls;
    const options = { emitEvent: false };

    if (option?.isCash) {
      cashReceived.enable(options);
    } else {
      cashReceived.reset(null, options);
      cashReceived.disable(options);
    }

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
  }

  private snapshot(): PaymentState {
    const { method, card, cashReceived } = this.form.getRawValue();
    const option = this.selectedOption();

    const details: PaymentDetails = {
      methodId: method,
      fiscalType: option?.fiscalType ?? 0,
      discountRate: option ? option.discount / 100 : 0,
      card: option?.cardType
        ? {
            flag: card.flag,
            type: option.cardType,
            installments: option.cardType === 'credit' ? card.installments : 1,
          }
        : null,
      cashReceived: option?.isCash ? cashReceived : null,
    };

    return { valid: this.form.valid, details };
  }

  private validateCashReceived(control: AbstractControl): ValidationErrors | null {
    return isCashShort(control.value as number | null, this.total())
      ? { cashShort: true }
      : null;
  }
}
