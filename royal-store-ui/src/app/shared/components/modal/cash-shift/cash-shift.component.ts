import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, take } from 'rxjs';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputNumberFieldComponent } from '@app/shared/components/form-fields/input-number-field/input-number-field.component';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { NotificationService } from '@app/shared/services/notification.service';
import { CashShiftStatus } from '@app/shared/enum/cash-shift-status.enum';
import { CashShift } from '@app/features/cash-shift/interfaces/cash-shift.interface';
import { CashShiftFacade } from '@app/features/cash-shift/facade/cash-shift.facade';
import { Router } from '@angular/router';

type CashMovementType = 'in' | 'out';

interface MovementOption {
  label: string;
  value: CashMovementType;
}

const MOVEMENT_OPTIONS: MovementOption[] = [
  { label: 'cashShift.movement.addMoney', value: 'in' },
  { label: 'cashShift.movement.removeMoney', value: 'out' },
];

const QUICK_REASON_KEYS: Record<CashMovementType, string[]> = {
  in: [
    'cashShift.movement.quickReason.changeFund',
    'cashShift.movement.quickReason.ownerContribution',
    'cashShift.movement.quickReason.reimbursement',
  ],
  out: [
    'cashShift.movement.quickReason.bankDeposit',
    'cashShift.movement.quickReason.safeTransfer',
    'cashShift.movement.quickReason.refund',
  ],
};

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-cash-shift',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    TranslatePipe,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    SelectButtonModule,
    InputNumberFieldComponent,
    InputFieldComponent,
  ],
  templateUrl: './cash-shift.component.html',
  styleUrl: './cash-shift.component.scss',
})
export class CashShiftComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  private readonly notificationService = inject(NotificationService);
  private readonly translateService = inject(TranslateService);
  private readonly cashShiftFacade = inject(CashShiftFacade);
  private readonly router = inject(Router);

  protected readonly CashShiftStatus = CashShiftStatus;
  protected readonly cashShift = (this.config.data as { cashShift: CashShift })
    .cashShift;

  protected readonly movementOptions = MOVEMENT_OPTIONS;
  protected readonly movementType = signal<CashMovementType>('in');
  protected readonly quickReasons = computed(
    () => QUICK_REASON_KEYS[this.movementType()],
  );

  protected readonly formGroup = new FormGroup({
    amount: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
    reason: new FormControl<string | null>(null, [
      Validators.required,
      Validators.maxLength(200),
    ]),
  });

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

  protected onMovementTypeChange(type: CashMovementType): void {
    this.movementType.set(type);
  }

  protected applyQuickReason(reasonKey: string): void {
    this.formGroup.controls.reason.setValue(
      this.translateService.instant(reasonKey),
    );
  }

  protected submitMovement(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    // TODO: call CashShiftService.registerMovement(...) once the add/remove-money endpoint exists.
    const amount = this.formGroup.controls.amount.value!;
    const messageKey =
      this.movementType() === 'in'
        ? 'cashShift.movement.addedSuccess'
        : 'cashShift.movement.removedSuccess';

    this.notificationService.success({
      detail: this.translateService.instant(messageKey, {
        amount: amount.toFixed(2),
      }),
    });

    this.formGroup.reset();
  }

  protected closeShift() {
    this.router.navigate(['cash-shift/close']);
    this.close();
  }
}
