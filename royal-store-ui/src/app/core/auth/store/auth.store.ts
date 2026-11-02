import {
  checkAuthProfile,
  login,
  loginSuccessProfile,
  loginSuccessRedirect,
  loginUnverifiedRedirect,
  logout,
  logoutRedirect,
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
  logout,
  logoutRedirect,
  register,
  registerSuccessRedirect,
  resendConfirmationEmail,
  resendConfirmationEmailNotify,
  verifyEmail,
};
