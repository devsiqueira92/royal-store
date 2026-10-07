import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { CashShiftStatus } from '@app/shared/enum/cash-shift-status.enum';
import { CashShift } from '../../interfaces/cash-shift.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift-summary',
  imports: [CurrencyPipe, DatePipe, TranslatePipe],
  templateUrl: './cash-shift-summary.component.html',
  styleUrl: './cash-shift-summary.component.scss',
})
export class CashShiftSummaryComponent {
  readonly cashShift = input.required<CashShift>();

  protected readonly dateFormat = 'HH:mm · MMM d, y';
  protected readonly isOpen = computed(
    () => this.cashShift().status === CashShiftStatus.Open,
  );
}
