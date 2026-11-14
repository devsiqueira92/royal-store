import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { catchError, finalize, map, of, Subject, switchMap } from 'rxjs';
import { IbptTaxService } from '@app/core/ibpt-tax/services/ibpt-tax.service';
import { ExpenseRecurrenceService } from '@app/features/expense/services/expense-recurrence.service';
import { Product } from '@app/features/product/interfaces/product.interface';
import { ProductService } from '@app/features/product/services/product.service';
import { PricingResultComponent } from '../../components/pricing-result/pricing-result.component';
import {
  CardFee,
  CardFeeMethod,
  FixedExpense,
  PricingInput,
  PricingSettings,
} from '../../interfaces/pricing.interface';
import {
  approximateTaxRate,
  calculatePrice,
  fixedExpenseRate,
  profitByMethod,
  recurrenceToFixedExpense,
  weightedCardRate,
} from '../../utils/pricing.util';

const SETTINGS_KEY = 'royal-store.pricing-settings';

const DEFAULT_SETTINGS: PricingSettings = {
  expenses: [],
  monthlyRevenue: null,
  fees: [
    { method: 'debit', rate: 0, share: 0 },
    { method: 'credit', rate: 0, share: 0 },
    { method: 'pix', rate: 0, share: 0 },
  ],
  commissionRate: 0,
  marginRate: 20,
  taxRate: 0,
};

const readSettings = (): PricingSettings => {
  try {
    const stored = localStorage.getItem(SETTINGS_KEY);
    return stored
      ? { ...DEFAULT_SETTINGS, ...(JSON.parse(stored) as Partial<PricingSettings>) }
      : DEFAULT_SETTINGS;
  } catch {
    return DEFAULT_SETTINGS;
  }
};

const digitsOf = (code: string): string => code.replace(/\D/g, '');

type ImportStatus = 'idle' | 'loading' | 'empty' | 'failed';
type TaxSource = 'manual' | 'product' | 'notFound';

/**
 * Suggested sale price by divisor markup.
 *
 * The smart component of the screen: it loads the catalog, the tax burden of
 * the chosen product and the recurring expenses, and holds every input as a
 * signal. The price itself is a pure function of those (`calculatePrice`).
 *
 * Store-wide figures (fixed expenses, revenue, card fees, margin) are kept in
 * the browser between visits; nothing here is written to the API.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CurrencyPipe,
    DecimalPipe,
    FormsModule,
    TranslatePipe,
    ButtonModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    PricingResultComponent,
  ],
  templateUrl: './price-calculator.component.html',
  styleUrl: './price-calculator.component.scss',
})
export class PriceCalculatorComponent {
  private readonly productService = inject(ProductService);
  private readonly ibptTaxService = inject(IbptTaxService);
  private readonly recurrenceService = inject(ExpenseRecurrenceService);

  private readonly initial = readSettings();
  private readonly taxLookup$ = new Subject<Product | null>();

  protected readonly isLoadingProducts = signal(true);
  protected readonly products = toSignal(
    this.productService.getAll({ isActive: true }).pipe(
      catchError(() => of([] as Product[])),
      finalize(() => this.isLoadingProducts.set(false)),
    ),
    { initialValue: [] as Product[] },
  );

  protected readonly selectedProductId = signal<string | null>(null);
  protected readonly selectedProduct = computed(
    () => this.products().find((product) => product.id === this.selectedProductId()) ?? null,
  );

  protected readonly cost = signal<number | null>(null);
  protected readonly taxRate = signal<number>(this.initial.taxRate);
  protected readonly taxSource = signal<TaxSource>('manual');
  protected readonly isLoadingTax = signal(false);

  protected readonly expenses = signal<FixedExpense[]>(this.initial.expenses);
  protected readonly monthlyRevenue = signal<number | null>(this.initial.monthlyRevenue);
  protected readonly importStatus = signal<ImportStatus>('idle');

  protected readonly fees = signal<CardFee[]>(this.initial.fees);
  protected readonly commissionRate = signal<number>(this.initial.commissionRate);
  protected readonly marginRate = signal<number>(this.initial.marginRate);

  protected readonly expensesTotal = computed(() =>
    this.expenses().reduce((sum, expense) => sum + expense.amount, 0),
  );

  protected readonly expenseRate = computed(() =>
    fixedExpenseRate(this.expensesTotal(), this.monthlyRevenue()),
  );

  /** Expenses were listed but there is no revenue to spread them over. */
  protected readonly revenueMissing = computed(
    () => this.expensesTotal() > 0 && !(this.monthlyRevenue() ?? 0),
  );

  protected readonly cardRate = computed(() => weightedCardRate(this.fees()));

  protected readonly pricingInput = computed<PricingInput>(() => ({
    cost: this.cost() ?? 0,
    taxRate: this.taxRate(),
    cardRate: this.cardRate(),
    fixedExpenseRate: this.expenseRate(),
    commissionRate: this.commissionRate(),
    marginRate: this.marginRate(),
  }));

  protected readonly result = computed(() => calculatePrice(this.pricingInput()));

  protected readonly methods = computed(() =>
    this.result().error ? [] : profitByMethod(this.result().price, this.pricingInput(), this.fees()),
  );

  constructor() {
    this.taxLookup$
      .pipe(
        switchMap((product) => {
          // A service is classified by NBS, a physical product by NCM.
          const isService = product?.type === 1;
          const code = (isService ? product?.nbs : product?.ncm) ?? '';

          if (!product || !code) {
            return of<number | null>(null);
          }

          this.isLoadingTax.set(true);

          return this.ibptTaxService.search(code, isService ? 1 : 0).pipe(
            map((taxes) => {
              const tax = taxes.find((item) => digitsOf(item.code) === digitsOf(code));
              return tax ? approximateTaxRate(tax, product.origin) : null;
            }),
            catchError(() => of<number | null>(null)),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((rate) => {
        this.isLoadingTax.set(false);

        // No published rate: whatever the user had typed stays in place.
        if (rate === null) {
          this.taxSource.set(this.selectedProduct() ? 'notFound' : 'manual');
          return;
        }

        this.taxRate.set(rate);
        this.taxSource.set('product');
      });

    effect(() => {
      const settings: PricingSettings = {
        expenses: this.expenses(),
        monthlyRevenue: this.monthlyRevenue(),
        fees: this.fees(),
        commissionRate: this.commissionRate(),
        marginRate: this.marginRate(),
        taxRate: this.taxRate(),
      };

      try {
        localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
      } catch {
        // Storage unavailable (private mode, quota): the calculator still works.
      }
    });
  }

  protected onProductChange(productId: string | null): void {
    this.selectedProductId.set(productId);

    const product = this.selectedProduct();
    if (product) {
      this.cost.set(product.baseCostPrice ?? null);
    }

    this.taxLookup$.next(product);
  }

  protected onTaxRateChange(rate: number | null): void {
    this.taxRate.set(rate ?? 0);
    this.taxSource.set('manual');
  }

  protected addExpense(): void {
    this.expenses.update((expenses) => [
      ...expenses,
      { id: crypto.randomUUID(), description: '', amount: 0, recurrenceId: null },
    ]);
  }

  protected updateExpense(id: string, patch: Partial<FixedExpense>): void {
    this.expenses.update((expenses) =>
      expenses.map((expense) => (expense.id === id ? { ...expense, ...patch } : expense)),
    );
  }

  protected removeExpense(id: string): void {
    this.expenses.update((expenses) => expenses.filter((expense) => expense.id !== id));
  }

  /**
   * Brings in the active recurring templates as monthly amounts. Rows imported
   * before are replaced, so a second import refreshes instead of duplicating;
   * rows typed by hand are left alone.
   */
  protected importRecurrences(): void {
    this.importStatus.set('loading');

    this.recurrenceService
      .getAll(true)
      .pipe(
        map((list) => list.items.filter((item) => item.isActive).map(recurrenceToFixedExpense)),
        catchError(() => of(null)),
      )
      .subscribe((imported) => {
        if (imported === null) {
          this.importStatus.set('failed');
          return;
        }

        this.importStatus.set(imported.length ? 'idle' : 'empty');
        this.expenses.update((expenses) => [
          ...expenses.filter((expense) => !expense.recurrenceId),
          ...imported,
        ]);
      });
  }

  protected updateFee(method: CardFeeMethod, patch: Partial<CardFee>): void {
    this.fees.update((fees) =>
      fees.map((fee) => (fee.method === method ? { ...fee, ...patch } : fee)),
    );
  }
}
