import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import {
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  startWith,
} from 'rxjs';
import {
  CheckoutResult,
  FulfillmentMode,
} from '../../interfaces/checkout.interface';
import { buildCheckoutTotals } from '../../interfaces/checkout-totals.util';
import { Customer } from '../../interfaces/customer.interface';
import { PaymentState } from '../../interfaces/payment.interface';
import { CustomerService } from '../../services/customer.service';
import { StoreBranchService } from '../../services/store-branch.service';
import {
  CheckoutStep,
  CheckoutStepsComponent,
} from '../checkout-steps/checkout-steps.component';
import { CheckoutSummaryComponent } from '../checkout-summary/checkout-summary.component';
import { CustomerStepComponent } from '../customer-step/customer-step.component';
import { PaymentStepComponent } from '../payment-step/payment-step.component';
import { FulfillmentStepComponent } from '../fulfillment-step/fulfillment-step.component';

const STEPS: CheckoutStep[] = [
  {
    index: 1,
    label: 'sale.checkout.steps.customer',
    title: 'sale.checkout.customer.title',
  },
  {
    index: 2,
    label: 'sale.checkout.steps.payment',
    title: 'sale.checkout.payment.title',
  },
  {
    index: 3,
    label: 'sale.checkout.steps.fulfillment',
    title: 'sale.checkout.fulfillment.title',
  },
];

const onlyDigits = (value: string): string => value.replace(/\D/g, '');

function matches(customer: Customer, term: string): boolean {
  const text = term.trim().toLowerCase();
  if (!text) {
    return true;
  }

  const digits = onlyDigits(text);

  return (
    customer.name.toLowerCase().includes(text) ||
    (!!digits && customer.phone.includes(digits)) ||
    (!!digits && customer.document.includes(digits))
  );
}

@Component({
  selector: 'app-checkout-dialog',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    DialogModule,
    CheckoutStepsComponent,
    CheckoutSummaryComponent,
    CustomerStepComponent,
    PaymentStepComponent,
    FulfillmentStepComponent,
  ],
  templateUrl: './checkout-dialog.component.html',
  styleUrl: './checkout-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutDialogComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly customerService = inject(CustomerService);
  private readonly branchService = inject(StoreBranchService);

  subtotal = input<number>(0);
  saleDiscount = input<number>(0);
  tax = input<number>(0);

  closed = output<void>();
  confirmed = output<CheckoutResult>();

  protected readonly steps = STEPS;
  protected readonly step = signal(1);
  protected readonly open = signal(true);
  protected readonly payment = signal<PaymentState | null>(null);

  protected readonly search = this.fb.control('');

  protected readonly form = this.fb.group({
    customerId: this.fb.control<string | null>(null),
    useCashback: this.fb.control(false),
    fulfillment: this.fb.group({
      mode: this.fb.control<FulfillmentMode>('pickup'),
      branchId: this.fb.control<string | null>(null, Validators.required),
    }),
  });

  private readonly customers$ = this.customerService.getCustomers();

  protected readonly branches = toSignal(this.branchService.getBranches(), {
    initialValue: [],
  });

  protected readonly filteredCustomers = toSignal(
    combineLatest([
      this.customers$,
      this.search.valueChanges.pipe(
        startWith(''),
        debounceTime(200),
        distinctUntilChanged(),
      ),
    ]).pipe(map(([list, term]) => list.filter(item => matches(item, term)))),
    { initialValue: [] as Customer[] },
  );

  protected readonly customerId = toSignal(
    this.form.controls.customerId.valueChanges.pipe(startWith(null)),
    { initialValue: null },
  );

  protected readonly selectedCustomer = toSignal(
    combineLatest([
      this.customers$,
      this.form.controls.customerId.valueChanges.pipe(startWith(null)),
    ]).pipe(map(([list, id]) => list.find(item => item.id === id) ?? null)),
    { initialValue: null as Customer | null },
  );

  protected readonly useCashback = toSignal(
    this.form.controls.useCashback.valueChanges.pipe(startWith(false)),
    { initialValue: false },
  );

  protected readonly mode = toSignal(
    this.form.controls.fulfillment.controls.mode.valueChanges.pipe(
      startWith(this.form.controls.fulfillment.controls.mode.value),
    ),
    { initialValue: this.form.controls.fulfillment.controls.mode.value },
  );

  protected readonly branchId = toSignal(
    this.form.controls.fulfillment.controls.branchId.valueChanges.pipe(
      startWith(null),
    ),
    { initialValue: null },
  );

  protected readonly totals = computed(() =>
    buildCheckoutTotals({
      subtotal: this.subtotal(),
      saleDiscount: this.saleDiscount(),
      tax: this.tax(),
      customer: this.selectedCustomer(),
      useCashback: this.useCashback(),
      paymentRate: this.payment()?.details.discountRate ?? 0,
    }),
  );

  protected readonly canAdvance = computed(() => {
    switch (this.step()) {
      case 1:
        return true;
      case 2:
        return !!this.payment()?.valid;
      default:
        return this.mode() === 'delivery' || !!this.branchId();
    }
  });

  ngOnInit(): void {
    const { customerId, useCashback, fulfillment } = this.form.controls;

    // A balance belongs to one customer, so switching customers drops the toggle.
    customerId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => useCashback.setValue(false));

    // A branch is only required when the goods are picked up in store.
    fulfillment.controls.mode.valueChanges
      .pipe(
        startWith(fulfillment.controls.mode.value),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(mode => {
        const branchId = fulfillment.controls.branchId;
        branchId.setValidators(mode === 'pickup' ? Validators.required : null);
        branchId.updateValueAndValidity({ emitEvent: false });
      });
  }

  protected selectCustomer(id: string | null): void {
    this.form.controls.customerId.setValue(id);
  }

  protected selectBranch(id: string): void {
    this.form.controls.fulfillment.controls.branchId.setValue(id);
  }

  protected toggleCashback(enabled: boolean): void {
    this.form.controls.useCashback.setValue(enabled);
  }

  protected back(): void {
    this.step.update(step => Math.max(step - 1, 1));
  }

  protected next(): void {
    if (this.canAdvance()) {
      this.step.update(step => Math.min(step + 1, STEPS.length));
    }
  }

  protected close(): void {
    if (!this.open()) {
      return;
    }

    this.open.set(false);
    this.closed.emit();
  }

  protected confirm(): void {
    const payment = this.payment();
    if (!payment?.valid || !this.canAdvance()) {
      return;
    }

    this.confirmed.emit({
      customer: this.selectedCustomer(),
      useCashback: this.useCashback(),
      fulfillment: this.form.controls.fulfillment.getRawValue(),
      payment: payment.details,
      totals: this.totals(),
    });
  }
}
