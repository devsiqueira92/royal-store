import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';
import {
  ExpenseRecurrence,
  ExpenseRecurrenceList,
  RecurrenceGenerationResult,
} from '../../interfaces/expense.interface';
import { seriesColor } from '../../utils/expense.util';

/**
 * The recurring templates and the generator that turns them into bills.
 *
 * Presentational: it reports the toggle and the manual run, and never touches
 * the API itself.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-recurrence-list',
  imports: [CurrencyPipe, TranslatePipe, TableModule, ButtonModule, SwitchComponent, PlainDatePipe],
  templateUrl: './expense-recurrence-list.component.html',
  styleUrl: './expense-recurrence-list.component.scss',
})
export class ExpenseRecurrenceListComponent {
  readonly recurrences = input<ExpenseRecurrenceList | null>(null);
  readonly lastGeneration = input<RecurrenceGenerationResult | null>(null);
  /** Furthest month any template has been generated through, computed by the page. */
  readonly generatedThrough = input<string | null>(null);
  /**
   * The earliest date on which any template will produce a bill — its due date
   * minus its lead days. Shown because a run before that date creates nothing,
   * and without it the generator looks broken rather than early.
   */
  readonly nextGeneration = input<string | null>(null);
  readonly loading = input<boolean>(false);
  readonly isGenerating = input<boolean>(false);

  readonly generate = output<void>();
  readonly activeChange = output<{ id: string; active: boolean }>();

  protected readonly colorOf = seriesColor;

  protected onActiveChange(recurrence: ExpenseRecurrence, active: boolean): void {
    if (active === recurrence.isActive) {
      return;
    }

    this.activeChange.emit({ id: recurrence.id, active });
  }
}
