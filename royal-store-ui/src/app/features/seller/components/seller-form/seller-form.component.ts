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
import {
  Seller,
  SellerFormControls,
  SellerRole,
} from '../../interfaces/seller.interface';
import { SellerFacade } from '../../facade/seller.facade';
import { PermissionFacade } from '@app/features/permission/facade/permission.facade';
import { PermissionResourceGroup } from '@app/features/permission/interfaces/permission-catalog.interface';
import { UserFacade } from '@app/features/user/facade/user.facade';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import { SellerAccessComponent } from '../seller-access/seller-access.component';
import { stripImplied } from '../seller-access/role-permission-rules';

@Component({
  selector: 'app-seller-form',
  imports: [
    ReactiveFormsModule,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
    SellerAccessComponent,
  ],
  templateUrl: './seller-form.component.html',
  styleUrl: './seller-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerFormComponent {
  private readonly sellerFacade = inject(SellerFacade);
  private readonly permissionFacade = inject(PermissionFacade);
  private readonly userFacade = inject(UserFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly isCreateMode = computed(
    () =>
      this.detailsSidebarService.contentToRender()?.props?.mode === 'create',
  );

  protected readonly permissionResources = toSignal(
    this.permissionFacade.resources$,
    { initialValue: [] as PermissionResourceGroup[] },
  );

  protected readonly selectedSeller = toSignal(
    this.sellerFacade.selectedSeller$,
    { initialValue: null as Seller | null },
  );

  protected readonly formGroup = this.buildFormGroup();

  // Bridge the reactive controls the access component reads to signals so its
  // inputs track resets and programmatic updates.
  protected readonly role = toSignal(this.formGroup.controls.role.valueChanges, {
    initialValue: this.formGroup.controls.role.value,
  });
  protected readonly permissions = toSignal(
    this.formGroup.controls.permissions.valueChanges,
    { initialValue: this.formGroup.controls.permissions.value },
  );

  constructor() {
    this.permissionFacade.loadCatalog();

    effect(() => {
      if (this.isCreateMode()) {
        this.enterCreateMode();
        return;
      }

      const seller = this.selectedSeller();
      this.populateForm(seller);
      // if (seller) {
      //   this.populateForm(seller);
      // } else {
      //   this.resetForm();
      // }
    });
  }

  protected onRoleChange(role: SellerRole): void {
    this.formGroup.controls.role.setValue(role);
  }

  protected onPermissionsChange(permissions: string[]): void {
    this.formGroup.controls.permissions.setValue(permissions);
  }

  protected onClose(): void {
    this.sellerFacade.clearSelectedSeller();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    if (this.isCreateMode()) {
      const { firstName, lastName } = this.formGroup.getRawValue();
      this.userFacade.createUser({
        firstName: (firstName ?? '').trim(),
        lastName: (lastName ?? '').trim(),
      });
      this.detailsSidebarService.closeDrawer();
      this.sellerFacade.loadSellers();

      return;
    }

    const seller = this.selectedSeller();
    if (!seller) {
      return;
    }

    const { role, permissions } = this.formGroup.getRawValue();
    this.sellerFacade.updateSellerPermissions(seller.id, { role, permissions });

    this.sellerFacade.clearSelectedSeller();
    this.detailsSidebarService.closeDrawer();
  }

  private enterCreateMode(): void {
    this.formGroup.reset({
      id: null,
      fullName: null,
      firstName: null,
      lastName: null,
      email: null,
      role: null,
      permissions: [],
    });
    this.formGroup.controls.firstName.enable();
    this.formGroup.controls.lastName.enable();
  }

  private populateForm(seller: Seller | null): void {
    // The API returns the role capitalized (e.g. "Seller"); the form options
    // are lowercase.
    const role = seller?.role
      ? (seller?.role.toLowerCase() as SellerRole)
      : null;

    this.formGroup.controls.firstName.disable();
    this.formGroup.controls.lastName.disable();
    this.formGroup.reset({
      id: seller?.id,
      fullName: seller?.fullName,
      firstName: null,
      lastName: null,
      email: seller?.email ?? null,
      role,
      permissions: stripImplied(role, seller?.permissions ?? []),
    });
  }

  private resetForm(): void {
    this.formGroup.controls.firstName.disable();
    this.formGroup.controls.lastName.disable();
    this.formGroup.reset({
      id: null,
      fullName: null,
      firstName: null,
      lastName: null,
      email: null,
      role: null,
      permissions: [],
    });
  }

  private buildFormGroup(): FormGroup<SellerFormControls> {
    return new FormGroup<SellerFormControls>({
      id: new FormControl(null),
      fullName: new FormControl({ value: null, disabled: true }),
      firstName: new FormControl(
        { value: null, disabled: true },
        [Validators.required, Validators.maxLength(50)],
      ),
      lastName: new FormControl(
        { value: null, disabled: true },
        [Validators.required, Validators.maxLength(50)],
      ),
      email: new FormControl({ value: null, disabled: true }),
      role: new FormControl(null),
      permissions: new FormControl<string[]>([], {
        nonNullable: true,
      }),
    });
  }
}
