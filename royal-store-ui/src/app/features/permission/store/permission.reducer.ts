import { createFeature, createReducer, on } from '@ngrx/store';
import { PermissionResourceGroup } from '../interfaces/permission-catalog.interface';
import { PermissionActions } from './permission.actions';

export interface PermissionState {
  resources: PermissionResourceGroup[];
  isLoading: boolean;
  error: string | null;
}

export const initialPermissionState: PermissionState = {
  resources: [],
  isLoading: false,
  error: null,
};

export const permissionFeature = createFeature({
  name: 'permission',
  reducer: createReducer(
    initialPermissionState,
    on(PermissionActions.loadPermissionCatalog, (state): PermissionState => ({
      ...state,
      isLoading: true,
      error: null,
    })),
    on(
      PermissionActions.loadPermissionCatalogSuccess,
      (state, { resources }): PermissionState => ({
        ...state,
        resources,
        isLoading: false,
      }),
    ),
    on(
      PermissionActions.loadPermissionCatalogFailure,
      (state, { error }): PermissionState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
  ),
});
