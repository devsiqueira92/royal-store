import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { startWith } from 'rxjs';
import { CashShift } from '../../interfaces/cash-shift.interface';
import { CloseCashShiftRequest } from '../../interfaces/close-cash-shift-request.interface';
import { CashShiftOverview } from '../../interfaces/cash-shift-overview.interface';

type DifferenceState = 'balanced' | 'over' | 'short';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-close-shift-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    CurrencyPipe,
    ButtonModule,
    InputNumberModule,
    InputTextModule,
  ],
  templateUrl: './close-shift-form.component.html',
  styleUrl: './close-shift-form.component.scss',
})
export class CloseShiftFormComponent {
  readonly cashShift = input.required<CashShift | CashShiftOverview>();
  readonly isOpenShift = input.required<boolean>();
  readonly isClosing = input(false);
  readonly canPerformAction = input(true);

  readonly confirmed = output<CloseCashShiftRequest>();

  protected readonly formGroup = new FormGroup({
    actualEndingCash: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0),
    ]),
    closingNote: new FormControl<string | null>(null, [
      Validators.maxLength(200),
    ]),
  });

  protected readonly actualEndingCash = toSignal(
    this.formGroup.controls.actualEndingCash.valueChanges.pipe(
      startWith(this.formGroup.controls.actualEndingCash.value),
    ),
    { initialValue: null },
  );

  protected readonly difference = computed(() => {
    const actual = this.actualEndingCash();
    if (actual === null) {
      return null;
    }
    return Math.round((actual - this.cashShift().expectedCash) * 100) / 100;
  });

  protected readonly differenceState = computed<DifferenceState>(() => {
    const difference = this.difference() ?? 0;
    if (difference === 0) {
      return 'balanced';
    }
    return difference > 0 ? 'over' : 'short';
  });

  protected readonly differenceSign = computed(() => {
    const signs: Record<DifferenceState, string> = {
      balanced: '',
      over: '+',
      short: '−',
    };
    return signs[this.differenceState()];
  });

  protected readonly absDifference = computed(() =>
    Math.abs(this.difference() ?? 0),
  );

  protected isActualInvalid(): boolean {
    const control = this.formGroup.controls.actualEndingCash;
    return control.touched && control.invalid;
  }

  protected fillExpected(): void {
    this.formGroup.controls.actualEndingCash.setValue(
      this.cashShift().expectedCash,
    );
  }

  protected confirm(): void {
    if (this.formGroup.invalid || this.isClosing()) {
      this.formGroup.markAllAsTouched();
      return;
    }

    this.confirmed.emit({
      actualEndingCash: this.formGroup.controls.actualEndingCash.value!,
      closingNote: this.formGroup.controls.closingNote.value?.trim() || null,
    });
  }
}
