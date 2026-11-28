import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { tap } from 'rxjs';
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
    AsyncPipe,
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

  protected readonly formGroup = this.buildFormGroup();

  /**
   * The supplier being edited; null while creating (the list clears it before opening the drawer).
   * The form is filled when a supplier arrives — once per supplier, so toggling its status (which
   * updates the same supplier in the store) never wipes what was typed.
   */
  protected readonly selectedSupplier$ = this.supplierFacade.selectedSupplier$.pipe(
    tap(supplier => {
      if (supplier && supplier.id !== this.formGroup.controls.id.value) {
        this.populateForm(supplier);
      }
    }),
  );

  /** Also true while an edited supplier is still loading, so its form can't be sent half-filled. */
  protected readonly isLoading$ = this.supplierFacade.isLoading$;

  constructor() {
    // Closed only once the API accepted the change; on failure the drawer stays open with the input.
    this.supplierFacade.saved$.pipe(takeUntilDestroyed()).subscribe(() => this.onClose());
  }

  protected onClose(): void {
    this.supplierFacade.clearSelectedSupplier();
    this.detailsSidebarService.closeDrawer();
  }

  // Active is toggled immediately through its own endpoint, never part of the submit.
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

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { id, name, cnpj, phone, address, city, zipCode } = this.formGroup.getRawValue();
    const request = {
      name: (name ?? '').trim(),
      cnpj: this.normalize(cnpj),
      phone: this.normalize(phone),
      address: this.normalize(address),
      city: this.normalize(city),
      zipCode: this.normalize(zipCode),
    };

    if (id) {
      this.supplierFacade.updateSupplier(id, request);
    } else {
      this.supplierFacade.createSupplier(request);
    }
  }

  private normalize(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private populateForm(supplier: Supplier): void {
    this.formGroup.reset({
      id: supplier.id,
      name: supplier.name,
      cnpj: supplier.cnpj,
      phone: supplier.phone,
      address: supplier.address,
      city: supplier.city,
      zipCode: supplier.zipCode,
    });
  }

  private buildFormGroup(): FormGroup<SupplierFormControls> {
    return new FormGroup<SupplierFormControls>({
      id: new FormControl(null),
      name: new FormControl(null, [Validators.required, Validators.maxLength(150)]),
      cnpj: new FormControl(null, [Validators.maxLength(14)]),
      phone: new FormControl(null, [Validators.maxLength(20)]),
      address: new FormControl(null, [Validators.maxLength(200)]),
      city: new FormControl(null, [Validators.maxLength(100)]),
      zipCode: new FormControl(null, [Validators.maxLength(10)]),
    });
  }
}
