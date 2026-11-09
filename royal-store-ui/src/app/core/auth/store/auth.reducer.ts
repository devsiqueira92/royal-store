import { createFeature, createReducer, on } from '@ngrx/store';
import { User } from '../interfaces/user.interface';
import { AuthActions } from './auth.actions';

export type VerifyEmailStatus = 'idle' | 'verifying' | 'success' | 'error';

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
    on(AuthActions.logout, (): AuthState => ({
      ...initialAuthState,
      isAuthenticated: false,
      isAuthChecked: true,
    })),
  ),
});
