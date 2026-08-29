export interface SellerPermissions {
  sellerId: string;
  userId: string;
  role: string;
  grants: string[];
  effectivePermissions: string[];
}
