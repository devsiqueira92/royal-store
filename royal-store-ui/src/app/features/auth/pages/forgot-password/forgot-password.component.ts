import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AsyncPipe } from '@angular/common';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MessageModule } from 'primeng/message';
import { AuthFacade } from '@app-core/auth/facade/auth.facade';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputFieldComponent } from '../../../../shared/components/form-fields/input-field/input-field.component';
import { LinkComponent } from '../../../../shared/components/form-fields/link/link.component';
import { ForgotPasswordForm } from '../../models/forgot-password-form.interface';
import { AuthShellComponent } from '../../components/auth-shell/auth-shell.component';

@Component({
  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    TranslatePipe,
    MessageModule,
    ButtonComponent,
    InputFieldComponent,
    LinkComponent,
    AuthShellComponent,
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordComponent implements OnInit {
  private authFacade = inject(AuthFacade);
  private router = inject(Router);

  protected readonly formGroup: FormGroup<ForgotPasswordForm>;
  protected readonly status$ = this.authFacade.forgotPasswordStatus$;
  protected readonly error$ = this.authFacade.forgotPasswordError$;
  protected readonly submittedEmail = signal<string | null>(null);

  constructor() {
    this.formGroup = this.buildForm();
  }

  ngOnInit(): void {
    this.authFacade.resetForgotPasswordState();
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { email } = this.formGroup.getRawValue();

    this.submittedEmail.set(email);
    this.authFacade.forgotPassword(email!);
  }

  protected tryAnotherEmail(): void {
    this.formGroup.reset();
    this.submittedEmail.set(null);
    this.authFacade.resetForgotPasswordState();
  }

  protected goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  private buildForm(): FormGroup<ForgotPasswordForm> {
    return new FormGroup<ForgotPasswordForm>({
      email: new FormControl(null, [Validators.required, Validators.email]),
    });
  }
}
