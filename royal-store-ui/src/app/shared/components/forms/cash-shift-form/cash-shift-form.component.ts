import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { CashMovementFormComponent } from '@app/features/cash-shift/components/cash-movement-form/cash-movement-form.component';
import { CashShiftSummaryComponent } from '@app/features/cash-shift/components/cash-shift-summary/cash-shift-summary.component';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { TranslatePipe } from '@ngx-translate/core';
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

  protected closeShift() {
    this.router.navigate(['cash-shift/management']);
    this.close();
  }
}
