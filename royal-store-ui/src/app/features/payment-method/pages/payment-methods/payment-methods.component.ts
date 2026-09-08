import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { PaymentMethodFacade } from '../../facade/payment-method.facade';
import { PaymentMethod } from '../../interfaces/payment-method.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { PaymentMethodFormComponent } from '../../components/payment-method-form/payment-method-form.component';

@Component({
  imports: [TableModule, ButtonModule, TranslatePipe, SwitchComponent],
  templateUrl: './payment-methods.component.html',
  styleUrl: './payment-methods.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentMethodsComponent implements OnInit {
  private readonly paymentMethodFacade = inject(PaymentMethodFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly paymentMethods = toSignal(
    this.paymentMethodFacade.paymentMethods$,
    { initialValue: [] as PaymentMethod[] },
  );

  protected readonly isLoading = toSignal(this.paymentMethodFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.paymentMethodFacade.loadPaymentMethods();
  }

  protected openDetails(paymentMethod: PaymentMethod): void {
    this.paymentMethodFacade.selectPaymentMethod(paymentMethod.id);
    this.detailsSidebarService.openDrawer(PaymentMethodFormComponent, {
      title: 'paymentMethod.form.title',
      subtitle: 'paymentMethod.form.subtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange(
    paymentMethod: PaymentMethod,
    active: boolean,
  ): void {
    if (active === paymentMethod.active) {
      return;
    }

    if (active) {
      this.paymentMethodFacade.activate(paymentMethod.id);
    } else {
      this.paymentMethodFacade.deactivate(paymentMethod.id);
    }
  }
}
