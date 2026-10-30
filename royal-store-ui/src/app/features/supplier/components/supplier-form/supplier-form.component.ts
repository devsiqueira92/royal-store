import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { SupplierFacade } from '../../facade/supplier.facade';
import { Supplier, SupplierFormControls } from '../../interfaces/supplier.interface';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { SwitchComponent } from '@app/shared/components/switch/switch.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

@Component({
  selector: 'app-supplier-form',
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    SwitchComponent,
  ],
  templateUrl: './supplier-form.component.html',
  styleUrl: './supplier-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SupplierFormComponent {
  private readonly supplierFacade = inject(SupplierFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () => this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly selectedSupplier = toSignal(this.supplierFacade.selectedSupplier$, {
    initialValue: null as Supplier | null,
  });

  // Active is toggled immediately through its own endpoint, never part of the submit.
  protected readonly active = computed(() => this.selectedSupplier()?.isActive ?? false);

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      this.populateForm(this.selectedSupplier());
    });
  }

  protected onClose(): void {
    this.supplierFacade.clearSelectedSupplier();
    this.detailsSidebarService.closeDrawer();
  }

  protected onActiveChange(active: boolean): void {
    const supplier = this.selectedSupplier();
    if (!supplier || active === supplier.isActive) {
      return;
    }

    if (active) {
      this.supplierFacade.activate(supplier.id);
    } else {
      this.supplierFacade.deactivate(supplier.id);
    }
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { name, cnpj, phone, address, city, zipCode } = this.formGroup.getRawValue();
    const request = {
      name: (name ?? '').trim(),
      cnpj: this.normalize(cnpj),
      phone: this.normalize(phone),
      address: this.normalize(address),
      city: this.normalize(city),
      zipCode: this.normalize(zipCode),
    };

    if (this.isCreateMode()) {
      this.supplierFacade.createSupplier(request);
    } else {
      const supplier = this.selectedSupplier();
      if (!supplier) {
        return;
      }

      this.supplierFacade.updateSupplier(supplier.id, request);
    }

    this.onClose();
  }

  private normalize(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private enterCreateMode(): void {
    this.formGroup.reset({
      name: null,
      cnpj: null,
      phone: null,
      address: null,
      city: null,
      zipCode: null,
    });
  }

  private populateForm(supplier: Supplier | null): void {
    this.formGroup.reset({
      name: supplier?.name ?? null,
      cnpj: supplier?.cnpj ?? null,
      phone: supplier?.phone ?? null,
      address: supplier?.address ?? null,
      city: supplier?.city ?? null,
      zipCode: supplier?.zipCode ?? null,
    });
  }

  private buildFormGroup(): FormGroup<SupplierFormControls> {
    return new FormGroup<SupplierFormControls>({
      name: new FormControl(null, [Validators.required, Validators.maxLength(150)]),
      cnpj: new FormControl(null, [Validators.maxLength(14)]),
      phone: new FormControl(null, [Validators.maxLength(20)]),
      address: new FormControl(null, [Validators.maxLength(200)]),
      city: new FormControl(null, [Validators.maxLength(100)]),
      zipCode: new FormControl(null, [Validators.maxLength(10)]),
    });
  }
}
