import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  CreateLabelRequest,
  Label,
  UpdateLabelRequest,
} from '../interfaces/label-format.interface';

export const LabelActions = createActionGroup({
  source: 'Label',
  events: {
    'Load Labels': props<{ isActive?: boolean }>(),
    'Load Labels Success': props<{ labels: Label[] }>(),
    'Load Labels Failure': props<{ error: string }>(),

    'Load Label': props<{ id: string }>(),
    'Load Label Success': props<{ label: Label }>(),
    'Load Label Failure': props<{ error: string }>(),

    'Create Label': props<{ request: CreateLabelRequest }>(),
    'Create Label Success': props<{ label: Label }>(),
    'Create Label Failure': props<{ error: string }>(),

    'Update Label': props<{ id: string; request: UpdateLabelRequest }>(),
    'Update Label Success': props<{ label: Label }>(),
    'Update Label Failure': props<{ error: string }>(),

    'Set Label Active': props<{ id: string; isActive: boolean }>(),
    'Set Label Active Success': props<{ id: string; isActive: boolean }>(),
    'Set Label Active Failure': props<{ error: string }>(),

    'Delete Label': props<{ id: string }>(),
    'Delete Label Success': props<{ id: string }>(),
    'Delete Label Failure': props<{ error: string }>(),

    'Select Label': props<{ id: string }>(),
    'Clear Selected Label': emptyProps(),
  },
});
