import { CurrencyPipe, JsonPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { filter, startWith, take } from 'rxjs';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { CashShift } from '../../interfaces/cash-shift.interface';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-close-cash-shift',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    CurrencyPipe,
    ButtonModule,
    InputNumberFieldComponent,
    InputFieldComponent,
    JsonPipe,
  ],
  templateUrl: './close-cash-shift.component.html',
  styleUrl: './close-cash-shift.component.scss',
})
export class CloseCashShiftComponent implements OnInit {
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);

  protected readonly cashShift = toSignal(this.cashShiftFacade.current$, {
    initialValue: null as CashShift | null,
  });
  protected readonly isClosing = toSignal(this.cashShiftFacade.isClosing$, {
    initialValue: false,
  });

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
    const shift = this.cashShift();
    if (actual === null || !shift) {
      return null;
    }
    return actual - shift.expectedCash;
  });

  protected readonly isShort = computed(() => (this.difference() ?? 0) < 0);
  protected readonly absDifference = computed(() =>
    Math.abs(this.difference() ?? 0),
  );

  ngOnInit(): void {
    this.cashShiftFacade.loadCurrent();
  }

  protected fillExpected(): void {
    const shift = this.cashShift();
    if (shift) {
      this.formGroup.controls.actualEndingCash.setValue(shift.expectedCash);
    }
  }

  protected confirm(): void {
    if (this.formGroup.invalid || this.isClosing() || !this.cashShift()) {
      this.formGroup.markAllAsTouched();
      return;
    }

    this.cashShiftFacade.current$
      .pipe(
        filter(cashShift => !cashShift),
        take(1),
      )
      .subscribe(() => this.router.navigate(['/dashboard']));

    this.cashShiftFacade.closeCashShift({
      actualEndingCash: this.formGroup.controls.actualEndingCash.value!,
      closingNote: this.formGroup.controls.closingNote.value?.trim() || null,
    });
  }
}
