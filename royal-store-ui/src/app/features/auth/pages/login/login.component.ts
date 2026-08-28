import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthFacade } from '@app-core/auth/facade/auth.facade';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AsyncPipe } from '@angular/common';
import { InputFieldComponent } from '../../../../shared/components/form-fields/input-field/input-field.component';
import { PasswordFieldComponent } from '../../../../shared/components/form-fields/password-field/password-field.component';
import { LinkComponent } from '../../../../shared/components/form-fields/link/link.component';
import { TranslatePipe } from '@ngx-translate/core';
import { DividerModule } from 'primeng/divider';
import { LoginForm } from '../../models/login-form.interface';

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    MessageModule,
    ButtonComponent,
    AsyncPipe,
    InputFieldComponent,
    PasswordFieldComponent,
    LinkComponent,
    TranslatePipe,
    DividerModule,
  ],
  templateUrl: 'login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private authFacade = inject(AuthFacade);

  protected readonly formGroup: FormGroup<LoginForm>;
  protected readonly isLoading$ = this.authFacade.isLoading$;
  protected readonly error$ = this.authFacade.error$;

  constructor() {
    this.formGroup = this.buildForm();
  }

  private buildForm(): FormGroup<LoginForm> {
    return new FormGroup<LoginForm>({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [Validators.required]),
    });
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { email, password } = this.formGroup.getRawValue();

    this.authFacade.login({ identifier: email!, password: password! });
  }
}
