import {
  ChangeDetectionStrategy,
  Component,
  inject,
  output,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { filter, take } from 'rxjs';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShift } from '../../interfaces/cash-shift.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-open-cash-shift',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    InputNumberFieldComponent,
  ],
  templateUrl: './open-cash-shift.component.html',
  styleUrl: './open-cash-shift.component.scss',
})
export class OpenCashShiftComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly cashShiftFacade = inject(CashShiftFacade);

  readonly opened = output<CashShift>();

  protected readonly submitting = toSignal(this.cashShiftFacade.isOpening$, {
    initialValue: false,
  });

  protected readonly formGroup = new FormGroup({
    openingBalance: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
  });

  constructor() {
    this.cashShiftFacade.current$
      .pipe(
        filter(cashShift => !!cashShift),
        take(1),
        takeUntilDestroyed(),
      )
      .subscribe(cashShift => {
        this.ref.close(cashShift);
        this.opened.emit(cashShift!);
      });
  }

  protected confirm(): void {
    if (this.formGroup.invalid || this.submitting()) {
      this.formGroup.markAllAsTouched();
      return;
    }
    this.cashShiftFacade.openCashShift({
      openingBalance: this.formGroup.controls.openingBalance.value!,
    });
  }
}
