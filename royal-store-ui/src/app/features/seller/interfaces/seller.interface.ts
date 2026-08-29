import { FormControl } from '@angular/forms';

export type SellerRole = 'seller' | 'manager' | 'stock';

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
  permissions?: string[];
}

export interface SellerFormControls {
  id: FormControl<string | null>;
  fullName: FormControl<string | null>;
  email: FormControl<string | null>;
  role: FormControl<SellerRole | null>;
  permissions: FormControl<string[]>;
}
