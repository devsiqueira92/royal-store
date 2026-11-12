import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthFacade } from '@app-core/auth/facade/auth.facade';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { PasswordFieldComponent } from '../../../../shared/components/form-fields/password-field/password-field.component';
import { passwordsMatchValidator } from '@app/shared/validators/password-match.validator';
import { ResetPasswordForm } from '../../models/reset-password-form.interface';
import { AuthShellComponent } from '../../components/auth-shell/auth-shell.component';

type ResetPasswordViewStatus = 'idle' | 'submitting' | 'success' | 'error';

@Component({
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    ButtonComponent,
    PasswordFieldComponent,
    AuthShellComponent,
  ],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authFacade = inject(AuthFacade);

  protected readonly formGroup: FormGroup<ResetPasswordForm>;

  // The link itself is malformed/missing its query params — distinct from (but shown
  // the same way as) the backend rejecting a well-formed link as expired/already used.
  private readonly linkMalformed = signal(false);

  private readonly resetStatus = toSignal(this.authFacade.resetPasswordStatus$, {
    initialValue: 'idle',
  });

  protected readonly status = computed<ResetPasswordViewStatus>(() =>
    this.linkMalformed() ? 'error' : this.resetStatus(),
  );

  private email = '';
  private token = '';

  constructor() {
    this.formGroup = this.buildForm();
  }

  ngOnInit(): void {
    debugger;
    this.authFacade.clearResetPasswordStatus();

    const token = this.route.snapshot.queryParamMap.get('token');
    const email = this.route.snapshot.queryParamMap.get('email');

    if (!token || !email) {
      this.linkMalformed.set(true);
      return;
    }

    try {
      this.email = decodeBase64Url(email);
      this.token = token;
    } catch {
      this.linkMalformed.set(true);
    }
  }

  protected onSubmit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }

    const { password, confirmPassword } = this.formGroup.getRawValue();

    this.authFacade.resetPassword({
      email: this.email,
      token: this.token,
      newPassword: password!,
      confirmPassword: confirmPassword!,
    });
  }

  protected goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  protected requestNewLink(): void {
    this.router.navigate(['/auth/forgot-password']);
  }

  private buildForm(): FormGroup<ResetPasswordForm> {
    return new FormGroup<ResetPasswordForm>(
      {
        password: new FormControl(null, [Validators.required, Validators.minLength(8)]),
        confirmPassword: new FormControl(null, [Validators.required]),
      },
      { validators: passwordsMatchValidator() },
    );
  }
}

// Mirrors Base64UrlEncode(Encoding.UTF8.GetBytes(email)) in ForgotPasswordCommandHandler —
// restore the '+'/'/' alphabet and padding atob() expects, then read the bytes it decodes
// to as UTF-8 (atob alone returns a Latin-1 string, which mangles non-ASCII characters).
function decodeBase64Url(value: string): string {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
  const binary = atob(padded);
  const bytes = Uint8Array.from(binary, char => char.charCodeAt(0));
  return new TextDecoder().decode(bytes);
}
