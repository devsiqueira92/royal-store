import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, take } from 'rxjs';
import { SaleFacade } from '../facade/sale.facade';

/** Checkout needs something to charge: an empty cart (a reload, a bookmarked URL, the back button
 * after a finished sale) is sent back to the POS. */
export const saleCartNotEmptyGuard: CanActivateFn = () => {
  const router = inject(Router);

  return inject(SaleFacade).cartItems$.pipe(
    take(1),
    map(items => (items.length > 0 ? true : router.createUrlTree(['/sales']))),
  );
};
