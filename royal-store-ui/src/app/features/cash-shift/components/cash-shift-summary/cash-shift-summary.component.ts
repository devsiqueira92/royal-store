import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { CashShiftOverview } from '../../interfaces/cash-shift-overview.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift-summary',
  imports: [CurrencyPipe, DatePipe, TranslatePipe],
  templateUrl: './cash-shift-summary.component.html',
  styleUrl: './cash-shift-summary.component.scss',
})
export class CashShiftSummaryComponent {
  readonly cashShift = input.required<CashShiftOverview>();

  protected readonly dateFormat = 'HH:mm · MMM d, y';
  protected readonly isOpen = computed(
    () => this.cashShift().closedAt === null,
  );
}
