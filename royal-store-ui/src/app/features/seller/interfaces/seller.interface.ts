import { FormControl } from '@angular/forms';

export type SellerRole = 'cashier' | 'manager' | 'stock';

export type SellerPermission =
  | 'processSales'
  | 'applyDiscounts'
  | 'manageProducts'
  | 'manageSellers';

export interface Seller {
  id: string;
  tenantId: string;
  fullName: string;
  phone: string;
  cpf: string;
  isActive: boolean;
  userId?: string;
  email?: string;
  role?: SellerRole;
  permissions?: SellerPermission[];
}

export interface SellerFormControls {
  id: FormControl<string | null>;
  fullName: FormControl<string | null>;
  email: FormControl<string | null>;
  role: FormControl<SellerRole | null>;
  permissions: FormControl<SellerPermission[]>;
}
