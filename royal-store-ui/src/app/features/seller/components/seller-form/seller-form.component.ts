import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  Seller,
  SellerFormControls,
  SellerPermission,
  SellerRole,
} from '../../interfaces/seller.interface';
import { SellerFacade } from '../../facade/seller.facade';
import { InputFieldComponent } from '@app/shared/components/form-fields/input-field/input-field.component';
import { FormLayoutComponent } from '@app/shared/components/layouts/form-layout/form-layout.component';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';

interface RoleOption {
  value: SellerRole;
  labelKey: string;
}

interface PermissionOption {
  value: SellerPermission;
  labelKey: string;
}

const ROLE_OPTIONS: RoleOption[] = [
  { value: 'cashier', labelKey: 'seller.form.roles.cashier' },
  { value: 'manager', labelKey: 'seller.form.roles.manager' },
  { value: 'stock', labelKey: 'seller.form.roles.stock' },
];

const PERMISSION_OPTIONS: PermissionOption[] = [
  { value: 'processSales', labelKey: 'seller.form.permissions.processSales' },
  { value: 'applyDiscounts', labelKey: 'seller.form.permissions.applyDiscounts' },
  { value: 'manageProducts', labelKey: 'seller.form.permissions.manageProducts' },
  { value: 'manageSellers', labelKey: 'seller.form.permissions.manageSellers' },
];

@Component({
  selector: 'app-seller-form',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    TranslatePipe,
    InputFieldComponent,
    FormLayoutComponent,
    ButtonComponent,
  ],
  templateUrl: './seller-form.component.html',
  styleUrl: './seller-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerFormComponent {
  private readonly sellerFacade = inject(SellerFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly roleOptions = ROLE_OPTIONS;
  protected readonly permissionOptions = PERMISSION_OPTIONS;

  protected readonly selectedSeller = toSignal(
    this.sellerFacade.selectedSeller$,
    { initialValue: null as Seller | null },
  );

  protected readonly formGroup = this.buildFormGroup();

  constructor() {
    effect(() => {
      const seller = this.selectedSeller();
      if (seller) {
        this.populateForm(seller);
      } else {
        this.resetForm();
      }
    });
  }

  protected setRole(role: SellerRole): void {
    this.formGroup.controls.role.setValue(role);
  }

  protected hasPermission(permission: SellerPermission): boolean {
    return this.formGroup.controls.permissions.value.includes(permission);
  }

  protected togglePermission(permission: SellerPermission): void {
    const current = this.formGroup.controls.permissions.value;
    const next = current.includes(permission)
      ? current.filter(item => item !== permission)
      : [...current, permission];
    this.formGroup.controls.permissions.setValue(next);
  }

  protected onClose(): void {
    this.sellerFacade.clearSelectedSeller();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(event?: Event): void {
    event?.preventDefault();
    // Update is intentionally a no-op for now — no action is dispatched yet.
  }

  private populateForm(seller: Seller): void {
    this.formGroup.reset({
      id: seller.id,
      fullName: seller.fullName,
      email: seller.email ?? null,
      role: seller.role ?? null,
      permissions: seller.permissions ?? [],
    });
  }

  private resetForm(): void {
    this.formGroup.reset({
      id: null,
      fullName: null,
      email: null,
      role: null,
      permissions: [],
    });
  }

  private buildFormGroup(): FormGroup<SellerFormControls> {
    return new FormGroup<SellerFormControls>({
      id: new FormControl(null),
      fullName: new FormControl(null, [
        Validators.required,
        Validators.maxLength(120),
      ]),
      email: new FormControl(null, [Validators.required, Validators.email]),
      role: new FormControl(null),
      permissions: new FormControl<SellerPermission[]>([], {
        nonNullable: true,
      }),
    });
  }
}
