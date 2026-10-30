import { FormControl } from '@angular/forms';

export interface Supplier {
  id: string;
  tenantId: string;
  name: string;
  cnpj: string | null;
  phone: string | null;
  address: string | null;
  city: string | null;
  zipCode: string | null;
  isActive: boolean;
}

export interface SupplierFormControls {
  name: FormControl<string | null>;
  cnpj: FormControl<string | null>;
  phone: FormControl<string | null>;
  address: FormControl<string | null>;
  city: FormControl<string | null>;
  zipCode: FormControl<string | null>;
}
