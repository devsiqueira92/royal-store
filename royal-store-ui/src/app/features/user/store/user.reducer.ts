import { createFeature, createReducer, on } from '@ngrx/store';
import { CreateUserResponse } from '../interfaces/create-user-response.interface';
import { UserActions } from './user.actions';

export interface UserState {
  isLoading: boolean;
  error: string | null;
  created: CreateUserResponse | null;
}

export const initialUserState: UserState = {
  isLoading: false,
  error: null,
  created: null,
};

export const userFeature = createFeature({
  name: 'user',
  reducer: createReducer(
    initialUserState,
    on(UserActions.createUser, (state): UserState => ({
      ...state,
      isLoading: true,
      error: null,
      created: null,
    })),
    on(UserActions.createUserSuccess, (state, { response }): UserState => ({
      ...state,
      isLoading: false,
      created: response,
    })),
    on(UserActions.createUserFailure, (state, { error }): UserState => ({
      ...state,
      isLoading: false,
      error,
    })),
  ),
});
