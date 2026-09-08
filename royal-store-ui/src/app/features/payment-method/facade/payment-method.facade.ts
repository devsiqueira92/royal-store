import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { UpdatePaymentDiscountRequest } from '../interfaces/update-payment-discount-request.interface';
import { PaymentMethodActions } from '../store/payment-method.actions';
import {
  selectPaymentMethods,
  selectSelectedPaymentMethod,
  selectActivePaymentMethods,
  selectIsLoading,
  selectError,
} from '../store/payment-method.selectors';

@Injectable({
  providedIn: 'root',
})
export class PaymentMethodFacade {
  private store = inject(Store);

  paymentMethods$ = this.store.select(selectPaymentMethods);
  activePaymentMethods$ = this.store.select(selectActivePaymentMethods);
  selectedPaymentMethod$ = this.store.select(selectSelectedPaymentMethod);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  loadPaymentMethods(): void {
    this.store.dispatch(PaymentMethodActions.loadPaymentMethods());
  }

  selectPaymentMethod(id: string): void {
    this.store.dispatch(PaymentMethodActions.selectPaymentMethod({ id }));
  }

  clearSelectedPaymentMethod(): void {
    this.store.dispatch(PaymentMethodActions.clearSelectedPaymentMethod());
  }

  updateDiscount(id: string, request: UpdatePaymentDiscountRequest): void {
    this.store.dispatch(
      PaymentMethodActions.updatePaymentDiscount({ id, request }),
    );
  }

  activate(id: string): void {
    this.store.dispatch(PaymentMethodActions.activatePaymentMethod({ id }));
  }

  deactivate(id: string): void {
    this.store.dispatch(PaymentMethodActions.deactivatePaymentMethod({ id }));
  }
}
