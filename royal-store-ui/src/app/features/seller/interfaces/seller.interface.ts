export interface Seller {
  id: string;
  tenantId: string;
  fullName: string;
  phone: string;
  cpf: string;
  isActive: boolean;
  userId?: string;
}
