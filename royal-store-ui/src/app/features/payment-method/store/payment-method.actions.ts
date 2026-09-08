import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { PaymentMethod } from '../interfaces/payment-method.interface';
import { UpdatePaymentDiscountRequest } from '../interfaces/update-payment-discount-request.interface';

export const PaymentMethodActions = createActionGroup({
  source: 'PaymentMethod',
  events: {
    'Load Payment Methods': emptyProps(),
    'Load Payment Methods Success': props<{
      paymentMethods: PaymentMethod[];
    }>(),
    'Load Payment Methods Failure': props<{ error: string }>(),
    'Select Payment Method': props<{ id: string }>(),
    'Clear Selected Payment Method': emptyProps(),
    'Update Payment Discount': props<{
      id: string;
      request: UpdatePaymentDiscountRequest;
    }>(),
    'Update Payment Discount Success': props<{
      paymentMethod: PaymentMethod;
    }>(),
    'Update Payment Discount Failure': props<{ error: string }>(),
    'Activate Payment Method': props<{ id: string }>(),
    'Activate Payment Method Success': props<{
      paymentMethod: PaymentMethod;
    }>(),
    'Activate Payment Method Failure': props<{ error: string }>(),
    'Deactivate Payment Method': props<{ id: string }>(),
    'Deactivate Payment Method Success': props<{
      paymentMethod: PaymentMethod;
    }>(),
    'Deactivate Payment Method Failure': props<{ error: string }>(),
  },
});
