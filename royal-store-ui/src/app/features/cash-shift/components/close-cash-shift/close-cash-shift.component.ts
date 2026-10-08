import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { filter, take } from 'rxjs';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShift } from '../../interfaces/cash-shift.interface';
import { CloseCashShiftRequest } from '../../interfaces/close-cash-shift-request.interface';
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
export class CloseCashShiftComponent {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);

  protected readonly cashShift = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });
  protected readonly isClosing = toSignal(this.cashShiftFacade.isClosing$, {
    initialValue: false,
  });
  protected readonly isOpen = computed(
    () => this.cashShift()?.closedAt !== null,
  );

  protected readonly shiftMeta = this.cashShift()?.id.substring(
    this.cashShift()?.id.length! - 5,
  );

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
