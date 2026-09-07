import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { CreateServiceRequest } from '../interfaces/create-service-request.interface';
import { UpdateServiceRequest } from '../interfaces/update-service-request.interface';
import { ServiceActions } from '../store/service.actions';
import {
  selectAllProducts,
  selectSelectedProduct,
  selectIsLoading,
  selectError,
  productVariants,
} from '../store/product.selectors';

/**
 * Write facade for the Service kind (`api/services`). A Service is stored as
 * a `Product` (Type = Service), so reads share ProductFacade's selectors —
 * this facade only adds the create/edit dispatch surface.
 */
@Injectable({
  providedIn: 'root',
})
export class ServiceFacade {
  private store = inject(Store);

  products$ = this.store.select(selectAllProducts);
  selectedProduct$ = this.store.select(selectSelectedProduct);
  productVariants$ = this.store.select(productVariants);
  isLoading$ = this.store.select(selectIsLoading);
  error$ = this.store.select(selectError);

  createService(request: CreateServiceRequest): void {
    this.store.dispatch(ServiceActions.createService({ request }));
  }

  saveServiceEdit(id: string, request: UpdateServiceRequest): void {
    this.store.dispatch(ServiceActions.saveServiceEdit({ id, request }));
  }
}
