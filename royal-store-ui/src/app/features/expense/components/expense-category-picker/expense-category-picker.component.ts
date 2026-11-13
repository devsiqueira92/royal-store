import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ExpenseCategorySummary } from '../../interfaces/expense.interface';
import { seriesColor } from '../../utils/expense.util';

/**
 * Picks the category a bill is filed under.
 *
 * A radio group rather than a select: there are a handful of categories, each
 * needs a colour and a line of examples to be picked correctly, and a shopkeeper
 * filing a bill should not have to open a menu to see the options. Built on real
 * `input[type=radio]`s, so arrow keys, labels and form validation all work
 * without being re-implemented.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-category-picker',
  imports: [TranslatePipe],
  templateUrl: './expense-category-picker.component.html',
  styleUrl: './expense-category-picker.component.scss',
})
export class ExpenseCategoryPickerComponent {
  readonly categories = input<readonly ExpenseCategorySummary[]>([]);
  readonly selectedId = input<string | null>(null);
  readonly label = input<string>('expense.form.category');
  readonly invalid = input<boolean>(false);

  readonly selected = output<string>();

  protected readonly colorOf = seriesColor;
}
