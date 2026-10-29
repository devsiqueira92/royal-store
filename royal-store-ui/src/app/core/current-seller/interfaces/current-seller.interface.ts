/** The signed-in user's seller profile, as returned by `GET /api/sellers/me`. */
export interface CurrentSeller {
  fullName: string;
  phone: string | null;
  email: string | null;
  isActive: boolean;
  // ISO 8601 date-time (DateTimeOffset on the API).
  createdAt: string;
  tenantName: string;
}
