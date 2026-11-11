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
  of,
  startWith,
  switchMap,
  tap,
} from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import {
  CheckoutResult,
  SaleAddress,
  SaleAddressFormControls,
} from '../../interfaces/checkout.interface';
import { buildCheckoutTotals } from '../../interfaces/checkout-totals.util';
import { Customer } from '../../interfaces/customer.interface';
import { PaymentEntry, PaymentState } from '../../interfaces/payment.interface';
import {
  CASHBACK_FISCAL_TYPE,
  resolveFiscalType,
} from '../../interfaces/payment-method-option.const';
import { SaleLineItem } from '../../interfaces/sale-line-item.interface';
import { CustomerFacade } from '@app/features/customer/facade/customer.facade';
import { CustomerAddress } from '@app/features/customer/interfaces/customer-address.interface';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { SaleFacade } from '../../facade/sale.facade';
import {
  CheckoutStep,
  CheckoutStepsComponent,
} from '../checkout-steps/checkout-steps.component';
import { CheckoutSummaryComponent } from '../checkout-summary/checkout-summary.component';
import { CustomerStepComponent } from '../customer-step/customer-step.component';
import { PaymentStepComponent } from '../payment-step/payment-step.component';
import { FulfillmentStepComponent } from '../fulfillment-step/fulfillment-step.component';
import { FiscalReceiptPreviewComponent } from '../fiscal-receipt-preview/fiscal-receipt-preview.component';
import { ViaCepService } from '@app/shared/services/via-cep.service';
import { NotificationService } from '@app/shared/services/notification.service';

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
  {
    index: 4,
    label: 'sale.checkout.steps.receipt',
    title: 'sale.checkout.receipt.title',
  },
];

const RECEIPT_STEP = 4;

const onlyDigits = (value: string): string => value.replace(/\D/g, '');

const clean = (value: string | null): string | null => value?.trim() || null;

const sameAddresses = (
  previous: CustomerAddress[],
  current: CustomerAddress[],
) =>
  previous.length === current.length &&
  previous.every((address, index) => address === current[index]);

/** Null when nothing was filled in, which the API reads as "no address". */
function toSaleAddress(value: SaleAddress): SaleAddress | null {
  const address: SaleAddress = {
    zipCode: value.zipCode ? onlyDigits(value.zipCode) || null : null,
    street: clean(value.street),
    number: clean(value.number),
    city: clean(value.city),
  };

  return Object.values(address).some(Boolean) ? address : null;
}

function matches(customer: Customer, term: string): boolean {
  const text = term.trim().toLowerCase();
  if (!text) {
    return true;
  }

  const digits = onlyDigits(text);
  const phone = customer.phone ?? '';

  return (
    customer.name.toLowerCase().includes(text) ||
    (!!digits && phone.includes(digits))
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
    FiscalReceiptPreviewComponent,
  ],
  templateUrl: './checkout-dialog.component.html',
  styleUrl: './checkout-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutDialogComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly customerFacade = inject(CustomerFacade);
  private readonly paymentMethodFacade = inject(PaymentMethodFacade);
  private readonly saleFacade = inject(SaleFacade);
  private readonly viaCepService = inject(ViaCepService);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);

  subtotal = input<number>(0);
  saleDiscount = input<number>(0);
  tax = input<number>(0);
  items = input<SaleLineItem[]>([]);

  closed = output<void>();
  confirmed = output<CheckoutResult>();

  protected readonly steps = STEPS;
  protected readonly step = signal(1);
  protected readonly open = signal(true);
  protected readonly payment = signal<PaymentState | null>(null);

  // Set only while a confirmed sale is in flight, so the effect below knows an
  // in-progress registration is one it started (and not a stale result from
  // a previous dialog instance that never got cleared from the store).
  protected readonly awaitingResult = signal(false);

  protected readonly isRegisteringSale = toSignal(
    this.saleFacade.isRegisteringSale$,
    { initialValue: false },
  );
  private readonly registerResult = toSignal(
    this.saleFacade.lastRegisterSaleResult$,
    { initialValue: null },
  );
  private readonly registerError = toSignal(this.saleFacade.error$, {
    initialValue: null,
  });

  protected readonly receiptSaleId = computed(
    () => this.registerResult()?.saleId ?? null,
  );

  /** Looked up only to attach a cashback line to the charge payload when it's redeemed. */
  protected readonly methods = toSignal(
    this.paymentMethodFacade.activePaymentMethods$,
    { initialValue: [] },
  );

  protected readonly search = this.fb.control('');

  protected readonly form = this.fb.group({
    customerId: this.fb.control<string | null>(null),
    useCashback: this.fb.control(false),
    fulfillment: this.fb.group({
      customerAddressId: this.fb.control<string | null>(null),
      address: this.fb.group<SaleAddressFormControls>({
        zipCode: this.fb.control<string | null>(null, [
          Validators.pattern(/^\d{5}-?\d{3}$/),
        ]),
        street: this.fb.control<string | null>(null, [
          Validators.maxLength(200),
        ]),
        number: this.fb.control<string | null>(null, [
          Validators.maxLength(20),
        ]),
        city: this.fb.control<string | null>(null, [Validators.maxLength(100)]),
      }),
    }),
  });

  private readonly customers$ = this.customerFacade.customers$;

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

  // Fetched on demand from the cashback-balance endpoint whenever the customer changes.
  protected readonly cashbackBalance = toSignal(
    this.customerFacade.cashbackBalance$,
    { initialValue: null as number | null },
  );

  // Active addresses of the selected customer, offered in the fulfillment step.
  protected readonly customerAddresses = signal<CustomerAddress[]>([]);

  protected readonly isAddressValid = toSignal(
    this.form.controls.fulfillment.controls.address.statusChanges.pipe(
      map(status => status !== 'INVALID'),
    ),
    { initialValue: true },
  );

  protected readonly totals = computed(() =>
    buildCheckoutTotals({
      subtotal: this.subtotal(),
      saleDiscount: this.saleDiscount(),
      tax: this.tax(),
      customer: this.selectedCustomer(),
      cashbackBalance: this.cashbackBalance() ?? 0,
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
        return this.isAddressValid();
    }
  });

  constructor() {
    // Reacts only to the registration *this* confirm() started — a dialog that never
    // confirmed ignores whatever is already sitting in the store from a previous sale.
    effect(() => {
      if (!this.awaitingResult() || this.isRegisteringSale()) {
        return;
      }

      if (this.registerResult()) {
        this.awaitingResult.set(false);
        this.step.set(RECEIPT_STEP);
        return;
      }

      const error = this.registerError();
      if (error) {
        this.awaitingResult.set(false);
        this.notificationService.error({
          detail: this.translateService.instant('sale.checkout.registerError'),
        });
      }
    });
  }

  ngOnInit(): void {
    this.customerFacade.loadCustomers();

    const { customerId, useCashback, fulfillment } = this.form.controls;

    // A balance belongs to one customer, so switching customers drops the toggle and
    // refetches the usable cashback for whoever is now selected.
    customerId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(id => {
        useCashback.setValue(false);

        if (id) {
          this.customerFacade.loadCashbackBalance(id);
        } else {
          this.customerFacade.clearCashbackBalance();
        }
      });

    const { customerAddressId, address } = fulfillment.controls;

    // With a customer, the address comes read-only from their record; without
    // one, it's typed in by hand. The customer list carries no addresses, so
    // the details record is loaded and only trusted once it's this customer's.
    customerId.valueChanges
      .pipe(
        tap(id => {
          address.reset();

          if (id) {
            this.customerFacade.loadCustomer(id);
            address.disable();
          } else {
            address.enable();
          }
        }),
        switchMap(id =>
          id
            ? this.customerFacade.selectedCustomer$.pipe(
                map(customer =>
                  customer?.id === id
                    ? (customer.addresses ?? []).filter(item => item.isActive)
                    : [],
                ),
                distinctUntilChanged(sameAddresses),
              )
            : of([]),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(addresses => {
        this.customerAddresses.set(addresses);
        // Each customer's list starts on their default delivery address.
        customerAddressId.setValue(
          addresses.find(item => item.isDefaultDelivery)?.id ?? null,
        );
      });

    customerAddressId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(id => {
        const selected = this.customerAddresses().find(item => item.id === id);
        address.reset(
          selected
            ? {
                zipCode: selected.zipCode,
                street: selected.street,
                number: selected.number,
                city: selected.city,
              }
            : {},
        );
      });

    this.viaCepService
      .lookupChanges(address.controls.zipCode)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(found => {
        if (!found) {
          this.notificationService.warn({
            detail: this.translateService.instant(
              'sale.checkout.fulfillment.zipCodeNotFound',
            ),
          });
          return;
        }

        // Town-wide zip codes have no street: keep what was typed there.
        const { street, city } = address.controls;
        address.patchValue({
          street: found.logradouro || street.value,
          city: found.localidade || city.value,
        });
      });
  }

  protected selectCustomer(id: string | null): void {
    this.form.controls.customerId.setValue(id);
  }

  protected toggleCashback(enabled: boolean): void {
    this.form.controls.useCashback.setValue(enabled);
  }

  protected back(): void {
    this.step.update(step => Math.max(step - 1, 1));
  }

  protected next(): void {
    if (this.canAdvance()) {
      // Capped before the receipt step: that one is only ever reached once the sale the
      // user confirms on step 3 actually registers, never by stepping forward manually.
      this.step.update(step => Math.min(step + 1, RECEIPT_STEP - 1));
    }
  }

  protected close(): void {
    if (!this.open() || this.awaitingResult()) {
      return;
    }

    this.open.set(false);
    this.closed.emit();
  }

  protected confirm(): void {
    const payment = this.payment();
    if (!payment?.valid || !this.canAdvance() || this.awaitingResult()) {
      return;
    }

    const totals = this.totals();
    const { methodId, fiscalType, discountRate, card } = payment.details;

    const payments: PaymentEntry[] = [
      {
        methodId: methodId ?? '',
        fiscalType,
        amount: totals.total,
        discountRate,
        card,
      },
    ];

    if (totals.cashbackApplied > 0) {
      const cashbackMethod = this.methods().find(
        method =>
          resolveFiscalType(method.paymentType) === CASHBACK_FISCAL_TYPE,
      );

      if (cashbackMethod) {
        payments.push({
          methodId: cashbackMethod.id,
          fiscalType: resolveFiscalType(cashbackMethod.paymentType),
          amount: totals.cashbackApplied,
          discountRate: 0,
          card: null,
        });
      }
    }

    this.awaitingResult.set(true);

    this.confirmed.emit({
      customer: this.selectedCustomer(),
      useCashback: this.useCashback(),
      fulfillment: {
        // TODO: delivery isn't supported yet, so every sale is treated as a pickup for now.
        // No branch entity exists yet (RegisterSaleFulfillment.BranchId is just an opaque
        // string), and pickup requires a non-blank one, hence this placeholder.
        mode: 'pickup',
        branchId: 'main',
        address: toSaleAddress(
          this.form.controls.fulfillment.controls.address.getRawValue(),
        ),
      },
      payments,
      totals,
    });
  }
}
