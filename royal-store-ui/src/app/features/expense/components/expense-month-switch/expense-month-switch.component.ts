import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlainDatePipe } from '@app/shared/pipes/plain-date.pipe';

/**
 * Steps the competence month the page is showing. Presentational: it holds no
 * month of its own, so the page stays the single source of which month is open.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-month-switch',
  imports: [TranslatePipe, PlainDatePipe],
  templateUrl: './expense-month-switch.component.html',
  styleUrl: './expense-month-switch.component.scss',
})
export class ExpenseMonthSwitchComponent {
  /** `yyyy-MM`. */
  readonly competenceMonth = input.required<string>();
  readonly disabled = input<boolean>(false);

  readonly previous = output<void>();
  readonly next = output<void>();
}
