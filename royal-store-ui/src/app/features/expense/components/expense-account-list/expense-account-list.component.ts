import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { FinancialAccount, FinancialAccountType } from '../../interfaces/expense.interface';

/**
 * The financial accounts table.
 *
 * Presentational: it renders the accounts it is given and reports what was
 * clicked. No balance is shown anywhere, because an account holds none — the
 * only figure here is what left it in the period the page asked for.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-account-list',
  imports: [
    CurrencyPipe,
    TranslatePipe,
    TableModule,
    ButtonModule,
    TooltipModule,
    SwitchComponent,
    PlainDatePipe,
  ],
  templateUrl: './expense-account-list.component.html',
  styleUrl: './expense-account-list.component.scss',
})
export class ExpenseAccountListComponent {
  readonly accounts = input.required<FinancialAccount[]>();
  readonly loading = input<boolean>(false);
  /** Blocks the switches while a write is in flight, so a double toggle cannot race. */
  readonly saving = input<boolean>(false);
  /** `yyyy-MM` — the period the outflow column covers, named in its header. */
  readonly competenceMonth = input.required<string>();

  readonly edit = output<FinancialAccount>();
  /** Carries the event too: the page anchors its confirmation popup to the button pressed. */
  readonly remove = output<{ account: FinancialAccount; event: Event }>();
  readonly activeChange = output<{ id: string; active: boolean }>();

  private readonly typeIcons: Record<FinancialAccountType, string> = {
    cash: 'pi pi-money-bill',
    'bank-account': 'pi pi-building',
    'digital-wallet': 'pi pi-mobile',
  };

  protected iconOf(account: FinancialAccount): string {
    return this.typeIcons[account.type] ?? 'pi pi-wallet';
  }

  protected onActiveChange(account: FinancialAccount, active: boolean): void {
    if (active === account.isActive) {
      return;
    }

    this.activeChange.emit({ id: account.id, active });
  }
}
