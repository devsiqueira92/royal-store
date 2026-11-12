// src/app/core/auth/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthFacade } from '../facade/auth.facade';
import { combineLatest, filter, map, take } from 'rxjs';

export const authGuard: CanActivateFn = () => {
  const authFacade = inject(AuthFacade);
  const router = inject(Router);
  return combineLatest([
    authFacade.isAuthChecked$,
    authFacade.isAuthenticated$,
  ]).pipe(
    filter(([isAuthChecked]) => isAuthChecked),
    take(1),
    map(([, isAuthenticated]) => {
      if (isAuthenticated) return true;

      router.navigate(['/homepage']);
      return false;
    }),
  );
};
