import { CustomerAddressChangeRequest } from './customer-address-request.interface';

export interface UpdateCustomerRequest {
  name: string;
  phone?: string | null;
  loyaltyTierId?: string | null;
  // Addresses left out stay as they are on the API.
  addresses?: CustomerAddressChangeRequest[];
}
