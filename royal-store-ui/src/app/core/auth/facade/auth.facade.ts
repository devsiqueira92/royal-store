import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoginRequest } from '../interfaces/login-request.interface';
import { RegisterRequest } from '../interfaces/register-request.interface';
import { ResetPasswordRequest } from '../interfaces/reset-password-request.interface';
import { AuthActions } from '../store/auth.actions';
import {
  selectIsAuthenticated,
  selectIsAuthChecked,
  selectIsLoading,
  selectError,
  selectUser,
  selectVerificationEmail,
  selectVerifyEmailStatus,
  selectIsResendingConfirmation,
  selectForgotPasswordStatus,
  selectForgotPasswordError,
  selectResetPasswordStatus,
  selectResetPasswordError,
} from '../store/auth.selectors';

@Injectable({
  providedIn: 'root',
})
export class AuthFacade {
  private store = inject(Store);

  isAuthenticated$ = this.store.select(selectIsAuthenticated);
  isAuthChecked$ = this.store.select(selectIsAuthChecked);
  user$ = this.store.select(selectUser);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);
  verificationEmail$ = this.store.select(selectVerificationEmail);
  verifyEmailStatus$ = this.store.select(selectVerifyEmailStatus);
  isResendingConfirmation$ = this.store.select(selectIsResendingConfirmation);
  forgotPasswordStatus$ = this.store.select(selectForgotPasswordStatus);
  forgotPasswordError$ = this.store.select(selectForgotPasswordError);
  resetPasswordStatus$ = this.store.select(selectResetPasswordStatus);
  resetPasswordError$ = this.store.select(selectResetPasswordError);

  checkProfile(): void {
    this.store.dispatch(AuthActions.checkAuthProfile());
  }

  login(request: LoginRequest): void {
    this.store.dispatch(AuthActions.login({ request }));
  }

  register(request: RegisterRequest): void {
    this.store.dispatch(AuthActions.register({ request }));
  }

  verifyEmail(userId: string, token: string): void {
    this.store.dispatch(AuthActions.verifyEmail({ userId, token }));
  }

  resendConfirmationEmail(email: string): void {
    this.store.dispatch(AuthActions.resendConfirmationEmail({ email }));
  }

  forgotPassword(email: string): void {
    this.store.dispatch(AuthActions.forgotPassword({ email }));
  }

  resetForgotPasswordState(): void {
    this.store.dispatch(AuthActions.resetForgotPasswordState());
  }

  resetPassword(request: ResetPasswordRequest): void {
    this.store.dispatch(AuthActions.resetPassword({ request }));
  }

  clearResetPasswordStatus(): void {
    this.store.dispatch(AuthActions.clearResetPasswordStatus());
  }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
