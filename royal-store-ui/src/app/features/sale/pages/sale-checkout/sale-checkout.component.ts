import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { debounceTime, distinctUntilChanged, map, startWith } from 'rxjs';
import {
  CheckoutDiscountLine,
  CheckoutResult,
} from '../../interfaces/checkout.interface';
import {
  buildCheckoutTotals,
  estimateCashbackEarned,
} from '../../interfaces/checkout-totals.util';
import {
  cartDiscount,
  cartSubtotal,
  cartTax,
  repriceItems,
} from '../../interfaces/cart-totals.util';
import { isCashShort } from '../../interfaces/cash-change.util';
import { EMPTY_SALE_CART } from '../../interfaces/sale-cart.interface';
import { DiscountMode } from '../../interfaces/sale-totals.interface';
import { RegisterSaleRequest } from '../../interfaces/register-sale-request.interface';
import { SalePriceListOption } from '../../interfaces/sale-price-list-option.interface';
import { Customer } from '../../interfaces/customer.interface';
import { PaymentEntry, PaymentState } from '../../interfaces/payment.interface';
import {
  CASHBACK_FISCAL_TYPE,
  resolveFiscalType,
} from '../../interfaces/payment-method-option.const';
import { CustomerFacade } from '@app/features/customer/facade/customer.facade';
import { PaymentMethodFacade } from '@app/features/payment-method/facade/payment-method.facade';
import { SaleFacade } from '../../facade/sale.facade';
import { NotificationService } from '@app/shared/services/notification.service';
import { DetailLayoutComponent } from '@app/shared/components/layouts/detail-layout/detail-layout.component';
import { PriceListPickerComponent } from '../../components/price-list-picker/price-list-picker.component';
import { CustomerPickerComponent } from '../../components/customer-picker/customer-picker.component';
import { PaymentPanelComponent } from '../../components/payment-panel/payment-panel.component';
import { CheckoutSummaryComponent } from '../../components/checkout-summary/checkout-summary.component';
import { FiscalReceiptPreviewComponent } from '../../components/fiscal-receipt-preview/fiscal-receipt-preview.component';

type CheckoutPhase = 'checkout' | 'receipt';

/** Quiet time after the last keystroke before the customer search hits the API. */
const CUSTOMER_SEARCH_DEBOUNCE_MS = 300;

/** Back to the POS, where the cart is still waiting (or, after a sale, a new one starts). */
const POS_ROUTE = ['/sales'];

/** Keyboard shortcuts of the page, shown on the buttons they press. */
const SHORTCUTS = {
  confirm: 'F9',
  leave: 'Esc',
} as const;

/**
 * What Escape means right now. An open dropdown handles it itself; inside a text field the first
 * press only leaves the field, so a cashier mid-typing never loses the checkout by accident.
 */
function resolveEscape(event: KeyboardEvent): 'ignore' | 'blur' | 'leave' {
  const target = event.target;
  const overlayOpen =
    document.querySelector('.p-select-overlay, .p-overlay-mask') !== null;

  if (
    event.defaultPrevented ||
    overlayOpen ||
    (target instanceof HTMLElement && target.closest('[aria-expanded="true"]'))
  ) {
    return 'ignore';
  }

  return target instanceof HTMLElement && target.closest('input, textarea')
    ? 'blur'
    : 'leave';
}

/**
 * Checkout page for the cart kept in the store: price list, customer and payment on the left, the
 * order summary with the confirm action on the right. Once the sale registers, the page turns into
 * its receipt.
 */
@Component({
  host: {
    '(document:keydown.f9)': 'onConfirmShortcut($event)',
    '(document:keydown.escape)': 'onLeaveShortcut($event)',
  },
  imports: [
    TranslatePipe,
    ButtonModule,
    DetailLayoutComponent,
    PriceListPickerComponent,
    CustomerPickerComponent,
    PaymentPanelComponent,
    CheckoutSummaryComponent,
    FiscalReceiptPreviewComponent,
  ],
  templateUrl: './sale-checkout.component.html',
  styleUrl: './sale-checkout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaleCheckoutComponent implements OnInit {
  protected readonly posRoute = POS_ROUTE;
  protected readonly shortcuts = SHORTCUTS;

  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly customerFacade = inject(CustomerFacade);
  private readonly paymentMethodFacade = inject(PaymentMethodFacade);
  private readonly saleFacade = inject(SaleFacade);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);

  /** The cart being charged; the manual discount is re-applied over the chosen list's subtotal. */
  private readonly cart = toSignal(this.saleFacade.cart$, {
    initialValue: EMPTY_SALE_CART,
  });

  protected readonly phase = signal<CheckoutPhase>('checkout');
  protected readonly payment = signal<PaymentState | null>(null);

  // Set only while a confirmed sale is in flight, so the effect below knows an
  // in-progress registration is one it started (and not a stale result from
  // a previous checkout that never got cleared from the store).
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

  protected readonly receiptSaleId = computed(() =>
    this.phase() === 'receipt' ? (this.registerResult()?.saleId ?? null) : null,
  );
  protected readonly cashbackCredited = computed(
    () => this.registerResult()?.cashbackEarned ?? 0,
  );

  /** Looked up only to attach a cashback line to the charge payload when it's redeemed. */
  private readonly methods = toSignal(
    this.paymentMethodFacade.activePaymentMethods$,
    { initialValue: [] },
  );

  protected readonly search = this.fb.control('');
  private readonly useCashbackControl = this.fb.control(false);

  /** Up to 5 active customers matching the search, straight from the API. */
  protected readonly customers = toSignal(this.customerFacade.searchResults$, {
    initialValue: [] as Customer[],
  });
  protected readonly isSearchingCustomers = toSignal(
    this.customerFacade.isSearching$,
    { initialValue: false },
  );

  /** Kept on its own: the search results change under it as soon as the term does. */
  protected readonly selectedCustomer = signal<Customer | null>(null);

  protected readonly useCashback = toSignal(
    this.useCashbackControl.valueChanges.pipe(startWith(false)),
    { initialValue: false },
  );

  // Fetched on demand from the cashback-balance endpoint whenever the customer changes.
  protected readonly cashbackBalance = toSignal(
    this.customerFacade.cashbackBalance$,
    { initialValue: null as number | null },
  );

  protected readonly priceListOptions = toSignal(
    this.saleFacade.priceListOptions$,
    { initialValue: [] as SalePriceListOption[] },
  );
  protected readonly isLoadingPriceLists = toSignal(
    this.saleFacade.isLoadingPriceListOptions$,
    { initialValue: false },
  );

  /** What the operator picked; until then the store's default list. */
  private readonly pickedPriceListId = signal<string | null>(null);

  protected readonly selectedPriceList = computed(() => {
    const options = this.priceListOptions();
    const picked = this.pickedPriceListId();

    return (
      options.find(option => option.priceListId === picked) ??
      options.find(option => option.isDefault) ??
      null
    );
  });

  // The cart priced on the chosen list: subtotal, discount and tax all follow it.
  protected readonly pricedItems = computed(() =>
    repriceItems(this.cart().items, this.selectedPriceList()),
  );
  private readonly subtotal = computed(() => cartSubtotal(this.pricedItems()));
  protected readonly discountValue = computed(() => this.cart().discountValue);
  protected readonly discountMode = computed(() => this.cart().discountMode);

  private readonly saleDiscount = computed(() =>
    cartDiscount(
      this.subtotal(),
      this.cart().discountValue,
      this.cart().discountMode,
    ),
  );
  private readonly tax = computed(() =>
    cartTax(this.pricedItems(), this.saleDiscount()),
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

  /** Every deduction actually applied, in the order the summary lists them. */
  protected readonly discountLines = computed<CheckoutDiscountLine[]>(() => {
    // The manual discount is not listed here: the summary shows it on its own editable row.
    const { paymentDiscount, paymentRate, cashbackApplied } = this.totals();

    const lines: CheckoutDiscountLine[] = [
      {
        label: 'sale.checkout.summary.paymentDiscount',
        rate: paymentRate,
        amount: paymentDiscount,
      },
      {
        label: 'sale.checkout.summary.cashback',
        rate: null,
        amount: cashbackApplied,
      },
    ];

    return lines.filter(line => line.amount > 0);
  });

  /** Real discounts only — redeemed cashback is the customer's own money, not a saving. */
  protected readonly savings = computed(
    () => this.totals().saleDiscount + this.totals().paymentDiscount,
  );

  protected readonly cashbackEarned = computed(() =>
    estimateCashbackEarned(this.totals()),
  );

  protected readonly unitCount = computed(() =>
    this.cart().items.reduce((total, item) => total + item.quantity, 0),
  );

  /** What the customer handed over in cash; kept after the sale for the receipt's change line. */
  protected readonly cashReceived = computed(
    () => this.payment()?.details.cashReceived ?? null,
  );

  protected readonly installments = computed(
    () => this.payment()?.details.card?.installments ?? 1,
  );

  /** Why the sale can't be confirmed yet, told to the cashier next to the disabled button. */
  protected readonly blocker = computed<string | null>(() => {
    if (this.isLoadingPriceLists()) {
      return 'sale.checkout.blockers.loadingPriceLists';
    }

    if (!this.selectedPriceList()) {
      return 'sale.checkout.blockers.priceList';
    }

    const payment = this.payment();
    if (!payment?.details.methodId) {
      return 'sale.checkout.blockers.method';
    }

    if (payment.details.card && !payment.details.card.flag) {
      return 'sale.checkout.blockers.brand';
    }

    if (isCashShort(payment.details.cashReceived, this.totals().total)) {
      return 'sale.checkout.blockers.cashShort';
    }

    return payment.valid ? null : 'sale.checkout.blockers.method';
  });

  constructor() {
    // Reacts only to the registration *this* confirm() started — a checkout that never
    // confirmed ignores whatever is already sitting in the store from a previous sale.
    effect(() => {
      if (!this.awaitingResult() || this.isRegisteringSale()) {
        return;
      }

      if (this.registerResult()) {
        this.awaitingResult.set(false);
        this.phase.set('receipt');
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
    this.saleFacade.loadPriceListOptions(
      this.cart().items.map(({ variantId, quantity }) => ({ variantId, quantity })),
    );
    // A registered sale's result must not linger: the next checkout would read it as its own.
    this.destroyRef.onDestroy(() => {
      this.saleFacade.clearPriceListOptions();
      this.saleFacade.clearRegisterSaleResult();
      this.customerFacade.clearCustomerSearch();
      this.customerFacade.clearCashbackBalance();
    });

    // One request per pause in typing; the first page (blank term) loads right away.
    this.search.valueChanges
      .pipe(
        debounceTime(CUSTOMER_SEARCH_DEBOUNCE_MS),
        map(term => term.trim()),
        startWith(''),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(term => this.customerFacade.searchCustomers(term));
  }

  /** Kept in the cart store, so it survives a trip back to the POS. */
  protected changeDiscount(value: number, mode: DiscountMode): void {
    this.saleFacade.setCartDiscount(value, mode);
  }

  protected selectPriceList(id: string): void {
    this.pickedPriceListId.set(id);
  }

  /**
   * A balance belongs to one customer, so any change drops the cashback toggle and refetches the
   * usable balance of whoever is now selected. Clearing the customer brings the search back blank.
   */
  protected selectCustomer(id: string | null): void {
    const customer = this.customers().find(item => item.id === id) ?? null;
    if (customer?.id === this.selectedCustomer()?.id) {
      return;
    }

    this.selectedCustomer.set(customer);
    this.useCashbackControl.setValue(false);

    if (customer) {
      this.customerFacade.loadCashbackBalance(customer.id);
    } else {
      this.customerFacade.clearCashbackBalance();
      this.search.setValue('');
    }
  }

  protected toggleCashback(enabled: boolean): void {
    this.useCashbackControl.setValue(enabled);
  }

  /** Same rules as the confirm button; the receipt has nothing left to confirm. */
  protected onConfirmShortcut(event: Event): void {
    event.preventDefault();

    if (!(event as KeyboardEvent).repeat) {
      this.confirm();
    }
  }

  /** Back to the POS (or on to a new sale from the receipt), unless Esc was meant for a field. */
  protected onLeaveShortcut(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    if (keyboardEvent.repeat) {
      return;
    }

    switch (resolveEscape(keyboardEvent)) {
      case 'blur':
        (keyboardEvent.target as HTMLElement).blur();
        return;
      case 'leave':
        event.preventDefault();
        this.leave();
        return;
      default:
        return;
    }
  }

  protected leave(): void {
    if (!this.awaitingResult()) {
      this.router.navigate(POS_ROUTE);
    }
  }

  protected confirm(): void {
    const payment = this.payment();
    if (
      this.phase() !== 'checkout' ||
      !payment?.valid ||
      this.blocker() ||
      this.awaitingResult()
    ) {
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
          fiscalType: CASHBACK_FISCAL_TYPE,
          amount: totals.cashbackApplied,
          discountRate: 0,
          card: null,
        });
      }
    }

    const checkout: CheckoutResult = {
      priceListId: this.selectedPriceList()?.priceListId ?? null,
      customer: this.selectedCustomer(),
      useCashback: this.useCashback(),
      fulfillment: {
        // TODO: delivery isn't offered at checkout yet, so every sale is a pickup for now.
        // No branch entity exists yet (RegisterSaleFulfillment.BranchId is just an opaque
        // string), and pickup requires a non-blank one, hence this placeholder.
        mode: 'pickup',
        branchId: 'main',
        address: null,
      },
      payments,
      totals,
    };

    const request: RegisterSaleRequest = {
      items: this.cart().items.map(({ variantId, quantity, productId }) => ({
        variantId,
        quantity,
        productId,
      })),
      checkout: {
        ...checkout,
        customer: checkout.customer ? { id: checkout.customer.id } : null,
      },
    };

    this.awaitingResult.set(true);
    this.saleFacade.registerSale(request);
  }
}
