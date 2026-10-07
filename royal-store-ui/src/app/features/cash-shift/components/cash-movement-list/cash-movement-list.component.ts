import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  CashMovement,
  CashMovementType,
} from '../../interfaces/cash-movement.interface';

const OUTGOING_TYPES: CashMovementType[] = ['withdrawal'];

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-movement-list',
  imports: [DatePipe, DecimalPipe, TranslatePipe],
  templateUrl: './cash-movement-list.component.html',
  styleUrl: './cash-movement-list.component.scss',
})
export class CashMovementListComponent {
  readonly movements = input.required<CashMovement[]>();

  protected isOutgoing(type: CashMovementType): boolean {
    return OUTGOING_TYPES.includes(type);
  }
}
