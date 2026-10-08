import { AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  CashMovement,
  CashMovementType,
} from '../../interfaces/cash-movement.interface';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { take, tap } from 'rxjs';

const OUTGOING_TYPES: CashMovementType[] = ['withdrawal'];

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-movement-list',
  imports: [DatePipe, DecimalPipe, TranslatePipe, AsyncPipe],
  templateUrl: './cash-movement-list.component.html',
  styleUrl: './cash-movement-list.component.scss',
})
export class CashMovementListComponent {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  protected readonly movements$ = this.cashShiftFacade.movements$;

  protected isOutgoing(type: CashMovementType): boolean {
    return OUTGOING_TYPES.includes(type);
  }

  ngOnInit() {
    this.cashShiftFacade.current$
      .pipe(
        take(1),
        tap(shift => this.cashShiftFacade.loadMovements(shift!.id)),
      )
      .subscribe();
  }
}
