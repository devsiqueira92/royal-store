import { createActionGroup, props } from '@ngrx/store';
import { CreateUserRequest } from '../interfaces/create-user-request.interface';
import { CreateUserResponse } from '../interfaces/create-user-response.interface';

export const UserActions = createActionGroup({
  source: 'User',
  events: {
    'Create User': props<{ request: CreateUserRequest }>(),
    'Create User Success': props<{ response: CreateUserResponse }>(),
    'Create User Failure': props<{ error: string }>(),
  },
});
