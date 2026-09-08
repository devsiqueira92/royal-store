import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';
import { CustomerFacade } from '../../facade/customer.facade';
import { Customer } from '../../interfaces/customer.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { CustomerFormComponent } from '../../components/customer-form/customer-form.component';

@Component({
  imports: [
    CurrencyPipe,
    TableModule,
    ButtonModule,
    TranslatePipe,
    SwitchComponent,
  ],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomersComponent implements OnInit {
  private readonly customerFacade = inject(CustomerFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly customers = toSignal(this.customerFacade.customers$, {
    initialValue: [] as Customer[],
  });

  protected readonly isLoading = toSignal(this.customerFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.customerFacade.loadCustomers();
  }

  protected createCustomer(): void {
    this.customerFacade.clearSelectedCustomer();
    this.detailsSidebarService.openDrawer(CustomerFormComponent, {
      title: 'customer.form.create',
      subtitle: 'customer.form.createSubtitle',
      mode: 'create',
    });
  }

  protected editCustomer(customer: Customer): void {
    this.customerFacade.loadCustomer(customer.id);
    this.customerFacade.loadCashbackHistory(customer.id);
    this.detailsSidebarService.openDrawer(CustomerFormComponent, {
      title: 'customer.form.edit',
      subtitle: 'customer.form.subtitle',
      mode: 'edit',
    });
  }

  protected getInitials(name: string): string {
    return name
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  protected onActiveChange(customer: Customer, active: boolean): void {
    if (active === customer.isActive) {
      return;
    }

    if (active) {
      this.customerFacade.reactivateCustomer(customer.id);
    } else {
      this.customerFacade.deactivateCustomer(customer.id);
    }
  }
}
