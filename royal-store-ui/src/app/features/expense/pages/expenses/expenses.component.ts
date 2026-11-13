import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService } from 'primeng/api';
import { TabsModule } from 'primeng/tabs';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ExpenseFacade } from '../../facade/expense.facade';
import {
  ExpenseCategorySummary,
  ExpenseFilterChip,
  ExpenseListItem,
  ExpenseMonthSummary,
  ExpenseRecurrenceList,
  ExpenseStatusFilter,
} from '../../interfaces/expense.interface';
import { shiftCompetenceMonth, toCompetenceMonth } from '../../utils/expense.util';
import { ExpenseSummaryCardsComponent } from '../../components/expense-summary-cards/expense-summary-cards.component';
import { ExpenseMonthSwitchComponent } from '../../components/expense-month-switch/expense-month-switch.component';
import { ExpenseFiltersComponent } from '../../components/expense-filters/expense-filters.component';
import { ExpenseListComponent } from '../../components/expense-list/expense-list.component';
import { ExpenseRecurrenceListComponent } from '../../components/expense-recurrence-list/expense-recurrence-list.component';
import { ExpenseCategoryReportComponent } from '../../components/expense-category-report/expense-category-report.component';
import { ExpenseCashFlowComponent } from '../../components/expense-cash-flow/expense-cash-flow.component';
import { ExpenseFormComponent } from '../../components/expense-form/expense-form.component';
import { ExpensePaymentFormComponent } from '../../components/expense-payment-form/expense-payment-form.component';

type ExpenseTab = 'expenses' | 'recurrences' | 'reports';

/**
 * Operating expenses: what it costs to keep the doors open.
 *
 * The smart component of the module. It owns the competence month, the active
 * tab and the two list filters, and hands plain data to the presentational
 * pieces below it.
 *
 * Filtering happens here rather than on the API. The month is fetched once and
 * the chips narrow it in memory, which keeps a chip click instant and — more
 * importantly — keeps the chip counts exact: read off a filtered, paginated
 * response they could only ever be the counts of one page.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    TranslatePipe,
    ButtonModule,
    TabsModule,
    ExpenseSummaryCardsComponent,
    ExpenseMonthSwitchComponent,
    ExpenseFiltersComponent,
    ExpenseListComponent,
    ExpenseRecurrenceListComponent,
    ExpenseCategoryReportComponent,
    ExpenseCashFlowComponent,
  ],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss',
})
export class ExpensesComponent implements OnInit {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  /** How many competence months the P&L covers. */
  private readonly reportMonths = 6;
  /** How far ahead the cash-flow projection looks. */
  private readonly projectionWeeks = 8;

  protected readonly competenceMonth = signal(toCompetenceMonth(new Date()));
  protected readonly activeTab = signal<ExpenseTab>('expenses');
  protected readonly statusFilter = signal<ExpenseStatusFilter>('all');
  protected readonly categoryFilter = signal<string | null>(null);

  protected readonly allItems = toSignal(this.expenseFacade.items$, {
    initialValue: [] as ExpenseListItem[],
  });

  protected readonly summary = toSignal(this.expenseFacade.summary$, {
    initialValue: null as ExpenseMonthSummary | null,
  });

  protected readonly recurrences = toSignal(this.expenseFacade.recurrences$, {
    initialValue: null as ExpenseRecurrenceList | null,
  });

  protected readonly lastGeneration = toSignal(this.expenseFacade.lastGeneration$, {
    initialValue: null,
  });

  protected readonly categoryReport = toSignal(this.expenseFacade.categoryReport$, {
    initialValue: null,
  });

  protected readonly cashFlow = toSignal(this.expenseFacade.cashFlow$, { initialValue: null });

  protected readonly isLoading = toSignal(this.expenseFacade.isLoading$, { initialValue: false });
  protected readonly isGenerating = toSignal(this.expenseFacade.isGenerating$, {
    initialValue: false,
  });
  protected readonly isLoadingReports = toSignal(this.expenseFacade.isLoadingReports$, {
    initialValue: false,
  });

  /** Only the categories that actually appear in this month. */
  protected readonly monthCategories = computed<ExpenseCategorySummary[]>(() => {
    const seen = new Map<string, ExpenseCategorySummary>();

    for (const item of this.allItems()) {
      if (!seen.has(item.category.id)) {
        seen.set(item.category.id, item.category);
      }
    }

    return [...seen.values()];
  });

  /**
   * The rows on screen. The category filter is applied first so the status
   * counts below describe what the chips would actually show next.
   */
  protected readonly visibleItems = computed<ExpenseListItem[]>(() => {
    const categoryId = this.categoryFilter();
    const status = this.statusFilter();

    return this.allItems()
      .filter((item) => !categoryId || item.category.id === categoryId)
      .filter((item) => this.matchesStatus(item, status));
  });

  protected readonly listedTotal = computed(() =>
    this.visibleItems().reduce((sum, item) => sum + item.amount, 0),
  );

  protected readonly statusChips = computed<ExpenseFilterChip[]>(() => {
    const categoryId = this.categoryFilter();
    const scoped = this.allItems().filter((item) => !categoryId || item.category.id === categoryId);

    const countOf = (status: ExpenseStatusFilter) =>
      scoped.filter((item) => this.matchesStatus(item, status)).length;

    return [
      { value: 'all', labelKey: 'expense.filters.all', count: scoped.length },
      { value: 'pending', labelKey: 'expense.status.pending', count: countOf('pending') },
      {
        value: 'partially-paid',
        labelKey: 'expense.status.partially-paid',
        count: countOf('partially-paid'),
      },
      { value: 'paid', labelKey: 'expense.status.paid', count: countOf('paid') },
      { value: 'overdue', labelKey: 'expense.status.overdue', count: countOf('overdue') },
    ];
  });

  /** The furthest month any template has produced bills through. */
  protected readonly generatedThrough = computed(() => {
    const generated = (this.recurrences()?.items ?? [])
      .map((item) => item.generatedThrough)
      .filter((value): value is string => !!value);

    return generated.length ? generated.sort().at(-1)! : null;
  });

  /**
   * The soonest any active template will create a bill — the API's `generatesOn`,
   * which is the due date minus the template's lead days. A generator run before
   * that date is a no-op, so the date is what makes the run legible.
   */
  protected readonly nextGeneration = computed(() => {
    const upcoming = (this.recurrences()?.items ?? [])
      .filter((item) => item.isActive)
      .map((item) => item.generatesOn)
      .filter((value): value is string => !!value)
      .sort();

    return upcoming.at(0) ?? null;
  });

  ngOnInit(): void {
    this.expenseFacade.loadMonth(this.competenceMonth());
    this.expenseFacade.loadCategories();
    this.expenseFacade.loadFinancialAccounts();
  }

  protected onTabChange(value: string | number | undefined): void {
    if (value === undefined) {
      return;
    }

    const tab = value as ExpenseTab;
    this.activeTab.set(tab);

    // Loaded on first view rather than upfront: the reports are three more
    // requests, and most visits only ever look at the month list.
    if (tab === 'recurrences' && !this.recurrences()) {
      this.expenseFacade.loadRecurrences();
    }

    if (tab === 'reports' && !this.categoryReport()) {
      this.expenseFacade.loadCategoryReport(this.competenceMonth(), this.reportMonths);
      this.expenseFacade.loadCashFlow(this.projectionWeeks);
    }
  }

  protected onPreviousMonth(): void {
    this.goToMonth(shiftCompetenceMonth(this.competenceMonth(), -1));
  }

  protected onNextMonth(): void {
    this.goToMonth(shiftCompetenceMonth(this.competenceMonth(), 1));
  }

  protected onStatusChange(status: ExpenseStatusFilter): void {
    this.statusFilter.set(status);
  }

  protected onCategoryChange(categoryId: string | null): void {
    this.categoryFilter.set(categoryId);
  }

  protected onCreate(): void {
    this.expenseFacade.clearSelectedExpense();
    this.detailsSidebarService.openDrawer(ExpenseFormComponent, {
      title: 'expense.form.create',
      subtitle: 'expense.form.subtitle',
      mode: 'create',
    });
  }

  protected onView(item: ExpenseListItem): void {
    this.expenseFacade.loadExpense(item.id);
    this.detailsSidebarService.openDrawer(ExpenseFormComponent, {
      title: 'expense.form.edit',
      subtitle: 'expense.form.subtitle',
      mode: 'edit',
    });
  }

  /**
   * Cancelling is not reversible from the UI, so it asks first — anchored to the
   * button that was pressed, which is how the rest of the app confirms a
   * destructive action.
   */
  protected onCancel({ item, event }: { item: ExpenseListItem; event: Event }): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('expense.list.cancelConfirm', {
        description: item.description,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('expense.list.cancelAccept'),
      rejectLabel: this.translateService.instant('expense.list.cancelReject'),
      accept: () => this.expenseFacade.cancelExpense(item.id),
    });
  }

  protected onRegisterPayment(item: ExpenseListItem): void {
    this.expenseFacade.loadExpense(item.id);
    this.detailsSidebarService.openDrawer(ExpensePaymentFormComponent, {
      title: 'expense.payment.title',
      subtitle: 'expense.payment.subtitle',
      mode: 'edit',
    });
  }

  protected onGenerate(): void {
    this.expenseFacade.generateRecurrences();
  }

  protected onRecurrenceActiveChange(change: { id: string; active: boolean }): void {
    this.expenseFacade.setRecurrenceActive(change.id, change.active);
  }

  private goToMonth(competenceMonth: string): void {
    this.competenceMonth.set(competenceMonth);
    this.expenseFacade.loadMonth(competenceMonth);

    // The P&L ends at the month on screen, so it follows the arrows; the
    // projection starts from today and does not.
    if (this.categoryReport()) {
      this.expenseFacade.loadCategoryReport(competenceMonth, this.reportMonths);
    }
  }

  private matchesStatus(item: ExpenseListItem, status: ExpenseStatusFilter): boolean {
    switch (status) {
      case 'all':
        return true;
      // Overdue is derived from the due date, not a status: a partly paid bill
      // past its date is both, and must appear under either chip.
      case 'overdue':
        return item.isOverdue;
      default:
        return item.status === status;
    }
  }
}
