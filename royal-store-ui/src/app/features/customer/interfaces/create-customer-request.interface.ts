import { CustomerAddressRequest } from './customer-address-request.interface';

export interface CreateCustomerRequest {
  name: string;
  document: string;
  phone?: string | null;
  loyaltyTierId?: string | null;
  addresses?: CustomerAddressRequest[];
}
