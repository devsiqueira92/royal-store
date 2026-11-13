import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { ExpenseFacade } from '../../facade/expense.facade';
import { FinancialAccount } from '../../interfaces/expense.interface';
import {
  firstDayOfMonth,
  lastDayOfMonth,
  shiftCompetenceMonth,
  toCompetenceMonth,
} from '../../utils/expense.util';
import { ExpenseAccountListComponent } from '../../components/expense-account-list/expense-account-list.component';
import { ExpenseAccountFormComponent } from '../../components/expense-account-form/expense-account-form.component';
import { ExpenseMonthSwitchComponent } from '../../components/expense-month-switch/expense-month-switch.component';

/**
 * Financial accounts: where the money leaves from.
 *
 * The smart component of the screen. An account carries no balance (FR-026a),
 * so the only figure listed is what left it in the month on screen — which the
 * API computes from the payments when the period is asked for, and returns as
 * null when it is not.
 *
 * Inactive accounts are shown by default: this is the only place one can be
 * brought back, and an account money has left can never be deleted.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CurrencyPipe,
    RouterLink,
    TranslatePipe,
    ButtonModule,
    SwitchComponent,
    ExpenseAccountListComponent,
    ExpenseMonthSwitchComponent,
  ],
  templateUrl: './financial-accounts.component.html',
  styleUrl: './financial-accounts.component.scss',
})
export class FinancialAccountsComponent implements OnInit {
  private readonly expenseFacade = inject(ExpenseFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  protected readonly competenceMonth = signal(toCompetenceMonth(new Date()));
  protected readonly showInactive = signal(true);

  protected readonly accounts = toSignal(this.expenseFacade.financialAccounts$, {
    initialValue: [] as FinancialAccount[],
  });

  protected readonly isLoading = toSignal(this.expenseFacade.isLoadingAccounts$, {
    initialValue: false,
  });

  protected readonly isSaving = toSignal(this.expenseFacade.isSaving$, { initialValue: false });

  protected readonly activeCount = computed(
    () => this.accounts().filter((account) => account.isActive).length,
  );

  protected readonly inactiveCount = computed(() => this.accounts().length - this.activeCount());

  /** What left every account in the month on screen, for the line under the header. */
  protected readonly totalOutflow = computed(() =>
    this.accounts().reduce((sum, account) => sum + (account.outflowInPeriod ?? 0), 0),
  );

  ngOnInit(): void {
    this.load();
  }

  protected onPreviousMonth(): void {
    this.competenceMonth.set(shiftCompetenceMonth(this.competenceMonth(), -1));
    this.load();
  }

  protected onNextMonth(): void {
    this.competenceMonth.set(shiftCompetenceMonth(this.competenceMonth(), 1));
    this.load();
  }

  protected onShowInactiveChange(show: boolean): void {
    this.showInactive.set(show);
    this.load();
  }

  protected onCreate(): void {
    this.expenseFacade.clearSelectedAccount();
    this.detailsSidebarService.openDrawer(ExpenseAccountFormComponent, {
      title: 'expense.accounts.form.create',
      subtitle: 'expense.accounts.form.subtitle',
      mode: 'create',
    });
  }

  protected onEdit(account: FinancialAccount): void {
    this.expenseFacade.selectAccount(account);
    this.detailsSidebarService.openDrawer(ExpenseAccountFormComponent, {
      title: 'expense.accounts.form.edit',
      subtitle: 'expense.accounts.form.subtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange({ id, active }: { id: string; active: boolean }): void {
    this.expenseFacade.setAccountActive(id, active);
  }

  /**
   * Deleting is permanent and the API allows it only while no payment, expense
   * or template points at the account — otherwise its history would lose the
   * answer to where the money went. An account in use comes back as a 409
   * whose message the global error toast shows.
   */
  protected onDelete({ account, event }: { account: FinancialAccount; event: Event }): void {
    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: this.translateService.instant('expense.accounts.list.deleteConfirm', {
        name: account.name,
      }),
      icon: 'pi pi-info-circle',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', outlined: true },
      acceptLabel: this.translateService.instant('expense.accounts.list.deleteAccept'),
      rejectLabel: this.translateService.instant('expense.accounts.list.deleteReject'),
      accept: () => this.expenseFacade.deleteAccount(account.id),
    });
  }

  private load(): void {
    const month = this.competenceMonth();

    this.expenseFacade.loadFinancialAccounts(
      this.showInactive(),
      firstDayOfMonth(month),
      lastDayOfMonth(month),
    );
  }
}
