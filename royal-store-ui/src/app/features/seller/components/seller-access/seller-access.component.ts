import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { SellerRole } from '../../interfaces/seller.interface';
import { PermissionResourceGroup } from '@app/features/permission/interfaces/permission-catalog.interface';
import { ROLE_IMPLIED_PERMISSIONS } from './role-permission-rules';

interface RoleOption {
  value: SellerRole;
  labelKey: string;
}

const ROLE_OPTIONS: RoleOption[] = [
  { value: 'seller', labelKey: 'seller.form.roles.seller' },
  { value: 'manager', labelKey: 'seller.form.roles.manager' },
  { value: 'stock', labelKey: 'seller.form.roles.stock' },
];

@Component({
  selector: 'app-seller-access',
  imports: [TranslatePipe],
  templateUrl: './seller-access.component.html',
  styleUrl: './seller-access.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerAccessComponent {
  /** Full permission catalog, grouped by resource. */
  resources = input<PermissionResourceGroup[]>([]);
  /** Currently selected role. */
  role = input<SellerRole | null>(null);
  /** Explicitly granted permissions (role-implied ones are not included). */
  permissions = input<string[]>([]);

  roleChange = output<SellerRole>();
  permissionsChange = output<string[]>();

  protected readonly roleOptions = ROLE_OPTIONS;

  private readonly impliedPermissions = computed<ReadonlySet<string>>(
    () => new Set(this.role() ? ROLE_IMPLIED_PERMISSIONS[this.role()!] : []),
  );

  protected isImplied(name: string): boolean {
    return this.impliedPermissions().has(name);
  }

  protected isChecked(name: string): boolean {
    return this.isImplied(name) || this.permissions().includes(name);
  }

  protected selectRole(role: SellerRole): void {
    if (role === this.role()) {
      return;
    }

    // A permission the new role implies is no longer an explicit grant.
    const implied = new Set(ROLE_IMPLIED_PERMISSIONS[role]);
    this.roleChange.emit(role);
    this.permissionsChange.emit(
      this.permissions().filter(permission => !implied.has(permission)),
    );
  }

  protected togglePermission(name: string): void {
    if (this.isImplied(name)) {
      return;
    }

    const current = this.permissions();
    const next = current.includes(name)
      ? current.filter(permission => permission !== name)
      : [...current, name];

    this.permissionsChange.emit(next);
  }
}
