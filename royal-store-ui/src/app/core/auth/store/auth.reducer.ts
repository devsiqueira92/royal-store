import { createFeature, createReducer, on } from '@ngrx/store';
import { User } from '../interfaces/user.interface';
import { AuthActions } from './auth.actions';

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  isLoading: boolean;
  error: string | null;
}

function hasLoggedInCookie(): boolean {
  return document.cookie.split('; ').includes('is_logged_in=true');
}

export const initialAuthState: AuthState = {
  isAuthenticated: hasLoggedInCookie(),
  user: null,
  isLoading: false,
  error: null,
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
      user,
      isLoading: false,
    })),
    on(AuthActions.checkAuthProfileFailure, (): AuthState => ({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      error: null,
    })),
    on(AuthActions.login, (state): AuthState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AuthActions.loginSuccess, (state): AuthState => ({
      ...state,
      isAuthenticated: true,
      isLoading: false,
      error: null,
    })),
    on(AuthActions.loginFailure, (state, { error }): AuthState => ({
      ...state,
      isAuthenticated: false,
      isLoading: false,
      error: error,
    })),
    on(AuthActions.register, (state): AuthState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(AuthActions.registerSuccess, (state): AuthState => ({
      ...state,
      isLoading: false,
      error: null,
    })),
    on(AuthActions.registerFailure, (state, { error }): AuthState => ({
      ...state,
      isLoading: false,
      error: error,
    })),
    on(AuthActions.logout, (): AuthState => ({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      error: null,
    })),
  ),
});
