import {
  checkAuthProfile,
  login,
  loginSuccessProfile,
  loginSuccessRedirect,
  register,
  registerSuccessRedirect,
} from './auth.effects';

export const authStore = {
  checkAuthProfile,
  login,
  loginSuccessProfile,
  loginSuccessRedirect,
  register,
  registerSuccessRedirect,
};
