import { FormControl } from '@angular/forms';
import { DocumentType } from '@app/shared/interfaces/document-type.enum';

export interface RegisterForm {
  email: FormControl<string | null>;
  document: FormControl<string | null>;
  documentType: FormControl<DocumentType | null>;
  storeName: FormControl<string | null>;
  password: FormControl<string | null>;
  confirmPassword: FormControl<string | null>;
}
