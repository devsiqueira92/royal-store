import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { ExpenseListItem } from '../../interfaces/expense.interface';
import { seriesColor, todayIso } from '../../utils/expense.util';

/** How a row's due date reads relative to today. */
type DueTone = 'overdue' | 'due-soon' | 'settled' | 'upcoming';

/**
 * The competence-month list. Presentational — it renders the rows it is given
 * and reports what was clicked; the page owns filtering and loading.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-list',
  imports: [CurrencyPipe, TranslatePipe, TableModule, ButtonModule, TooltipModule, PlainDatePipe],
  templateUrl: './expense-list.component.html',
  styleUrl: './expense-list.component.scss',
})
export class ExpenseListComponent {
  readonly items = input.required<ExpenseListItem[]>();
  readonly loading = input<boolean>(false);
  /** `yyyy-MM`, named in the table footer so a filtered total stays unambiguous. */
  readonly competenceMonth = input.required<string>();
  /** Sum of the rows on screen, which a filtered list makes different from the month total. */
  readonly listedTotal = input<number>(0);

  readonly view = output<ExpenseListItem>();
  readonly registerPayment = output<ExpenseListItem>();
  /**
   * Carries the click event as well as the row: cancelling is destructive, and
   * the page anchors its confirmation popup to the button that was pressed.
   */
  readonly cancel = output<{ item: ExpenseListItem; event: Event }>();

  protected readonly colorOf = seriesColor;

  /**
   * Cancelling is offered only where the API would accept it: a paid bill
   * cannot be cancelled, and one with payments has to have them reversed first.
   * Showing the button elsewhere would only ever produce a refusal.
   */
  protected canCancel(item: ExpenseListItem): boolean {
    return item.status === 'pending' && item.settledAmount === 0;
  }

  /** Whole days from today to the due date; negative once it is past. */
  protected daysUntil(dueDate: string): number {
    const [y, m, d] = dueDate.split('-').map(Number);
    const [ty, tm, td] = todayIso().split('-').map(Number);
    const due = Date.UTC(y, m - 1, d);
    const today = Date.UTC(ty, tm - 1, td);

    return Math.round((due - today) / 86_400_000);
  }

  protected dueTone(item: ExpenseListItem): DueTone {
    if (item.status === 'paid') {
      return 'settled';
    }

    if (item.isOverdue) {
      return 'overdue';
    }

    return this.daysUntil(item.dueDate) <= 3 ? 'due-soon' : 'upcoming';
  }

  /**
   * How much of the bill is settled, as a percentage. Guarded against a zero
   * amount, which the API forbids but a stale row could still carry.
   */
  protected settledPercentage(item: ExpenseListItem): number {
    if (item.amount <= 0) {
      return 0;
    }

    return Math.min(100, Math.round((item.settledAmount / item.amount) * 100));
  }
}
