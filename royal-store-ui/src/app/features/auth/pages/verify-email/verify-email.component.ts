import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthFacade } from '@app-core/auth/facade/auth.facade';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AuthShellComponent } from '../../components/auth-shell/auth-shell.component';

const RESEND_COOLDOWN_SECONDS = 60;

@Component({
  imports: [TranslatePipe, ButtonComponent, AuthShellComponent],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authFacade = inject(AuthFacade);
  private destroyRef = inject(DestroyRef);

  protected readonly email = toSignal(this.authFacade.verificationEmail$, {
    initialValue: null,
  });

  protected readonly status = toSignal(this.authFacade.verifyEmailStatus$, {
    initialValue: 'idle',
  });

  protected readonly isResending = toSignal(this.authFacade.isResendingConfirmation$, {
    initialValue: false,
  });

  protected readonly cooldownRemaining = signal(0);

  private cooldownIntervalId?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (userId && token) {
      this.authFacade.verifyEmail(userId, token);
    }

    this.destroyRef.onDestroy(() => {
      if (this.cooldownIntervalId) {
        clearInterval(this.cooldownIntervalId);
      }
    });
  }

  protected editEmail(): void {
    this.router.navigate(['/auth/register']);
  }

  protected goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  protected resendEmail(): void {
    const email = this.email();

    if (!email || this.isResending() || this.cooldownRemaining() > 0) {
      return;
    }

    this.authFacade.resendConfirmationEmail(email);
    this.startCooldown();
  }

  private startCooldown(): void {
    this.cooldownRemaining.set(RESEND_COOLDOWN_SECONDS);

    this.cooldownIntervalId = setInterval(() => {
      const remaining = this.cooldownRemaining() - 1;

      if (remaining <= 0) {
        this.cooldownRemaining.set(0);
        clearInterval(this.cooldownIntervalId);
        return;
      }

      this.cooldownRemaining.set(remaining);
    }, 1000);
  }
}
