import { DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  CashMovementResponse,
  RecordedCashMovementType,
} from '../../interfaces/cash-movement-response.interface';

const OUTGOING_TYPES: RecordedCashMovementType[] = ['withdrawal'];

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-movement-list',
  imports: [DatePipe, DecimalPipe, TranslatePipe],
  templateUrl: './cash-movement-list.component.html',
  styleUrl: './cash-movement-list.component.scss',
})
export class CashMovementListComponent {
  readonly movements = input.required<CashMovementResponse[]>();

  protected isOutgoing(type: RecordedCashMovementType): boolean {
    return OUTGOING_TYPES.includes(type);
  }
}
