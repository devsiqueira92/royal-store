import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import { ExpenseMonthSummary } from '../../interfaces/expense.interface';

/**
 * The four headline figures of the competence month.
 *
 * They are deliberately NOT affected by the list filters: the API computes them
 * over the whole month, and a card that moved when a chip was clicked could no
 * longer answer "what do I owe this month?".
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-summary-cards',
  imports: [CurrencyPipe, TranslatePipe, SkeletonModule, PlainDatePipe],
  templateUrl: './expense-summary-cards.component.html',
  styleUrl: './expense-summary-cards.component.scss',
})
export class ExpenseSummaryCardsComponent {
  readonly summary = input<ExpenseMonthSummary | null>(null);
  /** `yyyy-MM`, shown as the eyebrow of the total card. */
  readonly competenceMonth = input.required<string>();
  readonly loading = input<boolean>(false);
}
