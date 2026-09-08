import { createSelector } from '@ngrx/store';
import { paymentMethodFeature } from './payment-method.reducer';

export const {
  selectPaymentMethods,
  selectSelectedId,
  selectIsLoading,
  selectError,
} = paymentMethodFeature;

export const selectSelectedPaymentMethod = createSelector(
  selectPaymentMethods,
  selectSelectedId,
  (paymentMethods, selectedId) =>
    paymentMethods.find(p => p.id === selectedId) ?? null,
);

/** Methods a customer can actually pay with right now — everything else is catalog housekeeping. */
export const selectActivePaymentMethods = createSelector(
  selectPaymentMethods,
  paymentMethods => paymentMethods.filter(p => p.active),
);
