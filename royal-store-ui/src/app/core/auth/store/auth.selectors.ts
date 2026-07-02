import { authFeature } from './auth.reducer';

export const {
  selectIsAuthenticated,
  selectUser,
  selectIsLoading,
  selectError,
} = authFeature;
