import { FormControl } from '@angular/forms';

export interface ResetPasswordForm {
  password: FormControl<string | null>;
  confirmPassword: FormControl<string | null>;
}
