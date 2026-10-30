import { createFeature, createReducer, on } from '@ngrx/store';
import { Supplier } from '../interfaces/supplier.interface';
import { SupplierActions } from './supplier.actions';

export interface SupplierState {
  suppliers: Supplier[];
  selectedSupplier: Supplier | null;
  isLoading: boolean;
  error: string | null;
}

export const initialSupplierState: SupplierState = {
  suppliers: [],
  selectedSupplier: null,
  isLoading: false,
  error: null,
};

const upsert = (state: SupplierState, supplier: Supplier): SupplierState => {
  const exists = state.suppliers.some(s => s.id === supplier.id);
  return {
    ...state,
    suppliers: exists
      ? state.suppliers.map(s => (s.id === supplier.id ? supplier : s))
      : [...state.suppliers, supplier],
    selectedSupplier:
      state.selectedSupplier?.id === supplier.id
        ? supplier
        : state.selectedSupplier,
    isLoading: false,
  };
};

const setActive = (
  state: SupplierState,
  id: string,
  isActive: boolean,
): SupplierState => ({
  ...state,
  suppliers: state.suppliers.map(s => (s.id === id ? { ...s, isActive } : s)),
  selectedSupplier:
    state.selectedSupplier?.id === id
      ? { ...state.selectedSupplier, isActive }
      : state.selectedSupplier,
  isLoading: false,
});

export const supplierFeature = createFeature({
  name: 'supplier',
  reducer: createReducer(
    initialSupplierState,
    on(
      SupplierActions.loadSuppliers,
      SupplierActions.loadSupplier,
      SupplierActions.createSupplier,
      SupplierActions.updateSupplier,
      SupplierActions.activateSupplier,
      SupplierActions.deactivateSupplier,
      (state): SupplierState => ({
        ...state,
        isLoading: true,
        error: null,
      }),
    ),
    on(
      SupplierActions.loadSuppliersFailure,
      SupplierActions.loadSupplierFailure,
      SupplierActions.createSupplierFailure,
      SupplierActions.updateSupplierFailure,
      SupplierActions.activateSupplierFailure,
      SupplierActions.deactivateSupplierFailure,
      (state, { error }): SupplierState => ({
        ...state,
        isLoading: false,
        error,
      }),
    ),
    on(
      SupplierActions.loadSuppliersSuccess,
      (state, { suppliers }): SupplierState => ({
        ...state,
        suppliers,
        isLoading: false,
      }),
    ),
    on(
      SupplierActions.loadSupplierSuccess,
      (state, { supplier }): SupplierState => ({
        ...state,
        selectedSupplier: supplier,
        isLoading: false,
      }),
    ),
    on(
      SupplierActions.createSupplierSuccess,
      SupplierActions.updateSupplierSuccess,
      (state, { supplier }) => upsert(state, supplier),
    ),
    on(SupplierActions.activateSupplierSuccess, (state, { id }) =>
      setActive(state, id, true),
    ),
    on(SupplierActions.deactivateSupplierSuccess, (state, { id }) =>
      setActive(state, id, false),
    ),
    on(
      SupplierActions.clearSelectedSupplier,
      (state): SupplierState => ({
        ...state,
        selectedSupplier: null,
      }),
    ),
  ),
});
