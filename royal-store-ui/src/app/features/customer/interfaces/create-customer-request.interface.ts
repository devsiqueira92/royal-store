import { CustomerAddressRequest } from './customer-address-request.interface';

export interface CreateCustomerRequest {
  name: string;
  document?: string | null;
  phone?: string | null;
  birthDate?: string | null;
  loyaltyTierId?: string | null;
  addresses?: CustomerAddressRequest[];
}
