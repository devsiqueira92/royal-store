import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { InputNumberModule } from 'primeng/inputnumber';
import { TranslatePipe } from '@ngx-translate/core';
import {
  PaymentMethod,
  PaymentMethodFormControls,
} from '../../interfaces/payment-method.interface';
import { PaymentMethodFacade } from '../../facade/payment-method.facade';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

@Component({
  selector: 'app-payment-method-form',
  imports: [
    ReactiveFormsModule,
    InputNumberModule,
    TranslatePipe,
    FormLayoutComponent,
    ButtonComponent,
    SwitchComponent,
  ],
  templateUrl: './payment-method-form.component.html',
  styleUrl: './payment-method-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentMethodFormComponent {
  private readonly paymentMethodFacade = inject(PaymentMethodFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly selectedPaymentMethod = toSignal(
    this.paymentMethodFacade.selectedPaymentMethod$,
    { initialValue: null as PaymentMethod | null },
  );

  protected readonly formGroup = this.buildFormGroup();

  // Active is toggled immediately through its own endpoint, not part of the submit — bridge it to
  // a signal purely to render the current state.
  protected readonly active = computed(
    () => this.selectedPaymentMethod()?.active ?? false,
  );

  constructor() {
    effect(() => {
      const paymentMethod = this.selectedPaymentMethod();
      this.formGroup.reset({ discount: paymentMethod?.discount ?? 0 });
    });
  }

  protected onClose(): void {
    this.paymentMethodFacade.clearSelectedPaymentMethod();
    this.detailsSidebarService.closeDrawer();
  }

  protected onActiveChange(active: boolean): void {
    const paymentMethod = this.selectedPaymentMethod();
    if (!paymentMethod || active === paymentMethod.active) {
      return;
    }

    if (active) {
      this.paymentMethodFacade.activate(paymentMethod.id);
    } else {
      this.paymentMethodFacade.deactivate(paymentMethod.id);
    }
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const paymentMethod = this.selectedPaymentMethod();
    if (!paymentMethod) {
      return;
    }

    const { discount } = this.formGroup.getRawValue();
    this.paymentMethodFacade.updateDiscount(paymentMethod.id, {
      discount: discount ?? 0,
    });

    this.onClose();
  }

  private buildFormGroup(): FormGroup<PaymentMethodFormControls> {
    return new FormGroup<PaymentMethodFormControls>({
      discount: new FormControl(0, [
        Validators.required,
        Validators.min(0),
        Validators.max(100),
      ]),
    });
  }
}
