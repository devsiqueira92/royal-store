import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import {
  CashMovementDirection,
  CashMovementRequest,
} from '../../interfaces/cash-movement-request.interface';
import { CashShiftFacade } from '../../facade/cash-shift.facade';
import { RecordCashMovementRequest } from '../../interfaces/record-cash-movement-request.interface';

interface MovementOption {
  label: string;
  value: CashMovementDirection;
}

const MOVEMENT_OPTIONS: MovementOption[] = [
  { label: 'cashShift.movement.addMoney', value: 'in' },
  { label: 'cashShift.movement.removeMoney', value: 'out' },
];

const QUICK_REASON_KEYS: Record<CashMovementDirection, string[]> = {
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
  selector: 'app-cash-movement-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonModule,
    InputNumberModule,
    InputTextModule,
  ],
  templateUrl: './cash-movement-form.component.html',
  styleUrl: './cash-movement-form.component.scss',
})
export class CashMovementFormComponent {
  private readonly translateService = inject(TranslateService);
  private readonly cashShiftFacade = inject(CashShiftFacade);

  protected readonly movementOptions = MOVEMENT_OPTIONS;
  protected readonly movementType = signal<CashMovementDirection>('in');
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

  protected isInvalid(controlName: 'amount' | 'reason'): boolean {
    const control = this.formGroup.controls[controlName];
    return control.touched && control.invalid;
  }

  protected applyQuickReason(reasonKey: string): void {
    this.formGroup.controls.reason.setValue(
      this.translateService.instant(reasonKey),
    );
  }

  protected submit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }
    const movement: RecordCashMovementRequest = {
      amount: this.formGroup.controls.amount.value!,
      description: this.formGroup.controls.reason.value!.trim(),
    };

    if (this.movementType() === 'in') {
      this.cashShiftFacade.recordSupplement(movement);
    } else {
      this.cashShiftFacade.recordWithdrawal(movement);
    }

    this.formGroup.reset();
  }
}
