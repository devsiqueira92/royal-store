import { createActionGroup, props } from '@ngrx/store';
import { Product } from '../interfaces/product.interface';
import { CreateServiceRequest } from '../interfaces/create-service-request.interface';
import { UpdateServiceRequest } from '../interfaces/update-service-request.interface';

// A Service is a Product on the wire (Type = Service); these actions write
// through `api/services` but land in the same `product` feature state as
// ProductActions so the catalog list/selected-item stay a single source of
// truth for both kinds.
export const ServiceActions = createActionGroup({
  source: 'Service',
  events: {
    'Create Service': props<{ request: CreateServiceRequest }>(),
    'Create Service Success': props<{ product: Product }>(),
    'Create Service Failure': props<{ error: string }>(),
    'Save Service Edit': props<{
      id: string;
      request: UpdateServiceRequest;
    }>(),
    'Update Service Success': props<{ product: Product }>(),
    'Update Service Failure': props<{ error: string }>(),
  },
});
