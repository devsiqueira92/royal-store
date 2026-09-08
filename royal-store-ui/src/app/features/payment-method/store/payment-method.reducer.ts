import { createFeature, createReducer, on } from '@ngrx/store';
import { PaymentMethod } from '../interfaces/payment-method.interface';
import { PaymentMethodActions } from './payment-method.actions';

export interface PaymentMethodState {
  paymentMethods: PaymentMethod[];
  selectedId: string | null;
  isLoading: boolean;
  error: string | null;
}

export const initialPaymentMethodState: PaymentMethodState = {
  paymentMethods: [],
  selectedId: null,
  isLoading: false,
  error: null,
};

const replace = (
  state: PaymentMethodState,
  paymentMethod: PaymentMethod,
): PaymentMethodState => ({
  ...state,
  paymentMethods: state.paymentMethods.map(p =>
    p.id === paymentMethod.id ? paymentMethod : p,
  ),
  isLoading: false,
});

export const paymentMethodFeature = createFeature({
  name: 'paymentMethod',
  reducer: createReducer(
    initialPaymentMethodState,
    on(
      PaymentMethodActions.loadPaymentMethods,
      (state): PaymentMethodState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),
    on(
      PaymentMethodActions.loadPaymentMethodsSuccess,
      (state, { paymentMethods }): PaymentMethodState => ({
        ...state,
        paymentMethods,
        isLoading: false,
      }),
    ),
    on(
      PaymentMethodActions.loadPaymentMethodsFailure,
      (state, { error }): PaymentMethodState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      PaymentMethodActions.selectPaymentMethod,
      (state, { id }): PaymentMethodState => ({
        ...state,
        selectedId: id,
      }),
    ),
    on(
      PaymentMethodActions.clearSelectedPaymentMethod,
      (state): PaymentMethodState => ({
        ...state,
        selectedId: null,
      }),
    ),
    on(
      PaymentMethodActions.updatePaymentDiscount,
      PaymentMethodActions.activatePaymentMethod,
      PaymentMethodActions.deactivatePaymentMethod,
      (state): PaymentMethodState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),
    on(
      PaymentMethodActions.updatePaymentDiscountFailure,
      PaymentMethodActions.activatePaymentMethodFailure,
      PaymentMethodActions.deactivatePaymentMethodFailure,
      (state, { error }): PaymentMethodState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      PaymentMethodActions.updatePaymentDiscountSuccess,
      PaymentMethodActions.activatePaymentMethodSuccess,
      PaymentMethodActions.deactivatePaymentMethodSuccess,
      (state, { paymentMethod }) => replace(state, paymentMethod),
    ),
  ),
});
