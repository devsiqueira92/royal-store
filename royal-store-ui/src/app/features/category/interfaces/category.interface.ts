import { FormControl } from '@angular/forms';

export interface Category {
  id: string;
  name: string;
  description?: string;
  color?: string;
  isActive: boolean;
  productCount: number;
}

export interface CategoryFormControls {
  id: FormControl<string | null>;
  name: FormControl<string | null>;
  description: FormControl<string | null>;
  color: FormControl<string | null>;
}
