export interface CreateCustomerRequest {
  name: string;
  document: string;
  phone?: string | null;
  loyaltyTierId?: string | null;
}
