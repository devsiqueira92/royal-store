import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DynamicDialogConfig } from 'primeng/dynamicdialog';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { CashShiftFormComponent } from '../../forms/cash-shift-form/cash-shift-form.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CashShiftFormComponent],
  templateUrl: './cash-shift.component.html',
  styleUrl: './cash-shift.component.scss',
})
export class CashShiftComponent {
  private readonly config = inject(DynamicDialogConfig);

  protected readonly cashShift = (this.config.data as { cashShift: CashShift }).cashShift;
}
