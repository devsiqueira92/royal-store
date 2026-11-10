import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoginRequest } from '../interfaces/login-request.interface';
import { RegisterRequest } from '../interfaces/register-request.interface';
import { TokenResponse } from '../interfaces/token-response.interface';
import { ResetPasswordRequest } from '../interfaces/reset-password-request.interface';
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
    'Login Unverified': props<{ email: string }>(),
    Register: props<{ request: RegisterRequest }>(),
    'Register Success': props<{ email: string }>(),
    'Register Failure': props<{ error: string }>(),
    'Verify Email': props<{ userId: string; token: string }>(),
    'Verify Email Success': emptyProps(),
    'Verify Email Failure': props<{ error: string }>(),
    'Resend Confirmation Email': props<{ email: string }>(),
    'Resend Confirmation Email Success': props<{ message: string }>(),
    'Resend Confirmation Email Failure': props<{ error: string }>(),
    'Forgot Password': props<{ email: string }>(),
    'Forgot Password Success': emptyProps(),
    'Forgot Password Failure': props<{ error: string }>(),
    'Reset Forgot Password State': emptyProps(),
    'Reset Password': props<{ request: ResetPasswordRequest }>(),
    'Reset Password Success': emptyProps(),
    'Reset Password Failure': props<{ error: string }>(),
    'Clear Reset Password Status': emptyProps(),
    Logout: emptyProps(),
  },
});
