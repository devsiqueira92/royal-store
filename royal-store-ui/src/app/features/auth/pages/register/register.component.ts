import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AsyncPipe } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthFacade } from '@app-core/auth/facade/auth.facade';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputFieldComponent } from '../../../../shared/components/form-fields/input-field/input-field.component';
import { PasswordFieldComponent } from '../../../../shared/components/form-fields/password-field/password-field.component';
import { LinkComponent } from '../../../../shared/components/form-fields/link/link.component';
import { RegisterRequest } from '@app/core/auth/interfaces/register-request.interface';
import { passwordsMatchValidator } from '@app/shared/validators/password-match.validator';
import { RegisterForm } from '../../models/register-form.interface';
import { DocumentType } from '@app/shared/interfaces/document-type.enum';

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MessageModule,
    SelectButtonModule,
    ButtonComponent,
    AsyncPipe,
    InputFieldComponent,
    PasswordFieldComponent,
    LinkComponent,
    TranslatePipe,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {
  private authFacade = inject(AuthFacade);

  protected readonly formGroup: FormGroup<RegisterForm>;

  protected readonly documentTypeOptions = [
    { label: 'CNPJ', value: DocumentType.CNPJ },
    { label: 'CPF', value: DocumentType.CPF },
  ];

  protected readonly isLoading$ = this.authFacade.isLoading$;

  protected readonly registerError$ = this.authFacade.error$;

  constructor() {
    this.formGroup = this.buildForm();
  }

  private buildForm(): FormGroup<RegisterForm> {
    return new FormGroup<RegisterForm>(
      {
        email: new FormControl(null, [Validators.required, Validators.email]),
        document: new FormControl(null, [Validators.required]),
        documentType: new FormControl(DocumentType.CNPJ, [Validators.required]),
        storeName: new FormControl(null, [Validators.required]),
        password: new FormControl(null, [
          Validators.required,
          Validators.minLength(8),
        ]),
        confirmPassword: new FormControl(null, [Validators.required]),
      },
      { validators: passwordsMatchValidator() },
    );
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    this.authFacade.register(this.formGroup.value as RegisterRequest);
  }
}
