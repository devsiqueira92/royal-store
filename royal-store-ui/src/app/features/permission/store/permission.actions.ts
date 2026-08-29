import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { PermissionResourceGroup } from '../interfaces/permission-catalog.interface';

export const PermissionActions = createActionGroup({
  source: 'Permission',
  events: {
    'Load Permission Catalog': emptyProps(),
    'Load Permission Catalog Success': props<{
      resources: PermissionResourceGroup[];
    }>(),
    'Load Permission Catalog Failure': props<{ error: string }>(),
  },
});
