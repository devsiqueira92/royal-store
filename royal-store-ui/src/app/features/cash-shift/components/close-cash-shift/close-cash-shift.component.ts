import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { filter, take } from 'rxjs';
import { CashShiftStatus } from '@app/shared/enum/cash-shift-status.enum';
import { NotificationService } from '@app/shared/services/notification.service';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShift } from '../../interfaces/cash-shift.interface';
import { CashMovement } from '../../interfaces/cash-movement.interface';
import { CashMovementRequest } from '../../interfaces/cash-movement-request.interface';
import { CloseCashShiftRequest } from '../../interfaces/close-cash-shift-request.interface';
import {
  CASH_MOVEMENTS_MOCK,
  CASH_SHIFT_META_MOCK,
} from '../../mocks/cash-shift.mock';
import { CashShiftSummaryComponent } from '../cash-shift-summary/cash-shift-summary.component';
import { CashMovementFormComponent } from '../cash-movement-form/cash-movement-form.component';
import { CashMovementListComponent } from '../cash-movement-list/cash-movement-list.component';
import { CloseShiftFormComponent } from '../close-shift-form/close-shift-form.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-close-cash-shift',
  imports: [
    TranslatePipe,
    CashShiftSummaryComponent,
    CashMovementFormComponent,
    CashMovementListComponent,
    CloseShiftFormComponent,
  ],
  templateUrl: './close-cash-shift.component.html',
  styleUrl: './close-cash-shift.component.scss',
})
export class CloseCashShiftComponent implements OnInit {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);

  protected readonly cashShift = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });
  protected readonly isClosing = toSignal(this.cashShiftFacade.isClosing$, {
    initialValue: false,
  });
  protected readonly isOpen = computed(() => this.cashShift() !== null);

  // Mocked until the shift metadata and cash-movement endpoints exist.
  protected readonly shiftMeta = CASH_SHIFT_META_MOCK;
  protected readonly movements = signal<CashMovement[]>(CASH_MOVEMENTS_MOCK);

  ngOnInit(): void {
    this.cashShiftFacade.loadCurrent();
  }

  protected registerMovement(request: CashMovementRequest): void {
    // TODO: call CashShiftService.registerMovement(...) once the add/remove-money endpoint exists.
    this.movements.update(movements => [
      {
        id: crypto.randomUUID(),
        type: request.direction === 'in' ? 'supplement' : 'withdrawal',
        amount: request.amount,
        description: request.reason,
        occurredAt: new Date().toISOString(),
      },
      ...movements,
    ]);

    const messageKey =
      request.direction === 'in'
        ? 'cashShift.movement.addedSuccess'
        : 'cashShift.movement.removedSuccess';

    this.notificationService.success({
      detail: this.translateService.instant(messageKey, {
        amount: request.amount.toFixed(2),
      }),
    });
  }

  protected closeShift(request: CloseCashShiftRequest): void {
    if (this.isClosing()) {
      return;
    }

    this.cashShiftFacade.current$
      .pipe(
        filter(cashShift => !cashShift),
        take(1),
      )
      .subscribe(() => this.router.navigate(['/dashboard']));

    this.cashShiftFacade.closeCashShift(request);
  }
}
