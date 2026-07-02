import { authFeature } from './auth.reducer';

export const {
  selectIsAuthenticated,
  selectUser,
  selectIsLoading,
  selectError,
  selectVerificationEmail,
  selectVerifyEmailStatus,
  selectIsResendingConfirmation,
} = authFeature;
