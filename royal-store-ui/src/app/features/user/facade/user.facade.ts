import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateUserRequest } from '../interfaces/create-user-request.interface';
import { UserActions } from '../store/user.actions';
import {
  selectIsLoading,
  selectError,
  selectCreated,
} from '../store/user.selectors';

@Injectable({
  providedIn: 'root',
})
export class UserFacade {
  private store = inject(Store);

  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);
  created$ = this.store.select(selectCreated);

  createUser(request: CreateUserRequest): void {
    this.store.dispatch(UserActions.createUser({ request }));
  }
}
