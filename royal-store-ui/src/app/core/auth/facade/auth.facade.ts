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

  checkProfile(): void {
    this.store.dispatch(AuthActions.checkAuthProfile());
  }

  login(request: LoginRequest): void {
    this.store.dispatch(AuthActions.login({ request }));
  }

  register(request: RegisterRequest): void {
    this.store.dispatch(AuthActions.register({ request }));
  }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
