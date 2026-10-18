export interface CustomerAddressRequest {
  zipCode: string | null;
  street: string | null;
  number: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  complement: string | null;
  additionalInfo: string | null;
  isDefaultDelivery: boolean;
}

/** A null `id` adds a new address; otherwise that address is fully replaced. */
export interface CustomerAddressChangeRequest extends CustomerAddressRequest {
  id: string | null;
}
