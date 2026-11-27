import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { CurrencyPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputNumberModule } from 'primeng/inputnumber';
import { map, startWith, switchMap } from 'rxjs';
import {
  cashChange,
  isCashShort,
  quickCashAmounts,
} from '../../interfaces/cash-change.util';

/** Cash handed over by the customer and the change to give back. */
@Component({
  selector: 'app-cash-payment-form',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    TranslatePipe,
    InputGroupModule,
    InputGroupAddonModule,
    InputNumberModule,
  ],
  templateUrl: './cash-payment-form.component.html',
  styleUrl: './cash-payment-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CashPaymentFormComponent {
  control = input.required<FormControl<number | null>>();
  /** Amount due in cash. */
  total = input<number>(0);

  protected readonly received = toSignal(
    toObservable(this.control).pipe(
      switchMap(control =>
        control.valueChanges.pipe(
          startWith(control.value),
          map(() => control.value),
        ),
      ),
    ),
    { initialValue: null as number | null },
  );

  protected readonly change = computed(() =>
    cashChange(this.received(), this.total()),
  );
  protected readonly short = computed(() =>
    isCashShort(this.received(), this.total()),
  );
  protected readonly quickAmounts = computed(() =>
    quickCashAmounts(this.total()),
  );

  protected fill(amount: number): void {
    const control = this.control();
    control.setValue(amount);
    control.markAsTouched();
  }
}
