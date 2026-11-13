import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  ExpenseCategorySummary,
  ExpenseFilterChip,
  ExpenseStatusFilter,
} from '../../interfaces/expense.interface';
import { seriesColor } from '../../utils/expense.util';

/**
 * Status and category chips over the month list.
 *
 * Presentational: the counts and the category list are computed by the page
 * from the month it already holds, so clicking a chip costs no request and the
 * counts cannot disagree with the rows underneath.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-filters',
  imports: [TranslatePipe],
  templateUrl: './expense-filters.component.html',
  styleUrl: './expense-filters.component.scss',
})
export class ExpenseFiltersComponent {
  readonly statusChips = input.required<readonly ExpenseFilterChip[]>();
  readonly activeStatus = input.required<ExpenseStatusFilter>();
  /** Only the categories present in the month — an empty chip helps nobody. */
  readonly categories = input<readonly ExpenseCategorySummary[]>([]);
  readonly activeCategoryId = input<string | null>(null);

  readonly statusChange = output<ExpenseStatusFilter>();
  readonly categoryChange = output<string | null>();

  protected readonly colorOf = seriesColor;
}
