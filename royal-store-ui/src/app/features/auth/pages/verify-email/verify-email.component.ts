import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '@app-core/auth/services/auth.service';
import { NotificationService } from '@app/shared/services/notification.service';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AuthFacade } from '@app/core/auth/facade/auth.facade';
import { filter, switchMap } from 'rxjs';

type VerifyEmailStatus = 'pending' | 'verifying' | 'success' | 'error';

@Component({
  imports: [TranslatePipe, ButtonComponent],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private authFacade = inject(AuthFacade);
  private notificationService = inject(NotificationService);

  protected readonly email = signal(
    this.route.snapshot.queryParamMap.get('email') ?? '',
  );

  protected readonly status = signal<VerifyEmailStatus>('pending');
  protected readonly isResending = signal(false);

  constructor() {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (userId && token) {
      this.verifyEmail(userId, token);
    }
  }

  private verifyEmail(userId: string, token: string): void {
    this.status.set('verifying');

    this.authService.verifyEmail(userId, token).subscribe({
      next: () => this.status.set('success'),
      error: () => this.status.set('error'),
    });
  }

  protected editEmail(): void {
    this.router.navigate(['/auth/register']);
  }

  protected goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  protected resendEmail(): void {
    if (this.isResending()) {
      return;
    }

    this.isResending.set(true);

    this.authFacade.user$
      .pipe(
        filter(user => !!user),
        switchMap(user =>
          this.authService.resendConfirmationEmail({ email: user.email }),
        ),
      )
      .subscribe({
        next: response => {
          this.isResending.set(false);
          this.notificationService.success({
            summary: 'Verification email sent',
            detail: response.message,
          });
        },
        error: () => this.isResending.set(false),
      });
  }

  protected simulateVerifiedClick(): void {
    this.status.set('success');
  }
}
