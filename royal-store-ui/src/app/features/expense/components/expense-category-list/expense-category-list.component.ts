import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { ExpenseCategoryRow } from '../../interfaces/expense.interface';
import { seriesColor } from '../../utils/expense.util';

/**
 * The category hierarchy as a flat, indented table.
 *
 * Presentational: it renders the rows it is given and reports what was clicked.
 * The page owns loading, the drawer and the confirmations.
 */
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-expense-category-list',
  imports: [TranslatePipe, TableModule, ButtonModule, TooltipModule, SwitchComponent],
  templateUrl: './expense-category-list.component.html',
  styleUrl: './expense-category-list.component.scss',
})
export class ExpenseCategoryListComponent {
  readonly rows = input.required<ExpenseCategoryRow[]>();
  readonly loading = input<boolean>(false);
  /** Blocks the switches while a write is in flight, so a double toggle cannot race. */
  readonly saving = input<boolean>(false);

  readonly edit = output<ExpenseCategoryRow>();
  /** Carries the event too: the page anchors its confirmation popup to the button pressed. */
  readonly remove = output<{ row: ExpenseCategoryRow; event: Event }>();
  readonly activeChange = output<{ id: string; active: boolean }>();

  protected readonly colorOf = seriesColor;

  /** One indent step per level, applied as padding so the cell border stays put. */
  protected indentOf(row: ExpenseCategoryRow): string {
    return `${row.depth * 1.25}rem`;
  }

  /**
   * A category with children is refused by the API's delete (its children
   * reference it), so the button is disabled rather than offered and rejected.
   * Being referenced by an expense cannot be known from here — that one comes
   * back as a 409 explaining itself.
   */
  protected canDelete(row: ExpenseCategoryRow): boolean {
    return row.childCount === 0;
  }

  protected onActiveChange(row: ExpenseCategoryRow, active: boolean): void {
    if (active === row.isActive) {
      return;
    }

    this.activeChange.emit({ id: row.id, active });
  }
}
