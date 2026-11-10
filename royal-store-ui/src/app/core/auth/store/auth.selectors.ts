import { authFeature } from './auth.reducer';

export const {
  selectIsAuthenticated,
  selectIsAuthChecked,
  selectUser,
  selectIsLoading,
  selectError,
  selectVerificationEmail,
  selectVerifyEmailStatus,
  selectIsResendingConfirmation,
  selectForgotPasswordStatus,
  selectForgotPasswordError,
  selectResetPasswordStatus,
  selectResetPasswordError,
} = authFeature;
