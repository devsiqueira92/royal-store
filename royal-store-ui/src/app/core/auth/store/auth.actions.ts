import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoginRequest } from '../interfaces/login-request.interface';
import { TokenResponse } from '../interfaces/token-response.interface';
import { User } from '../interfaces/user.interface';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Check Auth Profile': emptyProps(),
    'Check Auth Profile Success': props<{ user: User }>(),
    'Check Auth Profile Failure': emptyProps(),
    Login: props<{ request: LoginRequest }>(),
    'Login Success': props<{ response: TokenResponse }>(),
    'Login Failure': props<{ error: string }>(),
    Logout: emptyProps(),
  },
});
