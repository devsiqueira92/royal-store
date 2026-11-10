import { createFeature, createReducer, on } from '@ngrx/store';
import { User } from '../interfaces/user.interface';
import { AuthActions } from './auth.actions';

export type VerifyEmailStatus = 'idle' | 'verifying' | 'success' | 'error';
export type ForgotPasswordStatus = 'idle' | 'sending' | 'sent' | 'error';
export type ResetPasswordStatus = 'idle' | 'submitting' | 'success' | 'error';

export interface AuthState {
  isAuthenticated: boolean;
  // Whether the bootstrap session check (checkAuthProfile) has resolved yet.
  // The real session lives in an httpOnly cookie scoped to the API's own
  // domain, so the frontend (a different origin) can't read it directly and
  // must ask the API via `/session` instead of guessing from document.cookie.
  isAuthChecked: boolean;
  user: User | null;
  isLoading: boolean;
  error: string | null;
  verificationEmail: string | null;
  verifyEmailStatus: VerifyEmailStatus;
  isResendingConfirmation: boolean;
  forgotPasswordStatus: ForgotPasswordStatus;
  forgotPasswordError: string | null;
  resetPasswordStatus: ResetPasswordStatus;
  resetPasswordError: string | null;
}

export const initialAuthState: AuthState = {
  isAuthenticated: false,
  isAuthChecked: false,
  user: null,
  isLoading: false,
  error: null,
  verificationEmail: null,
  verifyEmailStatus: 'idle',
  isResendingConfirmation: false,
  forgotPasswordStatus: 'idle',
  forgotPasswordError: null,
  resetPasswordStatus: 'idle',
  resetPasswordError: null,
};

export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(
    initialAuthState,
    on(AuthActions.checkAuthProfile, (state): AuthState => ({
      ...state,
      isLoading: true,
    })),
    on(AuthActions.checkAuthProfileSuccess, (state, { user }): AuthState => ({
      ...state,
      isAuthenticated: true,
      isAuthChecked: true,
      user,
      isLoading: false,
    })),
    on(AuthActions.checkAuthProfileFailure, (): AuthState => ({
      ...initialAuthState,
      isAuthenticated: false,
      isAuthChecked: true,
    })),
    on(AuthActions.login, (state): AuthState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AuthActions.loginSuccess, (state): AuthState => ({
      ...state,
      isAuthenticated: true,
      isAuthChecked: true,
      isLoading: false,
      error: null,
    })),
    on(AuthActions.loginFailure, (state, { error }): AuthState => ({
      ...state,
      isAuthenticated: false,
      isLoading: false,
      error: error,
    })),
    on(AuthActions.loginUnverified, (state, { email }): AuthState => ({
      ...state,
      isAuthenticated: false,
      isLoading: false,
      error: null,
      verificationEmail: email,
      verifyEmailStatus: 'idle',
    })),
    on(AuthActions.register, (state): AuthState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AuthActions.registerSuccess, (state, { email }): AuthState => ({
      ...state,
      isLoading: false,
      error: null,
      verificationEmail: email,
      verifyEmailStatus: 'idle',
    })),
    on(AuthActions.registerFailure, (state, { error }): AuthState => ({
      ...state,
      isLoading: false,
      error: error,
    })),
    on(AuthActions.verifyEmail, (state): AuthState => ({
      ...state,
      verifyEmailStatus: 'verifying',
    })),
    on(AuthActions.verifyEmailSuccess, (state): AuthState => ({
      ...state,
      verifyEmailStatus: 'success',
      verificationEmail: null,
    })),
    on(AuthActions.verifyEmailFailure, (state): AuthState => ({
      ...state,
      verifyEmailStatus: 'error',
    })),
    on(AuthActions.resendConfirmationEmail, (state): AuthState => ({
      ...state,
      isResendingConfirmation: true,
    })),
    on(AuthActions.resendConfirmationEmailSuccess, (state): AuthState => ({
      ...state,
      isResendingConfirmation: false,
    })),
    on(AuthActions.resendConfirmationEmailFailure, (state): AuthState => ({
      ...state,
      isResendingConfirmation: false,
    })),
    on(AuthActions.forgotPassword, (state): AuthState => ({
      ...state,
      forgotPasswordStatus: 'sending',
      forgotPasswordError: null,
    })),
    on(AuthActions.forgotPasswordSuccess, (state): AuthState => ({
      ...state,
      forgotPasswordStatus: 'sent',
    })),
    on(AuthActions.forgotPasswordFailure, (state, { error }): AuthState => ({
      ...state,
      forgotPasswordStatus: 'error',
      forgotPasswordError: error,
    })),
    on(AuthActions.resetForgotPasswordState, (state): AuthState => ({
      ...state,
      forgotPasswordStatus: 'idle',
      forgotPasswordError: null,
    })),
    on(AuthActions.resetPassword, (state): AuthState => ({
      ...state,
      resetPasswordStatus: 'submitting',
      resetPasswordError: null,
    })),
    on(AuthActions.resetPasswordSuccess, (state): AuthState => ({
      ...state,
      resetPasswordStatus: 'success',
    })),
    on(AuthActions.resetPasswordFailure, (state, { error }): AuthState => ({
      ...state,
      resetPasswordStatus: 'error',
      resetPasswordError: error,
    })),
    on(AuthActions.clearResetPasswordStatus, (state): AuthState => ({
      ...state,
      resetPasswordStatus: 'idle',
      resetPasswordError: null,
    })),
    on(AuthActions.logout, (): AuthState => ({
      ...initialAuthState,
      isAuthenticated: false,
      isAuthChecked: true,
    })),
  ),
});
