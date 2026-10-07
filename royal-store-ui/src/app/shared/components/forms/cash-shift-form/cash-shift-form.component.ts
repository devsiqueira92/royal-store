import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { CashMovementFormComponent } from '@app/features/cash-shift/components/cash-movement-form/cash-movement-form.component';
import { CashShiftSummaryComponent } from '@app/features/cash-shift/components/cash-shift-summary/cash-shift-summary.component';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { CashMovementRequest } from '@app/features/cash-shift/interfaces/cash-movement-request.interface';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { NotificationService } from '@app/shared/services/notification.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { filter, take } from 'rxjs';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift-form',
  imports: [
    TranslatePipe,
    ButtonModule,
    CashShiftSummaryComponent,
    CashMovementFormComponent,
  ],
  templateUrl: './cash-shift-form.component.html',
  styleUrl: './cash-shift-form.component.scss',
})
export class CashShiftFormComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);

  readonly cashShift = input.required<CashShift>();

  constructor() {
    this.cashShiftFacade.current$
      .pipe(
        filter(cashShift => !cashShift),
        take(1),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.ref.close());
  }

  protected close(): void {
    this.ref.close();
  }

  protected registerMovement(request: CashMovementRequest): void {
    // TODO: call CashShiftService.registerMovement(...) once the add/remove-money endpoint exists.
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

  protected closeShift() {
    this.router.navigate(['cash-shift/close']);
    this.close();
  }
}
