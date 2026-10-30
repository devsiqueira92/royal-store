import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { SupplierFacade } from '../../facade/supplier.facade';
import { Supplier } from '../../interfaces/supplier.interface';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { SupplierFormComponent } from '../../components/supplier-form/supplier-form.component';

@Component({
  imports: [TableModule, ButtonModule, TranslatePipe, SwitchComponent],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SuppliersComponent implements OnInit {
  private readonly supplierFacade = inject(SupplierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly suppliers = toSignal(this.supplierFacade.suppliers$, {
    initialValue: [] as Supplier[],
  });

  protected readonly isLoading = toSignal(this.supplierFacade.isLoading$, {
    initialValue: false,
  });

  ngOnInit(): void {
    this.supplierFacade.loadSuppliers();
  }

  protected createSupplier(): void {
    this.supplierFacade.clearSelectedSupplier();
    this.detailsSidebarService.openDrawer(SupplierFormComponent, {
      title: 'supplier.form.create',
      subtitle: 'supplier.form.createSubtitle',
      mode: 'create',
    });
  }

  protected editSupplier(supplier: Supplier): void {
    this.supplierFacade.loadSupplier(supplier.id);
    this.detailsSidebarService.openDrawer(SupplierFormComponent, {
      title: 'supplier.form.edit',
      subtitle: 'supplier.form.subtitle',
      mode: 'edit',
    });
  }

  protected onActiveChange(supplier: Supplier, active: boolean): void {
    if (active === supplier.isActive) {
      return;
    }

    if (active) {
      this.supplierFacade.activate(supplier.id);
    } else {
      this.supplierFacade.deactivate(supplier.id);
    }
  }
}
