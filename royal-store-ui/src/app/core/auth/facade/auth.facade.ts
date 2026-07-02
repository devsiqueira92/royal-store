import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoginRequest } from '../interfaces/login-request.interface';
import { RegisterRequest } from '../interfaces/register-request.interface';
import { AuthActions } from '../store/auth.actions';
import {
  selectIsAuthenticated,
  selectIsLoading,
  selectError,
  selectUser,
  selectVerificationEmail,
  selectVerifyEmailStatus,
  selectIsResendingConfirmation,
} from '../store/auth.selectors';

@Injectable({
  providedIn: 'root',
})
export class AuthFacade {
  private store = inject(Store);

  isAuthenticated$ = this.store.select(selectIsAuthenticated);
  user$ = this.store.select(selectUser);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);
  verificationEmail$ = this.store.select(selectVerificationEmail);
  verifyEmailStatus$ = this.store.select(selectVerifyEmailStatus);
  isResendingConfirmation$ = this.store.select(selectIsResendingConfirmation);

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

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
