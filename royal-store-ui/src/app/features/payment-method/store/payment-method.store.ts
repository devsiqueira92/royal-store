import {
  loadPaymentMethods,
  updatePaymentDiscount,
  activatePaymentMethod,
  deactivatePaymentMethod,
} from './payment-method.effects';

export const paymentMethodStore = {
  loadPaymentMethods,
  updatePaymentDiscount,
  activatePaymentMethod,
  deactivatePaymentMethod,
};
