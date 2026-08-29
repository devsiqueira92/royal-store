import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { PermissionActions } from '../store/permission.actions';
import {
  selectResources,
  selectIsLoading,
  selectError,
} from '../store/permission.selectors';

@Injectable({
  providedIn: 'root',
})
export class PermissionFacade {
  private store = inject(Store);

  resources$ = this.store.select(selectResources);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadCatalog(): void {
    this.store.dispatch(PermissionActions.loadPermissionCatalog());
  }
}
