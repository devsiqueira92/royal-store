import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Supplier } from '../interfaces/supplier.interface';
import { SupplierRequest } from '../interfaces/supplier-request.interface';

export const SupplierActions = createActionGroup({
  source: 'Supplier',
  events: {
    'Load Suppliers': emptyProps(),
    'Load Suppliers Success': props<{ suppliers: Supplier[] }>(),
    'Load Suppliers Failure': props<{ error: string }>(),

    'Load Supplier': props<{ id: string }>(),
    'Load Supplier Success': props<{ supplier: Supplier }>(),
    'Load Supplier Failure': props<{ error: string }>(),

    'Create Supplier': props<{ request: SupplierRequest }>(),
    'Create Supplier Success': props<{ supplier: Supplier }>(),
    'Create Supplier Failure': props<{ error: string }>(),

    'Update Supplier': props<{ id: string; request: SupplierRequest }>(),
    'Update Supplier Success': props<{ supplier: Supplier }>(),
    'Update Supplier Failure': props<{ error: string }>(),

    'Activate Supplier': props<{ id: string }>(),
    'Activate Supplier Success': props<{ id: string }>(),
    'Activate Supplier Failure': props<{ error: string }>(),

    'Deactivate Supplier': props<{ id: string }>(),
    'Deactivate Supplier Success': props<{ id: string }>(),
    'Deactivate Supplier Failure': props<{ error: string }>(),

    'Clear Selected Supplier': emptyProps(),
  },
});
