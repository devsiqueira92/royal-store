import {
  checkAuthProfile,
  login,
  loginSuccessProfile,
  loginSuccessRedirect,
  loginUnverifiedRedirect,
  register,
  registerSuccessRedirect,
  resendConfirmationEmail,
  resendConfirmationEmailNotify,
  verifyEmail,
} from './auth.effects';

export const authStore = {
  checkAuthProfile,
  login,
  loginSuccessProfile,
  loginSuccessRedirect,
  loginUnverifiedRedirect,
  register,
  registerSuccessRedirect,
  resendConfirmationEmail,
  resendConfirmationEmailNotify,
  verifyEmail,
};
